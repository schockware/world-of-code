*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# csharp-lib design: WX-QTY (core)

Answers `domains/weather/specs/core/qty.md`. This is a design, so it says how C# does it. What must be true stays in the spec.

## Model

A `Quantity` is a **sealed record class**, deliberately not a struct. Being a reference type is what lets a nullable reference answer "was the element reported at all?".

```csharp
public sealed record Quantity(decimal? Value, string Unit);

public sealed record Observation
{
    public required Station Station { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public Quantity? Temperature { get; init; }
    public Quantity? Dewpoint { get; init; }
    // ... one property per element in observation.json
}

public enum AnswerState { NotReported, ReportedWithoutValue, Reported }
```

| Answer state | C# form |
|---|---|
| Not reported | the element property is `null` |
| Reported without a value | a `Quantity` whose `Value` is `null` |
| Reported | a `Quantity` whose `Value` has a value |

Nullable reference types are enabled library-wide, so the compiler makes callers deal with the difference between the first and second rows. Immutability comes from `init` and records, so a `Quantity` cannot be mutated into another state after the fact.

`AnswerState` and `Observation.Classify(string element)` exist to serve the conformance operation `classify`. Callers are expected to use the properties directly.

## Numeric type

`Value` is `decimal?`. `System.Text.Json` reads JSON numbers into `decimal` without going through binary floating point, so `21.15` stays `21.15`. Trade-offs:

- Decimal digits survive exactly, up to about 28 significant digits. That comfortably covers surface weather elements.
- Values with a very large magnitude or exponent (`1e30`) do not fit in `decimal`. The library treats an unrepresentable number as `qty.value-not-numeric`. Recorded as an open item, since the spec has no reason for it.

## Parsing and rejection

Rejection is modelled with C#'s `Parse`/`TryParse` convention, so the culture's two expectations both hold:

```csharp
public sealed record Rejection(string Reason);   // e.g. "qty.value-missing"

public static class QuantityReader
{
    public static bool TryRead(JsonElement json, IUnitValidator units,
        [NotNullWhen(true)] out Quantity? quantity,
        [NotNullWhen(false)] out Rejection? rejection);

    public static Quantity Read(JsonElement json, IUnitValidator units); // throws WeatherValidationException
}
```

- Expected bad input (a malformed quantity from a feed) goes through `TryRead`, since it is not exceptional.
- `Read` throws `WeatherValidationException`, which carries the `Rejection`, for callers who prefer to fail fast.
- The reason strings from the spec are `const string` values in a static class, so they are not scattered as literals.

`QuantityReader` inspects the JSON object's members directly, since deserializing straight to the record would treat a missing `value` and `"value": null` the same. That is what WX-QTY-007 forbids.

## Traceability

| Requirement | Design decision |
|---|---|
| WX-QTY-001 | Three forms in the table above. The compiler distinguishes `null` from a `Quantity` with a null `Value`. |
| WX-QTY-002 | Element property absent means `null`. `Classify` returns `NotReported`. |
| WX-QTY-003 | `Quantity` with `Value == null` keeps its `Unit`, and classifies as `ReportedWithoutValue`. |
| WX-QTY-004 | `Value.HasValue`, including `0m` and negatives, classifies as `Reported`. |
| WX-QTY-005 | No API converts between states. There is no implicit conversion from `Quantity?` to `decimal`. |
| WX-QTY-006 | `decimal?` with no rounding anywhere in the library. Writing uses `Utf8JsonWriter.WriteNumberValue(decimal)`. |
| WX-QTY-007 | `QuantityReader` checks that the `value` member exists before reading it. |
| WX-QTY-008 | `QuantityReader` checks that the `unit` member exists, regardless of `value`. |
| WX-QTY-009 | Non-number and non-null `value` kinds are rejected, as are numbers that do not fit `decimal`. |
| WX-QTY-010 | `QuantityReader` enumerates members and rejects any other than `value` and `unit`. |

## Population handling

None. The library takes a `JsonElement` and returns a model or a `Rejection`. Formatting, rounding and locale rendering are not in this library. A consumer such as `dotnet-mvc` decides how each `AnswerState` looks to its people.

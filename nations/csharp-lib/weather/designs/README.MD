*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# csharp-lib weather designs

Designs for the weather domain specs, in C# culture. Specs live in `domains/weather/specs`, and each design cites the requirement IDs it answers.

| Spec set | Area | Design |
|---|---|---|
| core | QTY | `core/qty.md` |
| core | UNIT | `core/unit.md` |

## Project shape

```
src/
  WorldOfCode.Weather.slnx
  Directory.Build.props        net10.0, Nullable, TreatWarningsAsErrors, analyzers
  .editorconfig
  WorldOfCode.Weather/         the class library
  WorldOfCode.Weather.Tests/   xUnit project
```

- The test project links the conformance vectors from `domains/weather/specs/core/conformance/*.json` into its output, and turns each vector into an xUnit theory case named by file and vector `id`. Passing vectors is the scoreboard.
- The library links the shared `ucum-essence.xml` as an embedded resource.
- The library stays population-free. It depends on `System.Text.Json` and `System.Xml.Linq` only, and on nothing HTTP, UI or CLI shaped.
- Run with `dotnet test` from `src/`.

## Vector operations

| Operation | C# call |
|---|---|
| `validate` | `QuantityReader.TryRead` |
| `classify` | `ObservationReader.TryRead`, then `Observation.Classify(element)` |
| `roundtrip` | Read, then write with `WeatherJsonWriter`, then compare as JSON values (objects in any order, numbers by value) |

## Status

All 33 core vectors pass, and every one of the 312 atoms in the UCUM unit list is accepted by `UcumUnitValidator`.

## Open items

- **Provisional reasons.** `qty.not-object` and the `obs.*` reasons (`obs.not-object`, `obs.station-missing`, `obs.observed-at-missing`, `obs.observed-at-invalid`, `obs.element-null`, `obs.unknown-member`) exist so the reader has something to return, but no spec requirement backs them yet. They are listed in `Reasons` under a "Provisional" comment. WX-OBS and WX-TIME should define or replace them.
- **Timestamps.** `ObservedAt` is a `DateTimeOffset`, so an offset of `+00:00` is written back as `Z`, and trailing zeros of fractional seconds are dropped. That is TIME's territory, and the future spec should decide whether the source's text must survive.
- **Numbers outside `decimal` range** are rejected as `qty.value-not-numeric`, which the spec does not ask for. Decide whether the spec should say something about representable range.
- **No DI registration extension** yet (see `core/unit.md`).

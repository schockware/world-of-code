*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# golang-lib design: WX-QTY (core)

Answers `domains/weather/specs/core/qty.md`. This is a design, so it says how Go does it. What must be true stays in the spec.

## Model

Plain structs. Pointers carry the "was it answered?" distinction, which is the idiomatic Go way to say "may be absent".

```go
type Quantity struct {
    Value *float64 // nil: reported without a value
    Unit  string
}

type Observation struct {
    Station    Station
    ObservedAt time.Time
    Temperature *Quantity // nil: not reported
    Dewpoint    *Quantity
    // ... one field per element in observation.json
}

type AnswerState int

const (
    NotReported AnswerState = iota // the zero value: nothing was answered
    ReportedWithoutValue
    Reported
)
```

| Answer state | Go form |
|---|---|
| Not reported | the element field is `nil` |
| Reported without a value | a non-nil `*Quantity` whose `Value` is `nil` |
| Reported | a non-nil `*Quantity` whose `Value` is non-nil |

`NotReported` is the zero value of `AnswerState`, so an unset state means what it should. A `Quantity` on its own cannot be "not reported", since it is a value. Only the pointer on an `Observation` can be, which is why the element fields are pointers.

`Observation.State(e Element) AnswerState` exists to serve the conformance operation `classify`. `Element` is a typed string constant set (`Temperature`, `Dewpoint`, and so on).

## Numeric type

`Value` is `*float64`, the idiomatic choice, with encoding via `strconv` shortest-round-trip formatting. Trade-offs:

- `21.15`, `1013.2512345` and `123456789` round-trip exactly, since Go writes the shortest decimal that parses back to the same `float64`.
- Decimal text with more than about 15 to 17 significant digits does not survive. This differs from a decimal type, and is worth a vector if the spec ever needs it.
- `json.Number` would preserve source text exactly. It is not used because it makes every consumer parse strings. Recorded as an open item.

## Absent versus null

`encoding/json` cannot tell a missing `value` from `"value": null`. Both leave the pointer `nil`, and WX-QTY-007 needs them apart. So `Quantity` does not rely on default decoding. `ParseQuantity` decodes into `map[string]json.RawMessage` and checks the members itself:

```go
func ParseQuantity(data []byte, units UnitValidator) (Quantity, error)
```

It checks for the `value` and `unit` members, rejects any other member, then decodes `value` on its own (a JSON `null` becomes `Value == nil`, a number becomes a `float64`, anything else is rejected).

## Rejection

Errors are values. `ParseQuantity` returns an error and never panics.

```go
type Reason string

const (
    ReasonValueMissing    Reason = "qty.value-missing"
    ReasonUnitMissing     Reason = "qty.unit-missing"
    ReasonValueNotNumeric Reason = "qty.value-not-numeric"
    ReasonUnknownMember   Reason = "qty.unknown-member"
    // unit reasons are in unit.md
)

type RejectionError struct{ Reason Reason }

func (e *RejectionError) Error() string { return string(e.Reason) }
```

Callers test with `errors.As(err, &rej)` and read `rej.Reason`. There are no sentinel errors per reason, since the reason is data carried by one error type.

## Traceability

| Requirement | Design decision |
|---|---|
| WX-QTY-001 | Three forms in the table above. `nil` pointer versus non-nil pointer to a `Quantity` with a `nil` `Value`. |
| WX-QTY-002 | Element field is `nil`. `State` returns `NotReported`. |
| WX-QTY-003 | Non-nil `*Quantity` with `Value == nil` keeps its `Unit`. `State` returns `ReportedWithoutValue`. |
| WX-QTY-004 | Non-nil `Value`, including `0` and negatives. `State` returns `Reported`. A pointer means zero is never confused with absence. |
| WX-QTY-005 | No function converts between states. `Value` is never dereferenced into a default. |
| WX-QTY-006 | `float64` with shortest round-trip formatting on write. No rounding anywhere. |
| WX-QTY-007 | `ParseQuantity` checks the `value` key exists in the raw map. |
| WX-QTY-008 | `ParseQuantity` checks the `unit` key exists, regardless of `value`. |
| WX-QTY-009 | Raw `value` that is not a JSON number or `null` returns `ReasonValueNotNumeric`. |
| WX-QTY-010 | Any key other than `value` and `unit` returns `ReasonUnknownMember`. |

## Population handling

None. The package takes bytes and returns structs or an error. Formatting, rounding and locale rendering belong to whoever calls it, such as `golang-webapi`.

*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# rust-lib design: WX-QTY (core)

Answers `domains/weather/specs/core/qty.md`. This is a design, so it says how Rust does it. What must be true stays in the spec.

## Model

Plain structs with public fields, and `Option` carries "may be absent" at both levels. There is no null in the model.

```rust
pub struct Quantity {
    pub value: Option<f64>, // None: reported without a value
    pub unit: String,
}

pub struct Observation {
    pub station: Station,
    pub observed_at: ObservedAt,
    pub temperature: Option<Quantity>, // None: not reported
    pub dewpoint: Option<Quantity>,
    // ... one field per element in observation.json
}

pub enum AnswerState { NotReported, ReportedWithoutValue, Reported }
```

| Answer state | Rust form |
|---|---|
| Not reported | the element field is `None` |
| Reported without a value | `Some(Quantity { value: None, .. })` |
| Reported | `Some(Quantity { value: Some(_), .. })` |

The two `Option` layers are the three states, and a `match` on them has to name all three, so the compiler makes a caller handle each. `Observation::state(Element) -> AnswerState` serves the conformance operation `classify`. `Element` is an enum (`Element::ALL`, `name()`, `from_name()`), so a misspelled element is a compile error and not a silent `NotReported`. `Observation::get` and `set` take an `Element` for code that walks every element.

A `Quantity` on its own cannot be "not reported", since it is a value. Only the `Option` on an `Observation` can be. Fields are public and there is no invariant for a constructor to guard. The parsers do the validating, and `Quantity::new` builds without it.

## Numeric type

`value` is `Option<f64>`, the idiomatic choice. There is no unit-of-measure type or newtype for the number, because the domain forbids conversion and a newtype would add ceremony without adding a rule.

- `21.15`, `1013.2512345` and `123456789` round-trip exactly. `serde_json` writes the shortest decimal that parses back to the same `f64`.
- A whole number that `f64` holds exactly (below 2^53) is written without a fraction, so `12` stays `12` and not `12.0`. Negative zero is written as a float so its sign survives.
- Decimal text with more than about 15 to 17 significant digits does not survive. `serde_json`'s `arbitrary_precision` feature, or a decimal crate, would fix that at the price of a dependency and a heavier type. Recorded as an open item in `../README.MD`.
- A `NaN` or infinite `value` cannot be written as JSON. Reading never produces one, but the field is public, so `to_value` and `to_json` return `Err(Rejection)` with `qty.value-not-numeric` and never write `null` in its place, which would collapse two states (WX-QTY-005).

## Absent versus null

A deserializer with derive (`#[derive(Deserialize)]`) cannot tell a missing `value` from `"value": null`. Both give `None`, and WX-QTY-007 needs them apart. So neither type derives `Deserialize`. `Quantity::from_value` takes a `serde_json::Value`, matches on `Value::Object`, and asks the map for its keys with `get("value")` and `get("unit")`. A missing key and a `null` are different results there. Text goes through `Quantity::parse`, which is `serde_json::from_str::<Value>` and then `from_value`, and `Observation::parse` does the same. `serde_json` is used as a syntax reader only.

```rust
pub fn from_value(json: &Value, units: &dyn UnitValidator) -> Result<Quantity, Rejection>
```

It rejects any member other than `value` and `unit`, checks that both exist, then reads `value` (`null` becomes `None`, a number becomes `Some(f64)`, anything else is rejected). The unit checks are in `unit.md`.

`Observation::from_value` treats an element whose JSON is `null` as a fault (`obs.element-null`), since "not reported" has one form, which is omission. Writing omits every `None` element, so a round trip never turns omission into `null` (WX-QTY-005).

## Observation time

`observed_at` is an `ObservedAt`, a newtype over the RFC 3339 `String` that the source sent. It can only be built by `ObservedAt::new`, which validates the `date-time` form (calendar and clock ranges included, leap seconds allowed) and keeps the text unchanged.

| Choice | For | Against |
|---|---|---|
| `ObservedAt(String)` (used) | Faithful: offset and fractional digits come back exactly. No dependency. | Not an instant, so no comparison or arithmetic until a caller converts it. The RFC 3339 check is hand-written. |
| A time crate type (`time`, `chrono`, `jiff`) | Real instants and arithmetic. | A dependency the library does not otherwise need, and an instant does not remember its source offset or digits, which is a faithfulness question that belongs to TIME. |
| `std::time::SystemTime` | No dependency. | Cannot parse or format RFC 3339, and loses the offset. |

The spec set has no time requirement yet, so the least committal choice was taken. Go and C# parse to a time type, and Go's writer rewrites `+00:00` as `Z`. This one does not.

## Rejection

Errors are values, and `Result` is how a caller sees them. Nothing on the read path panics or indexes without a bounds check.

```rust
pub enum Reason {
    QtyValueMissing,   // "qty.value-missing"
    QtyUnitMissing,    // "qty.unit-missing"
    QtyValueNotNumeric,// "qty.value-not-numeric"
    QtyUnknownMember,  // "qty.unknown-member"
    // unit reasons are in unit.md, provisional ones are in rejection.rs
}
impl Reason { pub const fn as_str(self) -> &'static str }

pub struct Rejection { /* reason */ }   // Display + std::error::Error
pub enum ParseError { Malformed(serde_json::Error), Rejected(Rejection) }
```

- `Reason` is a closed enum, so a `match` over it is checked for exhaustiveness. `as_str()` returns the exact spec code, and `Display` prints the same. There is no string comparison in the library.
- `Rejection` carries the `Reason`, implements `Display` and `std::error::Error`, and has `From<Reason>`, so `?` and `.ok_or(Reason::X)?` read cleanly.
- Text that is not JSON is not a spec rejection. It comes back as `ParseError::Malformed` with the decoder's error, distinct from `ParseError::Rejected`. Callers holding a `Value` use `from_value` and see only `Rejection`.
- The provisional codes (`qty.not-object`, `obs.*`) are the same ones the other nations use.

## Traceability

| Requirement | Design decision |
|---|---|
| WX-QTY-001 | `Option<Quantity>` and `Option<f64>` give the three forms in the table above. `AnswerState` names them. |
| WX-QTY-002 | Element field is `None`. `state` returns `NotReported`. |
| WX-QTY-003 | `Some(Quantity)` with `value: None` keeps its `unit`. `state` returns `ReportedWithoutValue`. Written as `"value": null`. |
| WX-QTY-004 | `value: Some(_)`, including `0.0` and negatives. `state` returns `Reported`. `Option` means zero is never confused with absence. |
| WX-QTY-005 | No function converts between states. `value` is never defaulted with `unwrap_or`, and writing omits `None` elements and writes `null` for a `None` value. |
| WX-QTY-006 | `f64` with shortest round-trip formatting on write. No rounding anywhere. |
| WX-QTY-007 | `from_value` asks the JSON object for the key `value`. Missing is `QtyValueMissing`. |
| WX-QTY-008 | Same for `unit`, regardless of `value`. Missing is `QtyUnitMissing`. |
| WX-QTY-009 | A `value` that is not `Null` or `Number` is `QtyValueNotNumeric`. |
| WX-QTY-010 | Any key other than `value` and `unit` is `QtyUnknownMember`. This is checked first. |

## Population handling

None. The crate takes text or a `Value` and returns structs or a rejection. Formatting, rounding and locale rendering belong to whoever calls it.

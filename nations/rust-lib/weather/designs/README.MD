*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# rust-lib weather designs

Designs for the weather domain specs, in Rust culture. Specs live in `domains/weather/specs`, and each design cites the requirement IDs it answers.

| Spec set | Area | Design |
|---|---|---|
| core | QTY | `core/qty.md` |
| core | UNIT | `core/unit.md` |

## Crate shape

The crate root is `nations/rust-lib/weather`, one level up from `designs/`.

```
Cargo.toml            crate `weather`, edition 2024, one dependency: serde_json
src/lib.rs            re-exports, parse_quantity, parse_observation
src/quantity.rs       Quantity
src/observation.rs    Observation, Element, Station, ObservedAt, AnswerState
src/rejection.rs      Reason, Rejection, ParseError
src/unit.rs           UnitValidator (the seam) and the order of unit checks
src/ucum/mod.rs       the UCUM parser, and its unit tests
src/ucum/atoms_gen.rs generated atom and prefix data, checked in
examples/gen_ucum.rs  the generator: reads ucum-essence.xml, writes atoms_gen.rs
tests/conformance.rs  runs every shared vector
```

- `cargo fmt` clean, and `cargo clippy --all-targets -- -D warnings` clean. `unsafe_code` is forbidden and `missing_docs` warns, so a public item without a doc comment fails the clippy run.
- The library stays population-free. The only dependency is `serde_json`, used as a JSON value tree and never for derive-based decoding (see `core/qty.md`).
- Run with `cargo test` from the crate root. It needs no network after the first fetch of `serde_json` and no generation step. Regenerate the unit data with `cargo run --example gen_ucum`.
- `tests/conformance.rs` loads `domains/weather/specs/core/conformance/*.json` through `env!("CARGO_MANIFEST_DIR")`. It runs every vector, collects failures as `file/id: reason`, and fails once with the whole list, so one bad vector does not hide the rest. Passing vectors is the scoreboard.

## Vector operations

| Operation | Rust call |
|---|---|
| `validate` | `Quantity::from_value`, comparing `Rejection::reason().as_str()` with the vector |
| `classify` | `parse_observation`, then `Observation::state(element)` |
| `roundtrip` | `parse_quantity` or `parse_observation`, then `to_json`, then compare as JSON values (objects in any order, numbers by value) |

## Status

All 33 core vectors pass, and every one of the 312 atoms in the UCUM unit list is accepted by `ucum::is_valid`. The unit tests in `src/ucum/mod.rs` mirror the Go nation's table.

## Open items

- **Provisional reasons.** `Reason::QtyNotObject` and the `Reason::Obs*` variants exist so the parsers have something to return, but no spec requirement backs them yet. They are marked provisional in `rejection.rs`. WX-OBS and WX-TIME should define or replace them.
- **Timestamps.** `ObservedAt` keeps validated RFC 3339 text and does no time arithmetic, so offsets and fractional digits survive. This differs from Go, which parses to `time.Time` and writes `+00:00` back as `Z`. Whether the domain wants an instant type is TIME's territory. See `core/qty.md`.
- **Whether `f64` is enough.** Decimal text with more than about 15 to 17 significant digits does not survive. `serde_json`'s `arbitrary_precision` feature would keep source text if a vector ever needs it.
- **Malformed JSON.** The specs say nothing about text that is not JSON at all. `ParseError::Malformed` carries the decoder's error, and it is not a `Reason`. A number too large for `f64` (such as `1e999`) is refused by the decoder in this category rather than as `qty.value-not-numeric`, unlike Go.
- **Member order.** With several faults in one `Observation`, members are checked in name order, so the reported reason is stable but is not spec-defined. Go's order is unspecified.
- **Toolchain.** No `cargo deny`, `cargo audit` or MSRV job is configured. `rust-version` is set to 1.85, the first release with edition 2024, and has not been tested against that exact release.

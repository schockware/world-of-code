*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# rust-lib design: WX-UNIT (core)

Answers `domains/weather/specs/core/unit.md`.

## Validation seam

A one-method trait, defined in the crate that consumes it:

```rust
pub trait UnitValidator {
    fn is_valid(&self, unit: &str) -> bool;
}

impl<F: Fn(&str) -> bool> UnitValidator for F { /* calls the closure */ }
```

`Quantity::from_value`, `Quantity::parse` and the `Observation` equivalents take `&dyn UnitValidator`. There is no container and no global. A caller builds the validator and passes it in. A test passes a closure, using the blanket impl, so no adapter type is needed (Go needs `UnitValidatorFunc`). The real implementation is in the `ucum` module (`ucum::Validator`, a zero-size unit struct), so the seam is a module boundary as well as a trait.

The unit reasons are variants of the `Reason` enum from `qty.md`: `Reason::UnitNotUcum` (`unit.not-ucum`) and `Reason::UnitNamespaced` (`unit.namespaced`).

## Order of checks

`unit::check_unit` runs the checks in this order:

1. The unit is a non-empty string. An empty string, or a JSON value that is not a string, is `unit.not-ucum` (WX-UNIT-004).
2. A namespace prefix (`wmo:`, `wmoUnit:`, `nwsUnit:`, `uc:`, held in a `const` array) is `unit.namespaced`. This runs **before** the validator, so `wmoUnit:degC` yields `unit.namespaced` (WX-UNIT-003).
3. `UnitValidator::is_valid` runs last, yielding `unit.not-ucum`.

The unit is stored exactly as read. It is copied into an owned `String` once, and nothing in the path calls `to_lowercase`, `trim` or similar (WX-UNIT-002). Ownership keeps it that way: a `Quantity` owns its unit, and a caller changes it only through a `&mut Quantity` they hold, never behind another holder's back. There is no conversion API anywhere in the crate.

## The UCUM question (settled)

`ucum::is_valid` is a small hand-written recursive descent parser over the UCUM case-sensitive grammar (documented in `domains/weather/standards/ucum/README.MD`). Validation only needs a yes or no, so no conversion machinery is needed and no crate was adopted. It follows the Go parser rule for rule:

- an optional leading `/`, then a term of components joined by `.` or `/`
- a component is a parenthesized term, an annotation alone, a plain digit run (a factor), or an atom with an optional signed exponent and an optional annotation
- `10*` and `10^` are atoms, matched before the plain digit rule
- a prefix applies only to atoms marked metric, and the SI base units all take prefixes
- an exponent applies to a unit, so `(m/s)2` is rejected
- an annotation is `{`, printable ASCII other than braces, then `}`

The parser walks the input as bytes over a `&str` with an index. Every delimiter it tests is ASCII, so each slice it takes falls on a character boundary and non-ASCII input (`mé`) is rejected without a panic. A test feeds it odd input to keep that true.

**Atom and prefix data.** `src/ucum/atoms_gen.rs` holds two `static` tables (atoms sorted for binary search, with a metric flag, and the prefixes). `examples/gen_ucum.rs` writes it from the shared `ucum-essence.xml` (`cargo run --example gen_ucum`). The file is checked in, so `cargo test` needs no generation step and no network. Reasons for this route:

- `include_str!` could embed the XML from outside the crate, but the file would then have to be scanned at run time on every start. A generated `static` costs nothing at run time.
- A `build.rs` would put the generator in every consumer's build and make the source path a build-time requirement for anyone who depends on the crate from a registry. An example keeps generation a maintainer's step.
- The generator scans the start tags itself, since the file is machine-written, one tag per unit or prefix. That avoids an XML crate. It reads the file as UTF-8, because `encoding="ascii"` is a subset. It refuses an `&` in a code rather than guessing.

The module's unit tests check that every atom in the unit list is accepted (312 today), plus a valid and an invalid table that mirrors the Go nation's.

Not yet checked against an outside UCUM implementation (see the note in the spec's `unit.md`).

## Traceability

| Requirement | Design decision |
|---|---|
| WX-UNIT-001 | `UnitValidator::is_valid` as the last check, implemented by `ucum::Validator`. |
| WX-UNIT-002 | Unit stored exactly as read in an owned `String`. No normalization, and no conversion function exists. |
| WX-UNIT-003 | The prefix check runs before the validator and takes precedence. |
| WX-UNIT-004 | Empty string rejected with `Reason::UnitNotUcum` before the validator is called. |

## Population handling

None. Converting units for a locale is left to consumers, and the crate exports no conversion function.

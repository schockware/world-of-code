*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# golang-lib design: WX-UNIT (core)

Answers `domains/weather/specs/core/unit.md`.

## Validation seam

A one-method interface, defined in the package that uses it (the consumer side), as Go prefers:

```go
// UnitValidator reports whether unit is a valid UCUM code (case-sensitive form).
type UnitValidator interface {
    Valid(unit string) bool
}
```

`ParseQuantity` takes a `UnitValidator` as an argument. There is no container and no global registry. A caller builds the validator and passes it in. A test passes a function-backed fake, using an adapter type with a `Valid` method:

```go
type UnitValidatorFunc func(string) bool

func (f UnitValidatorFunc) Valid(u string) bool { return f(u) }
```

The real implementation lives in a separate package (`ucum`), so the seam is a package boundary as well as an interface.

The unit reasons join the `Reason` constants from `qty.md`:

```go
const (
    ReasonNotUCUM    Reason = "unit.not-ucum"
    ReasonNamespaced Reason = "unit.namespaced"
)
```

## Order of checks

1. The unit is a non-empty string (`unit.not-ucum` for empty, WX-UNIT-004).
2. The namespace prefix check runs **before** the validator, so `wmoUnit:degC` yields `unit.namespaced` (WX-UNIT-003). The prefixes are a small, fixed list held as a package-level slice.
3. `UnitValidator.Valid` runs last, yielding `unit.not-ucum`.

The unit is stored exactly as read. Go strings are immutable byte sequences, and nothing in the path calls `strings.ToLower`, `strings.TrimSpace` or similar (WX-UNIT-002). `Quantity` is passed by value, so a caller cannot change a unit out from under another holder of the same quantity.

## The UCUM question (settled)

`ucum.Valid` is a small hand-written recursive descent parser over the UCUM case-sensitive grammar (documented in `domains/weather/standards/ucum/README.MD`). Validation only needs a yes or no, so no conversion machinery is needed and no module was adopted.

- The atom and prefix lists are generated into `ucum/atoms_gen.go` by `ucum/gen.go` (`go generate ./...`) from the shared `ucum-essence.xml`. `//go:embed` cannot reach outside the module, so generating Go source is the idiomatic route. The generated file is committed, so a build needs no generation step.
- A prefix applies only to atoms marked metric, and the SI base units all take prefixes.
- The package's own tests check that every atom in the unit list is accepted, plus compound forms and invalid ones.

Not yet checked against an outside UCUM implementation (see the note in the spec's `unit.md`).

## Traceability

| Requirement | Design decision |
|---|---|
| WX-UNIT-001 | `UnitValidator.Valid` as the last check, implemented by `ucum.Validator`. |
| WX-UNIT-002 | Unit stored exactly as read. No normalization and no conversion function exists. |
| WX-UNIT-003 | Prefix check runs before the validator and takes precedence. |
| WX-UNIT-004 | Empty string rejected with `ReasonNotUCUM` before the validator is called. |

## Population handling

None. Converting units for a locale is left to consumers, and the package exports no conversion function.

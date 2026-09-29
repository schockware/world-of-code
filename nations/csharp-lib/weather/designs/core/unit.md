*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# csharp-lib design: WX-UNIT (core)

Answers `domains/weather/specs/core/unit.md`.

## Validation seam

Unit validation sits behind an interface, injected into the reader. This is the .NET way of isolating a dependency with a real design cost (a UCUM parser), and it lets the tests swap in a fake.

```csharp
public interface IUnitValidator
{
    bool IsValid(string unit);   // true when unit is valid UCUM (case-sensitive form)
}
```

`QuantityReader` depends only on this interface. The default implementation, `UcumUnitValidator`, is constructed directly. A DI registration extension (`AddWeather()`) is deliberately left out for now, because the library has no host and no container. A consuming nation such as `aspnet-webapi` can register it with one line.

## Order of checks

1. The unit is a string, and not empty (`unit.not-ucum` for empty, WX-UNIT-004).
2. The namespace prefix check runs **before** UCUM validation, so `wmoUnit:degC` yields `unit.namespaced` (WX-UNIT-003). It reuses the pattern from `quantity.json`.
3. `IUnitValidator.IsValid` runs last, yielding `unit.not-ucum`.

The unit string is stored exactly as read. There is no `ToLowerInvariant`, no trimming and no canonicalization anywhere in the path (WX-UNIT-002), and `Quantity` is an immutable record, so `Value` and `Unit` cannot drift apart.

## The UCUM question (settled)

`UcumUnitValidator` is a small hand-written recursive descent parser over the UCUM case-sensitive grammar (documented in `domains/weather/standards/ucum/README.MD`). Validation only needs a yes or no, so no conversion machinery is needed and no package was adopted.

- The atom and prefix lists come from the shared `ucum-essence.xml`, linked into the assembly as an embedded resource and parsed once on first use.
- A prefix applies only to atoms marked metric, and the SI base units all take prefixes.
- The validator's own tests check that every atom in the unit list is accepted, plus compound forms (`10*3/uL`, `mm[Hg]`, `kg.m/s2`, annotations) and invalid ones.

Not yet checked against an outside UCUM implementation (see the note in the spec's `unit.md`).

## Traceability

| Requirement | Design decision |
|---|---|
| WX-UNIT-001 | `IUnitValidator.IsValid` as the last check, implemented by `UcumUnitValidator`. |
| WX-UNIT-002 | Unit stored exactly as read in an immutable record. No normalization and no conversion API exists. |
| WX-UNIT-003 | Prefix check runs before UCUM validation and takes precedence. |
| WX-UNIT-004 | Empty string rejected with `unit.not-ucum` before the validator is called. |

## Population handling

None. Converting units for a locale is left to consumers, and this library offers no conversion API to be tempted by. That is part of the design, not an omission.

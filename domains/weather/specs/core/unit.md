*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# WX-UNIT: Units (core)

Covers `Quantity.unit` (`../../contracts/domain/schemas/quantity.json`). The domain validates the form of a unit and carries it unchanged. It does **not** convert between units. Conversion is a population concern.

Rationale for the namespace rule comes from `../../contracts/domain/README.md` ("Left out, and why"). NWS `wmoUnit:` and `nwsUnit:` values cannot be converted without a mapping table the project does not have yet.

## Operations

Vectors in `conformance/unit.json` use these operations, defined in `qty.md`: `validate` and `roundtrip`. Each rejection vector contains exactly one fault, except where WX-UNIT-003 sets a precedence.

## Requirements

### WX-UNIT-001
**Status:** Draft
**Level:** MUST
A `unit` MUST be a valid UCUM code, in the case-sensitive form. Anything else MUST be rejected with reason `unit.not-ucum`.
**Rationale:** UCUM is the one unit system both the US (NWS) and international (SensorThings) sources can express, and it is machine-parseable.
**Contract:** `quantity.json`
**Vectors:** `unit.json#accept-*`, `unit.json#reject-not-ucum-*`

### WX-UNIT-002
**Status:** Draft
**Level:** MUST
A `unit` MUST be carried exactly as the source reported it. An implementation MUST NOT convert it to another unit, and MUST NOT normalize its spelling or case. A `value` and its `unit` MUST NOT be changed independently of each other.
**Rationale:** The domain stays faithful to the source. Conversion belongs to population handling, and a value read against the wrong unit is wrong data.
**Contract:** `quantity.json`
**Vectors:** `unit.json#preserve-*`

### WX-UNIT-003
**Status:** Draft
**Level:** MUST
A `unit` with a namespace prefix (`wmo:`, `wmoUnit:`, `nwsUnit:`, or `uc:`) MUST be rejected with reason `unit.namespaced`. When a unit is both namespaced and not valid UCUM, the reason MUST be `unit.namespaced`.
**Rationale:** The prefixes are deprecated or custom, and converting them needs a mapping table that does not exist yet. The precedence gives one predictable reason for the common NWS case.
**Contract:** `quantity.json` (its `unit` pattern)
**Vectors:** `unit.json#reject-namespaced-*`

### WX-UNIT-004
**Status:** Draft
**Level:** MUST
An empty `unit` MUST be rejected with reason `unit.not-ucum`.
**Rationale:** The empty string is not a UCUM code, and a quantity with no unit is meaningless. (UCUM dimensionless quantities use `1`.)
**Contract:** `quantity.json`
**Vectors:** `unit.json#reject-not-ucum-empty`

## Notes

- **UCUM parsing has a design cost.** A nation without a UCUM library must supply one. That cost belongs to its design and is not a reason to weaken WX-UNIT-001.
- **Vector verification.** The accepted and rejected unit codes in `unit.json` agree with two independent validators (`csharp-lib` and `golang-lib`), each written from the UCUM grammar against the unit list vendored in `../../../standards/ucum`. They have not been checked against an outside UCUM implementation, so keep WX-UNIT-001 at `Draft` until they are.
- **Shared unit data.** The unit list is vendored in `domains/weather/standards/ucum` so every nation validates against the same data.

## Population assumptions

This spec assumes the population handles the following. It is the domain's assumption for now, and may be revised as populations get their own specs.

- **Conversion.** Converting units for a locale or preference (for example `Cel` to `[degF]` for a US user, or `m/s` to knots for a mariner) is done where the population interacts.
- **Display labels.** Showing `Cel` as "°C", translating unit names, and pluralization are population concerns.
- **Unfamiliar units.** Deciding what to show, or how to warn, when a source reports a unit the population does not recognize is a population concern.
- **Conversion accuracy.** Tolerance, precision and rounding of any conversion belong to whoever performs it. The domain defines none.
- **Namespaced source units.** NWS values using `wmoUnit:` or `nwsUnit:` are rejected by the domain today. A population that needs them must wait for a mapping table, or handle them before data enters the domain.

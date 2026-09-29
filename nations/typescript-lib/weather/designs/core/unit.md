*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# typescript-lib design: WX-UNIT (core)

Answers `domains/weather/specs/core/unit.md`.

## Validation seam

The seam is a function type, defined next to its consumer. TypeScript is structurally typed, and a one-method interface would add ceremony that an arrow function does not need.

```ts
/** Reports whether unit is a valid UCUM code (case-sensitive form). */
type UnitValidator = (unit: string) => boolean;
```

`parseQuantity(input, units)` and `parseObservation(input, units)` take a `UnitValidator` as an argument. There is no container and no global registry. A test passes `(u) => u === 'furlong'`, and production passes `isValid`.

The real implementation is a separate entry point, `@world-of-code/weather/ucum`, exported from `src/ucum/`. The root entry point never imports it, so the seam is a package boundary as well as a type, and a caller that brings its own validator does not load the atom table.

The unit reasons `unit.not-ucum` and `unit.namespaced` are members of the `Reason` union from `qty.md`.

## Order of checks

`checkUnit(unit: unknown, units)` runs these in order, and returns a `Result<string>`:

1. `typeof unit === 'string'` and non-empty, else `unit.not-ucum` (WX-UNIT-004). A `null`, number or object unit fails here too, since types are erased and the input is `unknown`.
2. The namespace check, `startsWith` against `wmo:`, `wmoUnit:`, `nwsUnit:` and `uc:`, runs **before** the validator, so `wmoUnit:degC` yields `unit.namespaced` (WX-UNIT-003).
3. The `UnitValidator` runs last, yielding `unit.not-ucum`.

The unit is returned exactly as read. JavaScript strings are immutable, and nothing in the path calls `trim`, `toLowerCase` or `normalize` (WX-UNIT-002). `Quantity` is frozen, so `value` and `unit` cannot be changed independently after parsing. There is no conversion function anywhere in the package.

## The UCUM question (settled)

`ucum.isValid` is a small hand-written recursive descent parser over the UCUM case-sensitive grammar (`domains/weather/standards/ucum/README.MD`), a port of the same rules the Go nation uses. Validation needs only a yes or no, so no conversion machinery is needed and no package was adopted, which keeps the runtime at zero dependencies.

- A prefix applies only to atoms marked metric, and the SI base units all take prefixes. `10*` and `10^` are atoms, any other digit run is a factor, exponents attach to units and not to parenthesized terms, annotations are printable ASCII inside braces, and a leading `/` is allowed.
- The parser walks UTF-16 code units. Every character the grammar allows is ASCII, so any other code unit ends the parse in failure (`mé` is rejected).
- The atom and prefix data are generated into `src/ucum/atoms.gen.ts` by `scripts/gen-ucum.mjs` (`npm run gen:ucum`) from the shared `ucum-essence.xml`. The file declares `encoding="ascii"`, which Node has no decoder name for, so the script reads it as `latin1`, a lossless superset. It scans the `<prefix>`, `<base-unit>` and `<unit>` tags, decodes XML entities in codes, and refuses to write a suspiciously small list. Node built-ins only.
- The generated file is checked in, so `npm test` needs no generation step. `npm run check:ucum` fails when the checked-in file is stale.
- The atoms are a `Map`, not an object literal, so `constructor` or `__proto__` is not found as an atom.
- The package's own tests check that every atom in the unit list is accepted, plus the same valid and invalid table as the Go nation.

Not yet checked against an outside UCUM implementation (see the note in the spec's `unit.md`).

## Traceability

| Requirement | Design decision |
|---|---|
| WX-UNIT-001 | The `UnitValidator` as the last check, implemented by `ucum.isValid`. |
| WX-UNIT-002 | Unit stored exactly as read, in a frozen `Quantity`. No normalization and no conversion function exists. |
| WX-UNIT-003 | The prefix check runs before the validator and takes precedence. |
| WX-UNIT-004 | Empty (and non-string) unit rejected with `unit.not-ucum` before the validator is called. |

## Population handling

None. Converting units for a locale is left to consumers, and the package exports no conversion function.

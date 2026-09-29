*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# typescript-lib weather designs

Designs for the weather domain specs, in TypeScript culture. Specs live in `domains/weather/specs`, and each design cites the requirement IDs it answers.

| Spec set | Area | Design |
|---|---|---|
| core | QTY | `core/qty.md` |
| core | UNIT | `core/unit.md` |

## Package shape

The package root is `nations/typescript-lib/weather`, one level up from `designs/`.

```
package.json          @world-of-code/weather, ESM, zero runtime dependencies
tsconfig.json         strict, noUncheckedIndexedAccess, exactOptionalPropertyTypes, erasableSyntaxOnly, nodenext
src/                  entry point "."  : Quantity, Observation, AnswerState, Result, UnitValidator, parse*/read*/stringify*
src/ucum/             entry point "./ucum": the UCUM validator, atoms.gen.ts is generated and checked in
scripts/gen-ucum.mjs  writes src/ucum/atoms.gen.ts from the shared ucum-essence.xml
test/                 node:test suites: conformance, ucum, untrusted
```

- **No runtime dependencies.** `typescript` and `@types/node` are dev dependencies only. There is no validation library, because the type guards are short and hand-written.
- **No build step to test.** Node 24 strips types natively, so `npm test` runs the `.ts` files as they are. Imports carry the `.ts` extension, which `tsc` accepts (`allowImportingTsExtensions`) and rewrites on `npm run build` (`rewriteRelativeImportExtensions`). `erasableSyntaxOnly` keeps the source to syntax that stripping can remove, so no `enum`, `namespace` or constructor parameter properties.
- **Commands.** `npm test`, `npm run typecheck` (`tsc --noEmit`), `npm run build` (to `dist/`), `npm run gen:ucum` to regenerate the unit data, and `npm run check:ucum` to fail if the checked-in file is stale.
- The library stays population-free: no DOM, HTTP, CLI or formatting.
- Tests register one subtest per conformance vector, named `file/id`, loading `domains/weather/specs/core/conformance/*.json` relative to the test file. Passing vectors is the scoreboard.

## Vector operations

| Operation | TypeScript call |
|---|---|
| `validate` | `parseQuantity(input, isValid)`, checking `ok` and `rejection.reason` |
| `classify` | `parseObservation`, then `state(observation, element)` |
| `roundtrip` | `stringifyQuantity` or `stringifyObservation` of the parsed value, then `JSON.parse` and `assert.deepStrictEqual` against the expected result (key order does not matter, numbers compare by value) |

## Status

All 33 core vectors pass, `tsc --noEmit` is clean, and every one of the 312 atoms in the UCUM unit list is accepted by `ucum.isValid`. Extra suites cover hostile JSON (`__proto__` and `constructor` members, `1e999`, inherited properties, malformed observations).

## Open items

- **Provisional reasons.** `qty.not-object` and the `obs.*` reasons exist so the parsers have something to return, and no spec requirement backs them yet. They are marked "Provisional" in `src/result.ts`, the same set the other nations use. WX-OBS and WX-TIME should define or replace them.
- **Check order inside an observation.** Go walks a map in random order, so it has no fixed order when an input has more than one fault. This design fixes one (see `core/qty.md`), which the specs do not require.
- **Timestamps.** `observedAt` stays a validated RFC 3339 string, so the offset and text survive. That is TIME's territory. Leap seconds (`:60`) are rejected, as Go's `time.Parse` does, though RFC 3339 allows them.
- **Whether `number` is enough,** or source text should be carried for decimals beyond about 15 to 17 significant digits. Decide if the spec ever needs that.
- **Malformed JSON text** is not a spec rejection. The parsers take an already-parsed value, so `JSON.parse` errors reach the caller as the `SyntaxError` they are.
- **Not yet checked against an outside UCUM implementation,** as the spec's unit notes say.

*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# typescript-lib design: WX-QTY (core)

Answers `domains/weather/specs/core/qty.md`. This is a design, so it says how TypeScript does it. What must be true stays in the spec.

## Model

Plain readonly object types. `null` carries "answered, and empty", and an absent property carries "never answered". TypeScript has both `null` and `undefined`, so each gets one job.

```ts
interface Quantity {
  readonly value: number | null;   // null: reported without a value
  readonly unit: string;
}

interface Observation extends ElementQuantities {   // { readonly temperature?: Quantity; ... }
  readonly station: Station;
  readonly observedAt: string;
}

type AnswerState = 'not-reported' | 'reported-without-value' | 'reported';
```

| Answer state | TypeScript form |
|---|---|
| Not reported | the element property is absent from the `Observation` |
| Reported without a value | a `Quantity` whose `value` is `null` |
| Reported | a `Quantity` whose `value` is a finite `number` |

`exactOptionalPropertyTypes` makes `temperature?: Quantity` mean "may be missing", and not "may be `undefined`". That leaves "not reported" exactly one form, `{}` without the key, and the compiler refuses `{ temperature: undefined }`. `AnswerState` is a string literal union, whose members are the same strings the conformance vectors use, so no mapping is needed. `state(observation, element)` serves `classify`, and `Element` is a literal union derived from an `elements` tuple in contract order.

Parsed values are `Object.freeze`d. `readonly` is only a compile-time promise, and the freeze makes it true at runtime as well.

## Numeric type

`value` is `number`, an IEEE 754 double, as Go's `float64` is. TypeScript has no other JSON-native number type, so this is not a choice so much as the platform. Trade-offs:

- `21.15`, `1013.2512345` and `123456789` round-trip exactly, since `JSON.stringify` writes the shortest decimal that parses back to the same double.
- Decimal text with more than about 15 to 17 significant digits does not survive, and neither does the spelling (`1.0` is written `1`, `1e2` is `100`), though the value does. Integers beyond 2^53 lose precision on `JSON.parse`, before this library sees them.
- `-0` is a number and is reported, but `JSON.stringify(-0)` writes `0`. Recorded as a quirk, not a requirement.
- `JSON.parse("1e999")` yields `Infinity`, which is a `number` that JSON cannot express. It is rejected as `qty.value-not-numeric` (WX-QTY-009), as is `NaN`, so a `Quantity` always holds a finite number or `null`.
- A `bigint` or decimal-string representation would preserve more, and every consumer would pay for it. Recorded as an open item.

## Time

`observedAt` is the validated RFC 3339 string, kept as read, and not a `Date`. Types are erased and `Date` is a millisecond instant with no offset, so a `Date` would lose `+09:30`, fractional digits past milliseconds and the spelling of `Z`. Keeping the string makes the round trip exact, and it means TypeScript, unlike Go, does not rewrite `+00:00` as `Z`. The cost is that a consumer who wants an instant calls `Date.parse` or a `Temporal` type itself. The validator checks form only: a capital `T` and `Z`, an offset, and a real calendar date and clock time. TIME will decide whether that stays.

## Absent versus null, and untrusted JSON

`JSON.parse` returns `any`, and types are erased, so a `Quantity` type says nothing about what arrives. `parseQuantity(input: unknown, units)` narrows the input with hand-written guards, and there is no schema library. Absent versus null is `Object.hasOwn`:

- `Object.hasOwn(input, 'value')` false: `qty.value-missing`. True with `null`: reported without a value. Reading `input.value` alone gives `undefined` for both a missing member and an inherited one.
- `isRecord` accepts only objects whose prototype is `Object.prototype` or `null`, so an object with inherited `value` and `unit` is `qty.not-object` and not a valid quantity. Arrays, `null` and primitives are rejected the same way.
- `Object.keys` lists own enumerable names. `JSON.parse('{"__proto__": 1}')` creates an own data property called `__proto__`, so it is listed and rejected as an unknown member (`qty.unknown-member` or `obs.unknown-member`). The library never assigns to a key taken from the input: it builds results with fixed keys, so there is no prototype pollution path.
- Lookup tables that key on input text (the UCUM atoms) are `Map`s, since `{}["constructor"]` finds an inherited function.

The parsers take a parsed value, not text. Malformed JSON is the caller's `SyntaxError` from `JSON.parse`, and is not a spec rejection.

Checks run in one fixed order, so an input with several faults always gets one reason. For a Quantity: object, unknown member, `value` present, `unit` present, `value` numeric or `null`, then the unit checks in `unit.md`. For an Observation: object, unknown member, station, `observedAt` present then valid, then each element in contract order, where a JSON `null` is `obs.element-null` before the Quantity rules run. A station is an object with a string `id`, an optional string `name` and no other member. Anything else is `obs.station-missing`.

## Rejection

Rejection is a value. Bad input is expected, so no parser throws for it.

```ts
type Result<T> =
  | { readonly ok: true; readonly value: T }
  | { readonly ok: false; readonly rejection: Rejection };

interface Rejection { readonly reason: Reason }

type Reason = 'qty.value-missing' | 'qty.unit-missing' | 'qty.value-not-numeric'
  | 'qty.unknown-member' | 'unit.not-ucum' | 'unit.namespaced'
  | /* provisional */ 'qty.not-object' | 'obs.not-object' | 'obs.station-missing'
  | 'obs.observed-at-missing' | 'obs.observed-at-invalid' | 'obs.element-null' | 'obs.unknown-member';
```

Callers narrow on `ok`, then `switch` on `rejection.reason`, and the compiler checks the switch is exhaustive. `readQuantity` and `readObservation` are throwing wrappers (`RejectionError`, with a `reason` field) for callers who prefer exceptions. The `Reason` union is a plain literal type and not an `enum`, which `erasableSyntaxOnly` forbids and which adds nothing here. The `qty.not-object` and `obs.*` codes are provisional, because the specs do not say what a non-object or a malformed observation is.

## Writing

`stringifyQuantity` and `stringifyObservation` build a plain object with fixed keys and call `JSON.stringify`. An element that is absent is not written, a `null` value is written as `"value": null` with its unit, and numbers go through `JSON.stringify` unrounded. Nothing formats a number.

## Traceability

| Requirement | Design decision |
|---|---|
| WX-QTY-001 | Three forms in the table above. `state()` returns the `AnswerState` literal for each. |
| WX-QTY-002 | Element property absent, checked with `Object.hasOwn`. `state()` returns `'not-reported'`, and an inherited property does not count. |
| WX-QTY-003 | `Quantity` with `value: null` keeps its `unit`. `state()` returns `'reported-without-value'`. |
| WX-QTY-004 | A finite `number`, including `0` and negatives. The state test is `value === null`, never truthiness, so zero is reported. |
| WX-QTY-005 | No function converts between states. `value` is never defaulted, and the writer omits only absent elements. |
| WX-QTY-006 | `number` written by `JSON.stringify`, shortest round trip. No rounding anywhere. Limits under "Numeric type". |
| WX-QTY-007 | `Object.hasOwn(input, 'value')` false gives `qty.value-missing`. |
| WX-QTY-008 | `Object.hasOwn(input, 'unit')` false gives `qty.unit-missing`, whatever `value` holds. |
| WX-QTY-009 | `value` that is not `null` or a finite `number` gives `qty.value-not-numeric`. This includes `Infinity` from `1e999`. |
| WX-QTY-010 | Any `Object.keys` entry other than `value` and `unit` gives `qty.unknown-member`, including `__proto__`. |

## Population handling

None. The package takes parsed JSON and returns frozen objects or a rejection. Formatting, rounding and locale rendering belong to whoever calls it, such as a `react` or `node-webapi` nation.

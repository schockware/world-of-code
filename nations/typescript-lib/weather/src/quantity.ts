import { isRecord, ok, reject, unwrap, type Result } from './result.ts';
import { checkUnit, type UnitValidator } from './unit.ts';

/**
 * A measured value and the unit it was reported in (WX-QTY).
 * `value: null` means "reported without a value". "Not reported" is an absent element
 * on an Observation, so a Quantity on its own is never "not reported".
 */
export interface Quantity {
  readonly value: number | null;
  readonly unit: string;
}

/**
 * Reads a Quantity from an already-parsed JSON value (the result of `JSON.parse`), applying
 * WX-QTY and WX-UNIT. Types are erased at runtime, so every check here is a hand-written guard.
 * A rejected input returns `{ ok: false }`, and nothing here throws for bad input.
 *
 * Checks run in this order: an object; no unknown member; `value` present; `unit` present;
 * `value` is a finite number or null; then the unit checks in `checkUnit`.
 */
export function parseQuantity(input: unknown, units: UnitValidator): Result<Quantity> {
  if (!isRecord(input)) return reject('qty.not-object');

  // Object.keys lists own enumerable names only, and JSON.parse makes "__proto__" an own
  // data property, so a member with that name is seen here and rejected like any other.
  for (const name of Object.keys(input)) {
    if (name !== 'value' && name !== 'unit') return reject('qty.unknown-member');
  }
  // hasOwn tells a missing `value` from `"value": null` (WX-QTY-007). Reading input.value
  // alone gives undefined for a missing member, and would be one typo away from
  // reading an inherited one.
  if (!Object.hasOwn(input, 'value')) return reject('qty.value-missing');
  if (!Object.hasOwn(input, 'unit')) return reject('qty.unit-missing');

  const value = input['value'];
  if (value !== null && !isFiniteNumber(value)) return reject('qty.value-not-numeric');

  const unit = checkUnit(input['unit'], units);
  if (!unit.ok) return unit;

  return ok(Object.freeze({ value, unit: unit.value }));
}

/** Like `parseQuantity`, but throws a `RejectionError` when the input is rejected. */
export function readQuantity(input: unknown, units: UnitValidator): Quantity {
  return unwrap(parseQuantity(input, units));
}

/**
 * Writes a Quantity as JSON. Both members are always written, so `value: null` stays `null`
 * (WX-QTY-003). The number is written by `JSON.stringify`, which emits the shortest text
 * that parses back to the same double, and never rounds (WX-QTY-006).
 */
export function stringifyQuantity(quantity: Quantity): string {
  return JSON.stringify(toWire(quantity));
}

/** The plain object `JSON.stringify` writes for a Quantity. */
export function toWire(quantity: Quantity): { value: number | null; unit: string } {
  return { value: quantity.value, unit: quantity.unit };
}

/** JSON has no Infinity or NaN, but `JSON.parse("1e999")` yields Infinity. Reject non-finite numbers. */
function isFiniteNumber(value: unknown): value is number {
  return typeof value === 'number' && Number.isFinite(value);
}

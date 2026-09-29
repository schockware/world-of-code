/** A rejection code defined by the weather specs. */
export type Reason =
  | 'qty.value-missing'
  | 'qty.unit-missing'
  | 'qty.value-not-numeric'
  | 'qty.unknown-member'
  | 'unit.not-ucum'
  | 'unit.namespaced'
  // Provisional: WX-OBS is not written yet, and WX-QTY does not say what a non-object Quantity is.
  | 'qty.not-object'
  | 'obs.not-object'
  | 'obs.station-missing'
  | 'obs.observed-at-missing'
  | 'obs.observed-at-invalid'
  | 'obs.element-null'
  | 'obs.unknown-member';

/** Why an input was rejected. The reason is data, so a caller can switch on it. */
export interface Rejection {
  readonly reason: Reason;
}

/** Expected bad input is a value, not an exception. Narrow on `ok`. */
export type Result<T> =
  | { readonly ok: true; readonly value: T }
  | { readonly ok: false; readonly rejection: Rejection };

/** Thrown only by the `read*` wrappers. The parsers never throw for bad input. */
export class RejectionError extends Error {
  readonly reason: Reason;

  constructor(reason: Reason) {
    super(reason);
    this.name = 'RejectionError';
    this.reason = reason;
  }
}

export function ok<T>(value: T): Result<T> {
  return { ok: true, value };
}

export function reject(reason: Reason): Result<never> {
  return { ok: false, rejection: { reason } };
}

/** Returns the value of an accepted result, or throws a RejectionError. */
export function unwrap<T>(result: Result<T>): T {
  if (!result.ok) throw new RejectionError(result.rejection.reason);
  return result.value;
}

/**
 * True for a plain JSON object: not null, not an array, and not an instance of a class.
 * Objects built by JSON.parse always qualify, and a `Map`, `Date` or `Object.create(proto)`
 * with inherited members does not, so an inherited `value` can never be mistaken for a member.
 */
export function isRecord(input: unknown): input is Readonly<Record<string, unknown>> {
  if (typeof input !== 'object' || input === null || Array.isArray(input)) return false;
  const proto: unknown = Object.getPrototypeOf(input);
  return proto === Object.prototype || proto === null;
}

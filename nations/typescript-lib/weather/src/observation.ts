import { parseQuantity, toWire } from './quantity.ts';
import type { Quantity } from './quantity.ts';
import { isRecord, ok, reject, unwrap, type Result } from './result.ts';
import type { UnitValidator } from './unit.ts';

/** What the source said about one element (WX-QTY-001). The strings are the ones the conformance vectors use. */
export type AnswerState = 'not-reported' | 'reported-without-value' | 'reported';

/** Every element of an Observation, in contract order. */
export const elements = [
  'temperature',
  'dewpoint',
  'relativeHumidity',
  'windDirection',
  'windSpeed',
  'windGust',
  'barometricPressure',
  'seaLevelPressure',
  'visibility',
] as const;

/** The name of an element of an Observation, as it appears in the contract. */
export type Element = (typeof elements)[number];

/** The observing station. */
export interface Station {
  readonly id: string;
  readonly name?: string;
}

type ElementQuantities = { readonly [E in Element]?: Quantity };

/**
 * A set of surface weather elements observed at one station at one instant.
 *
 * An element that is absent (no own property) was not reported. With
 * `exactOptionalPropertyTypes`, `?: Quantity` means the property may be missing but may not
 * hold `undefined`, so "absent" has exactly one form.
 *
 * `observedAt` is the validated RFC 3339 string, kept as read. See designs/core/qty.md
 * for the trade-off against a `Date`.
 */
export interface Observation extends ElementQuantities {
  readonly station: Station;
  readonly observedAt: string;
}

/**
 * Answers "what did the source say about this element?" (WX-QTY-001 to 005).
 * hasOwn keeps an inherited property from counting as an answer.
 */
export function state(observation: Observation, element: Element): AnswerState {
  if (!Object.hasOwn(observation, element)) return 'not-reported';
  const quantity = observation[element];
  if (quantity === undefined) return 'not-reported'; // defensive: the type forbids it, a cast could not
  return quantity.value === null ? 'reported-without-value' : 'reported';
}

const elementSet: ReadonlySet<string> = new Set(elements);
const memberNames: ReadonlySet<string> = new Set(['station', 'observedAt', ...elements]);

/**
 * Reads an Observation from an already-parsed JSON value. Only the parts the core specs cover are
 * strict. Reasons starting `obs.` are provisional until WX-OBS is written.
 *
 * Checks run in a fixed order, so one input always gives one reason: an object; no unknown member;
 * a valid station; `observedAt` present, then valid; then each element in contract order, a JSON
 * `null` first (`obs.element-null`), then the Quantity rules.
 */
export function parseObservation(input: unknown, units: UnitValidator): Result<Observation> {
  if (!isRecord(input)) return reject('obs.not-object');
  for (const name of Object.keys(input)) {
    if (!memberNames.has(name)) return reject('obs.unknown-member');
  }

  if (!Object.hasOwn(input, 'station')) return reject('obs.station-missing');
  const station = parseStation(input['station']);
  if (station === undefined) return reject('obs.station-missing');

  if (!Object.hasOwn(input, 'observedAt')) return reject('obs.observed-at-missing');
  const observedAt = input['observedAt'];
  if (typeof observedAt !== 'string' || !isRfc3339(observedAt)) return reject('obs.observed-at-invalid');

  const parsed: { -readonly [E in Element]?: Quantity } = {};
  for (const element of elements) {
    if (!Object.hasOwn(input, element)) continue;
    const member = input[element];
    // "Not reported" has exactly one form: the element is omitted. A JSON null is not allowed.
    if (member === null) return reject('obs.element-null');
    const quantity = parseQuantity(member, units);
    if (!quantity.ok) return quantity;
    parsed[element] = quantity.value;
  }

  return ok(Object.freeze({ ...parsed, station, observedAt }));
}

/** Like `parseObservation`, but throws a `RejectionError` when the input is rejected. */
export function readObservation(input: unknown, units: UnitValidator): Observation {
  return unwrap(parseObservation(input, units));
}

/**
 * Writes contract JSON. An element that was not reported is omitted, one reported without a value
 * is written with `"value": null`, and numbers are written as they are held.
 */
export function stringifyObservation(observation: Observation): string {
  const wire: Record<string, unknown> = {
    station: observation.station.name === undefined
      ? { id: observation.station.id }
      : { id: observation.station.id, name: observation.station.name },
    observedAt: observation.observedAt,
  };
  for (const element of elements) {
    if (!Object.hasOwn(observation, element)) continue;
    const quantity = observation[element];
    if (quantity !== undefined) wire[element] = toWire(quantity);
  }
  return JSON.stringify(wire);
}

/** A station is an object with a string `id`, an optional string `name`, and no other member. */
function parseStation(input: unknown): Station | undefined {
  if (!isRecord(input)) return undefined;
  for (const name of Object.keys(input)) {
    if (name !== 'id' && name !== 'name') return undefined;
  }
  const id = input['id'];
  if (!Object.hasOwn(input, 'id') || typeof id !== 'string') return undefined;
  if (!Object.hasOwn(input, 'name')) return Object.freeze({ id });
  const name = input['name'];
  if (typeof name !== 'string') return undefined;
  return Object.freeze({ id, name });
}

const rfc3339 =
  /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})(?:\.\d+)?(?:Z|[+-](\d{2}):(\d{2}))$/;

/**
 * RFC 3339 date-time, as strict as Go's time.Parse(time.RFC3339): a capital `T` and `Z`, an
 * offset always present, seconds 00 to 59 (no leap second), and a real calendar date.
 * The text is checked, and not turned into a Date.
 */
function isRfc3339(text: string): boolean {
  const m = rfc3339.exec(text);
  if (m === null) return false;
  const [year, month, day, hour, minute, second, offHour, offMinute] = m.slice(1).map((part) =>
    part === undefined ? 0 : Number(part),
  ) as [number, number, number, number, number, number, number, number];
  if (month < 1 || month > 12 || day < 1) return false;
  const leap = year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0);
  const daysInMonth = [31, leap ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][month - 1] ?? 0;
  return day <= daysInMonth && hour <= 23 && minute <= 59 && second <= 59 && offHour <= 23 && offMinute <= 59;
}

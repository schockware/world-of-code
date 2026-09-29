import { ok, reject, type Result } from './result.ts';

/**
 * The validation seam (WX-UNIT-001): reports whether a unit is a valid UCUM code
 * (case-sensitive form). `isValid` from `@world-of-code/weather/ucum` is the real one,
 * and a test passes any arrow function.
 */
export type UnitValidator = (unit: string) => boolean;

/** The namespace prefixes WX-UNIT-003 rejects. */
const namespacePrefixes = ['wmo:', 'wmoUnit:', 'nwsUnit:', 'uc:'] as const;

/**
 * Applies WX-UNIT in order: a non-empty string, then no namespace prefix, then the validator.
 * The unit is returned exactly as read (WX-UNIT-002).
 */
export function checkUnit(unit: unknown, units: UnitValidator): Result<string> {
  if (typeof unit !== 'string' || unit === '') return reject('unit.not-ucum');
  // WX-UNIT-003: a namespaced unit is reported as namespaced even though it is not valid UCUM either.
  if (namespacePrefixes.some((prefix) => unit.startsWith(prefix))) return reject('unit.namespaced');
  if (!units(unit)) return reject('unit.not-ucum');
  return ok(unit);
}

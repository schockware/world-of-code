// The weather domain core (domains/weather/specs). No population: no DOM, HTTP or CLI.
// The UCUM validator is a separate entry point, `@world-of-code/weather/ucum`, so the seam is real.
export { RejectionError, unwrap } from './result.ts';
export type { Reason, Rejection, Result } from './result.ts';
export { parseQuantity, readQuantity, stringifyQuantity } from './quantity.ts';
export type { Quantity } from './quantity.ts';
export { checkUnit } from './unit.ts';
export type { UnitValidator } from './unit.ts';
export { elements, parseObservation, readObservation, state, stringifyObservation } from './observation.ts';
export type { AnswerState, Element, Observation, Station } from './observation.ts';

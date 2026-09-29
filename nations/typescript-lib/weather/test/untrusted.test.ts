import assert from 'node:assert/strict';
import { describe, it } from 'node:test';

import { parseObservation, parseQuantity, readQuantity, RejectionError, state, stringifyObservation } from '../src/index.ts';
import type { Reason } from '../src/index.ts';
import { isValid } from '../src/ucum/index.ts';

// Types are erased at runtime, so these inputs arrive as `unknown` from JSON.parse or worse.

function reasonOf(result: { ok: boolean; rejection?: { reason: Reason } }): Reason | 'ok' {
  return result.ok ? 'ok' : (result.rejection?.reason ?? 'ok');
}

const station = '"station":{"id":"KSEA"}';
const at = '"observedAt":"2026-01-01T00:00:00Z"';

describe('parseQuantity on untrusted input', () => {
  const cases: readonly (readonly [string, string, Reason | 'ok'])[] = [
    ['1e999 parses to Infinity', '{"value":1e999,"unit":"m"}', 'qty.value-not-numeric'],
    ['-1e999', '{"value":-1e999,"unit":"m"}', 'qty.value-not-numeric'],
    ['1e-999 underflows to zero, which is finite', '{"value":1e-999,"unit":"m"}', 'ok'],
    ['__proto__ member', '{"value":1,"unit":"m","__proto__":{"x":1}}', 'qty.unknown-member'],
    ['__proto__ alone', '{"__proto__":{"value":1,"unit":"m"}}', 'qty.unknown-member'],
    ['constructor member', '{"value":1,"unit":"m","constructor":1}', 'qty.unknown-member'],
    ['null is not an object', 'null', 'qty.not-object'],
    ['array is not an object', '[]', 'qty.not-object'],
    ['string is not an object', '"m"', 'qty.not-object'],
    ['number unit', '{"value":1,"unit":5}', 'unit.not-ucum'],
    ['null unit', '{"value":1,"unit":null}', 'unit.not-ucum'],
    ['array value', '{"value":[1],"unit":"m"}', 'qty.value-not-numeric'],
    ['object value', '{"value":{},"unit":"m"}', 'qty.value-not-numeric'],
    ['namespace wins over UCUM', '{"value":1,"unit":"wmoUnit:degC"}', 'unit.namespaced'],
    ['unit is not trimmed', '{"value":1,"unit":" m"}', 'unit.not-ucum'],
    ['negative zero is a number', '{"value":-0,"unit":"m"}', 'ok'],
  ];
  for (const [name, json, want] of cases) {
    it(name, () => {
      assert.equal(reasonOf(parseQuantity(JSON.parse(json), isValid)), want);
    });
  }

  it('does not see members inherited from a prototype', () => {
    const inherited = Object.create({ value: 1, unit: 'm' }) as unknown;
    assert.equal(reasonOf(parseQuantity(inherited, isValid)), 'qty.not-object');
  });

  it('rejects an explicit undefined value, which JSON cannot carry, as not numeric', () => {
    assert.equal(reasonOf(parseQuantity({ value: undefined, unit: 'm' }, isValid)), 'qty.value-not-numeric');
  });

  it('keeps the unit exactly as read', () => {
    const result = parseQuantity(JSON.parse('{"value":1,"unit":"[degF]"}'), isValid);
    assert.ok(result.ok);
    assert.equal(result.value.unit, '[degF]');
  });

  it('read wrapper throws a RejectionError carrying the reason', () => {
    assert.throws(
      () => readQuantity({ unit: 'm' }, isValid),
      (e: unknown) => e instanceof RejectionError && e.reason === 'qty.value-missing',
    );
  });

  it('accepts a fake validator through the seam', () => {
    assert.equal(reasonOf(parseQuantity({ value: 1, unit: 'furlong' }, (u) => u === 'furlong')), 'ok');
    assert.equal(reasonOf(parseQuantity({ value: 1, unit: 'm' }, () => false)), 'unit.not-ucum');
  });
});

describe('parseObservation on untrusted input', () => {
  const cases: readonly (readonly [string, string, Reason | 'ok'])[] = [
    ['minimal', `{${station},${at}}`, 'ok'],
    ['station with name', `{"station":{"id":"KSEA","name":"Seattle"},${at}}`, 'ok'],
    ['offset kept', `{${station},"observedAt":"2026-01-01T09:30:00.25+09:30"}`, 'ok'],
    ['not an object', 'null', 'obs.not-object'],
    ['unknown member', `{${station},${at},"humidity":{"value":1,"unit":"%"}}`, 'obs.unknown-member'],
    ['__proto__ member', `{${station},${at},"__proto__":{}}`, 'obs.unknown-member'],
    ['toString member', `{${station},${at},"toString":1}`, 'obs.unknown-member'],
    ['station missing', `{${at}}`, 'obs.station-missing'],
    ['station without id', `{"station":{"name":"x"},${at}}`, 'obs.station-missing'],
    ['station id not a string', `{"station":{"id":1},${at}}`, 'obs.station-missing'],
    ['station extra member', `{"station":{"id":"K","lat":1},${at}}`, 'obs.station-missing'],
    ['station null', `{"station":null,${at}}`, 'obs.station-missing'],
    ['observedAt missing', `{${station}}`, 'obs.observed-at-missing'],
    ['observedAt not a string', `{${station},"observedAt":5}`, 'obs.observed-at-invalid'],
    ['observedAt no offset', `{${station},"observedAt":"2026-01-01T00:00:00"}`, 'obs.observed-at-invalid'],
    ['observedAt date only', `{${station},"observedAt":"2026-01-01"}`, 'obs.observed-at-invalid'],
    ['observedAt 30 February', `{${station},"observedAt":"2026-02-30T00:00:00Z"}`, 'obs.observed-at-invalid'],
    ['observedAt hour 24', `{${station},"observedAt":"2026-01-01T24:00:00Z"}`, 'obs.observed-at-invalid'],
    ['observedAt 29 Feb in a leap year', `{${station},"observedAt":"2028-02-29T00:00:00Z"}`, 'ok'],
    ['element null', `{${station},${at},"temperature":null}`, 'obs.element-null'],
    ['element bad quantity', `{${station},${at},"temperature":{"unit":"Cel"}}`, 'qty.value-missing'],
    ['element bad unit', `{${station},${at},"windSpeed":{"value":1,"unit":"[kn_i]"}}`, 'ok'],
    ['element namespaced', `{${station},${at},"windSpeed":{"value":1,"unit":"nwsUnit:m_s-1"}}`, 'unit.namespaced'],
  ];
  for (const [name, json, want] of cases) {
    it(name, () => {
      assert.equal(reasonOf(parseObservation(JSON.parse(json), isValid)), want);
    });
  }

  it('keeps the observedAt text, offset included, and omits not-reported elements when writing', () => {
    const text = `{${station},"observedAt":"2026-01-01T09:30:00+09:30","dewpoint":{"value":null,"unit":"Cel"}}`;
    const parsed = parseObservation(JSON.parse(text), isValid);
    assert.ok(parsed.ok);
    assert.equal(parsed.value.observedAt, '2026-01-01T09:30:00+09:30');
    assert.equal(state(parsed.value, 'dewpoint'), 'reported-without-value');
    assert.equal(state(parsed.value, 'temperature'), 'not-reported');
    assert.equal(Object.hasOwn(parsed.value, 'temperature'), false);
    assert.deepStrictEqual(JSON.parse(stringifyObservation(parsed.value)), JSON.parse(text));
  });

  it('writes numbers without rounding or altering them', () => {
    const text = `{${station},${at},"temperature":{"value":0.1,"unit":"Cel"},"windSpeed":{"value":1e21,"unit":"m/s"}}`;
    const parsed = parseObservation(JSON.parse(text), isValid);
    assert.ok(parsed.ok);
    const written = JSON.parse(stringifyObservation(parsed.value)) as Record<string, { value: number }>;
    assert.equal(written['temperature']?.value, 0.1);
    assert.equal(written['windSpeed']?.value, 1e21);
  });

  it('does not count an inherited property as an answer', () => {
    const base = parseObservation(JSON.parse(`{${station},${at}}`), isValid);
    assert.ok(base.ok);
    const derived = Object.create({ temperature: { value: 1, unit: 'Cel' } }, {
      station: { value: base.value.station, enumerable: true },
      observedAt: { value: base.value.observedAt, enumerable: true },
    }) as typeof base.value;
    assert.equal(state(derived, 'temperature'), 'not-reported');
  });
});

import assert from 'node:assert/strict';
import { describe, it } from 'node:test';

import { atomCodes, isValid } from '../src/ucum/index.ts';

describe('ucum', () => {
  it('accepts every atom in the unit list', () => {
    const codes = atomCodes();
    assert.deepStrictEqual(codes.filter((code) => !isValid(code)), []);
    assert.ok(codes.length >= 300, `only ${codes.length} atoms loaded, expected the full unit list`);
  });

  const table: readonly (readonly [string, boolean])[] = [
    ['m/s', true],
    ['/min', true],
    ['km2', true],
    ['s-1', true],
    ['10*3/uL', true],
    ['mm[Hg]', true],
    ['kg.m/s2', true],
    ['(m/s)', true],
    ['{rbc}', true],
    ['mL{total}', true],
    ['hPa', true],
    ['%', true],
    ['[degF]', true],

    ['', false],
    ['degC', false],
    ['celsius', false],
    ['cel', false],
    ['m/', false],
    ['m..s', false],
    ['m s', false],
    ['(m', false],
    ['(m/s)2', false], // an exponent applies to a unit, not to a parenthesized term
    ['m{unclosed', false],
    ['m-', false],
    ['[degF', false],
    ['kBtu_IT[', false],
    ['mé', false],

    // Prototype-key names are not atoms.
    ['constructor', false],
    ['__proto__', false],
    ['toString', false],
  ];
  for (const [code, want] of table) {
    it(`isValid(${JSON.stringify(code)}) is ${want}`, () => {
      assert.equal(isValid(code), want);
    });
  }
});

import assert from 'node:assert/strict';
import { readdirSync, readFileSync } from 'node:fs';
import { basename, join } from 'node:path';
import { describe, it } from 'node:test';

import { elements, parseObservation, parseQuantity, state, stringifyObservation, stringifyQuantity } from '../src/index.ts';
import type { Element, Observation } from '../src/index.ts';
import { isValid } from '../src/ucum/index.ts';

// The conformance vectors shared by every nation. Passing them is the scoreboard.
const vectorDir = join(import.meta.dirname, '../../../../domains/weather/specs/core/conformance');

interface Vector {
  readonly id: string;
  readonly operation: string;
  readonly input: Record<string, unknown>;
  readonly expect: { outcome: string; reason?: string; state?: string; result?: unknown };
}

const files = readdirSync(vectorDir).filter((f) => f.endsWith('.json')).sort();
assert.ok(files.length > 0, `no conformance files found in ${vectorDir}`);

for (const file of files) {
  const doc = JSON.parse(readFileSync(join(vectorDir, file), 'utf8')) as { vectors: Vector[] };
  describe(basename(file, '.json'), () => {
    for (const v of doc.vectors) {
      it(`${basename(file, '.json')}/${v.id}`, () => {
        switch (v.operation) {
          case 'validate':
            return runValidate(v);
          case 'classify':
            return runClassify(v);
          case 'roundtrip':
            return runRoundtrip(v);
          default:
            assert.fail(`unknown operation ${v.operation}`);
        }
      });
    }
  });
}

function runValidate(v: Vector): void {
  const result = parseQuantity(v.input['quantity'], isValid);
  if (v.expect.outcome === 'ok') {
    assert.equal(result.ok, true, `expected ok, got ${JSON.stringify(result)}`);
    return;
  }
  assert.equal(result.ok, false, 'expected a rejection');
  if (!result.ok) assert.equal(result.rejection.reason, v.expect.reason);
}

function runClassify(v: Vector): void {
  const parsed = parseObservation(v.input['observation'], isValid);
  assert.ok(parsed.ok, `observation rejected: ${JSON.stringify(parsed)}`);
  const element = v.input['element'];
  assert.ok(isElement(element), `unknown element ${String(element)}`);
  assert.equal(state(parsed.value, element), v.expect.state);
}

function runRoundtrip(v: Vector): void {
  let written: string;
  if (v.input['quantity'] !== undefined) {
    const parsed = parseQuantity(v.input['quantity'], isValid);
    assert.ok(parsed.ok, `quantity rejected: ${JSON.stringify(parsed)}`);
    written = stringifyQuantity(parsed.value);
  } else {
    const parsed = parseObservation(v.input['observation'], isValid);
    assert.ok(parsed.ok, `observation rejected: ${JSON.stringify(parsed)}`);
    const observation: Observation = parsed.value;
    written = stringifyObservation(observation);
  }
  // Deep, strict comparison: objects in any key order, numbers by value.
  assert.deepStrictEqual(JSON.parse(written), v.expect.result);
}

function isElement(name: unknown): name is Element {
  return typeof name === 'string' && (elements as readonly string[]).includes(name);
}

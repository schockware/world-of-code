*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# WX-QTY: Quantity (core)

Covers the `Quantity` object (`../../contracts/domain/schemas/quantity.json`), and how an `Observation` element relates to it. Units are covered in `unit.md`.

## Terms

| Term | Meaning |
|---|---|
| **Element** | A named measurement on an `Observation`, such as `temperature` or `windSpeed`. |
| **Not reported** | The element was never answered. The `Observation` omits it. |
| **Reported without a value** | The element was answered, and the answer was empty. It is a `Quantity` whose `value` is `null`. |
| **Reported** | The element was answered with a numeric value. It is a `Quantity` with a numeric `value`. |

These are the three **answer states**.

## Operations

Vectors in `conformance/qty.json` use these operations.

| Operation | Input | Result |
|---|---|---|
| `classify` | `observation`, `element` (a member name) | `outcome: ok` with `state` one of `not-reported`, `reported-without-value`, `reported`. |
| `validate` | `quantity` | `outcome: ok`, or `outcome: rejected` with a `reason`. |
| `roundtrip` | `quantity` or `observation` | The same input read and written back unchanged, as `outcome: ok` with `result`. |

Each rejection vector contains exactly one fault. This spec does not rank reasons against each other, except where a requirement says so.

## Requirements

### WX-QTY-001
**Status:** Draft
**Level:** MUST
An implementation MUST distinguish the three answer states for every element.
**Rationale:** "No answer", "an empty answer", and "an answer" mean different things, and the source can express all three.
**Contract:** `observation.json`, `quantity.json`
**Vectors:** `qty.json#state-*`

### WX-QTY-002
**Status:** Draft
**Level:** MUST
An element that is absent from an `Observation` MUST be classified as not reported.
**Rationale:** Omission is the only form of "never answered". The contract permits no other.
**Contract:** `observation.json`
**Vectors:** `qty.json#state-not-reported`

### WX-QTY-003
**Status:** Draft
**Level:** MUST
A `Quantity` whose `value` is `null` MUST be classified as reported without a value, and MUST retain its `unit`.
**Rationale:** An empty answer is still an answer, and it says something about the sensor and the unit it was reporting in.
**Contract:** `quantity.json`
**Vectors:** `qty.json#state-reported-without-value`, `qty.json#roundtrip-null-keeps-unit`

### WX-QTY-004
**Status:** Draft
**Level:** MUST
A `Quantity` whose `value` is a number MUST be classified as reported, including when the number is zero or negative.
**Rationale:** Zero is a measurement, not an absence.
**Contract:** `quantity.json`
**Vectors:** `qty.json#state-reported`, `qty.json#state-zero-is-reported`, `qty.json#state-negative-is-reported`

### WX-QTY-005
**Status:** Draft
**Level:** MUST
An operation MUST NOT convert one answer state into another. In particular, it MUST NOT read reported without a value as zero, and MUST NOT read not reported as reported without a value.
**Rationale:** Collapsing states silently changes what the source said.
**Contract:** `observation.json`
**Vectors:** `qty.json#roundtrip-keeps-states-distinct`

### WX-QTY-006
**Status:** Draft
**Level:** MUST
A numeric `value` MUST NOT be rounded, truncated, or otherwise changed when it is read and written back.
**Rationale:** The domain stays faithful to the source. Rounding and precision are population concerns.
**Contract:** `quantity.json`
**Vectors:** `qty.json#roundtrip-value-*`

### WX-QTY-007
**Status:** Draft
**Level:** MUST
A `Quantity` without a `value` member MUST be rejected with reason `qty.value-missing`.
**Rationale:** A missing `value` member is not the same as a `null` one. Only `null` means "answered, and empty".
**Contract:** `quantity.json`
**Vectors:** `qty.json#reject-value-missing`

### WX-QTY-008
**Status:** Draft
**Level:** MUST
A `Quantity` without a `unit` member MUST be rejected with reason `qty.unit-missing`, including when `value` is `null`.
**Rationale:** An empty answer still needs the unit it would have been reported in (see WX-QTY-003).
**Contract:** `quantity.json`
**Vectors:** `qty.json#reject-unit-missing`, `qty.json#reject-unit-missing-with-null-value`

### WX-QTY-009
**Status:** Draft
**Level:** MUST
A `Quantity` whose `value` is neither a number nor `null` MUST be rejected with reason `qty.value-not-numeric`.
**Rationale:** A numeric string such as `"12"` is a different claim from the number 12, and guessing which was meant changes the data.
**Contract:** `quantity.json`
**Vectors:** `qty.json#reject-value-string`, `qty.json#reject-value-boolean`

### WX-QTY-010
**Status:** Draft
**Level:** MUST
A `Quantity` with any member other than `value` and `unit` MUST be rejected with reason `qty.unknown-member`.
**Rationale:** Silently dropping members would lose information from the source, which contradicts faithfulness. The contract closes the object with `additionalProperties: false`.
**Contract:** `quantity.json`
**Vectors:** `qty.json#reject-unknown-member`

## Population assumptions

This spec assumes the population handles the following. It is the domain's assumption for now, and may be revised as populations get their own specs.

- **Presentation of the three states.** How "not reported" and "reported without a value" appear to a person or a consumer (blank, "N/A", "--", an omitted row) is a population decision. The domain only guarantees the states are distinguishable.
- **Whether the distinction matters.** A human may not care about the difference between "not reported" and "reported without a value". An automation may. The domain does not decide this.
- **Numeric formatting.** Decimal places, rounding mode (for example banker's rounding), digit grouping and locale-specific separators are applied where the population interacts, not in the domain.
- **Export rules.** If a pipeline needs a fixed precision or a placeholder for missing values, that is defined by the population interaction.

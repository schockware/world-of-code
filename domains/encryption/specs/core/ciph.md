*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# ENC-CIPH: Cipher operations (core)

Covers what every cipher offers, and the rules shared by all of them: key validation, rejection precedence, determinism, and the round trip. Each cipher's own key ranges and arithmetic are in its own file (`caes.md`, `aff.md`, `auto.md`, `sky.md`).

## Terms

| Term | Meaning |
|---|---|
| **Cipher** | One named algorithm, identified by a value from `../../contracts/domain/schemas/cipher-id.json`. |
| **Key** | An object in the shape of `keys/{cipher}.json`. A cipher with no secret takes the empty object. |
| **Valid key** | A key that `validateKey` accepts for that cipher. |
| **Inspection** | Anything that exposes a cipher's internal state: a key schedule, intermediate values, a machine's position at each step. |

## Operations

| Operation | Input | Result |
|---|---|---|
| `validateKey` | `cipher`, `key` | `outcome: ok`, or `outcome: rejected` with a `reason` |
| `encrypt` | `cipher`, `key`, `text` (plaintext) | `outcome: ok` with `text` (ciphertext), or `outcome: rejected` with a `reason` |
| `decrypt` | `cipher`, `key`, `text` (ciphertext) | `outcome: ok` with `text` (plaintext), or `outcome: rejected` with a `reason` |

How a nation names these, or whether the cipher is a parameter, an object, or a module, is a design decision.

## Requirements

### ENC-CIPH-001
**Status:** Draft
**Level:** MUST
Every cipher MUST offer exactly three operations: `validateKey`, `encrypt` and `decrypt`.
**Rationale:** The README ground rules limit the public surface to these three. Anything else (keyspace size, key enumeration, scoring) belongs to cracking.
**Contract:** None. This is behavior.
**Vectors:** None. Checked in each nation's design review.

### ENC-CIPH-002
**Status:** Draft
**Level:** MUST
The result of each operation MUST contain only its outcome, and its `text` or `reason`. It MUST NOT expose internal state.
**Rationale:** A cipher that leaks its state through ordinary results gives crackers an advantage history never had.
**Contract:** None. This is behavior.
**Vectors:** Every vector's `expect` lists the complete result.

### ENC-CIPH-003
**Status:** Draft
**Level:** MUST
An implementation MAY offer inspection. If it does, inspection MUST stay off until the caller explicitly enables it.
**Rationale:** README ground rules: "To inspect anything internally, middleware needs to be loaded." Whatever form a nation's inspection layer takes is a design decision.
**Contract:** None. This is behavior.
**Vectors:** None. Checked in each nation's design review.

### ENC-CIPH-004
**Status:** Draft
**Level:** MUST
Enabling inspection MUST NOT change the result of any operation.
**Rationale:** Observing a cipher must not change what it does, or inspection-based teaching tools would show a different cipher than the one under test.
**Contract:** None. This is behavior.
**Vectors:** A nation that offers inspection runs every vector with it enabled and disabled, with identical results.

### ENC-CIPH-005
**Status:** Draft
**Level:** MUST
For any cipher and key, `validateKey` MUST give the same outcome and reason as `encrypt` and `decrypt` give for that key with valid text.
**Rationale:** A caller that checks a key first must be able to rely on the answer.
**Contract:** `keys/`
**Vectors:** `ciph.json#validate-key-*`. Every `validateKey` vector in the cipher files has the same expectation as an `encrypt` with that key.

### ENC-CIPH-006
**Status:** Draft
**Level:** MUST
A cipher name not in `cipher-id.json` MUST be rejected with reason `cipher.unknown`. Names are case-sensitive.
**Rationale:** A misspelled cipher must fail loudly, not fall back to some default.
**Contract:** `cipher-id.json`
**Vectors:** `ciph.json#reject-unknown-cipher*`

### ENC-CIPH-007
**Status:** Draft
**Level:** MUST
A key that does not match its cipher's key contract in shape MUST be rejected with reason `key.malformed`. That covers a key that is not an object, a missing or extra member, a member of the wrong type, and a number that is not an integer where an integer is required.
**Rationale:** Shape faults and value faults are different mistakes, and a caller fixing one should not have to guess which it made. Value faults have their own reasons, defined per cipher.
**Contract:** `keys/{cipher}.json`
**Vectors:** `ciph.json#reject-malformed-*`, `aff.json#affine-reject-missing-b`, `auto.json#reject-primer-number`, `enig.json#reject-malformed-*`, `m209.json#reject-malformed-*`

### ENC-CIPH-008
**Status:** Draft
**Level:** MUST
When an input has more than one fault, the reason MUST be from the first faulty item in this order: the cipher name (`cipher.*`), then the key (`key.*`), then the text (`text.*`).
**Rationale:** One predictable reason per input. A key's validity never depends on the text, so the key is checked first.
**Contract:** None. This is behavior.
**Vectors:** `ciph.json#precedence-*`

### ENC-CIPH-009
**Status:** Draft
**Level:** MUST
`encrypt` and `decrypt` MUST be deterministic: the same cipher, key and text MUST always give the same result.
**Rationale:** Shared vectors only work if the output is fixed. Where history left a choice to the operator (the Jefferson disk's row, the book cipher's locator), that choice will be part of the key.
**Contract:** None. This is behavior.
**Vectors:** All vectors, when run more than once.

### ENC-CIPH-010
**Status:** Draft
**Level:** MUST
For every valid key and valid plaintext, `decrypt` applied to the result of `encrypt` MUST return the plaintext exactly.
**Rationale:** A cipher that cannot recover its message is broken. The skytale spec avoids padding for this reason (ENC-SKY-003).
**Contract:** None. This is behavior.
**Vectors:** Every `encrypt` vector, by decrypting its expected `text`. The cipher files also have explicit `decrypt` vectors.

### ENC-CIPH-011
**Status:** Draft
**Level:** MUST
`decrypt` with a valid key that is not the key used to encrypt MUST NOT be rejected. It MUST return text.
**Rationale:** Classical ciphers have no integrity check. A wrong key gives wrong letters, and telling them apart is a cracker's scoring job, not an error.
**Contract:** None. This is behavior.
**Vectors:** `ciph.json#wrong-key-decrypts-without-rejection`, `auto.json#ciphertext-wrong-primer-only-first-letter-wrong`

## Notes

- **Keyspace is not an operation.** The project brief asks each cipher to declare its keyspace and a key iterator. Under ENC-CIPH-001 those belong to the reserved `CRACK` area. Each cipher file records its keyspace in its Notes, for later use.
- **Reciprocal ciphers.** ROT13 and Atbash (and later Enigma and M-209) encrypt and decrypt with the same operation. They still offer both operations, so callers never need to know.

## Population assumptions

This spec assumes the operator or population handles the following.

- **Choosing and distributing keys.** Key sheets, daily keys, and keeping keys secret are procedure, not cipher behavior.
- **Showing rejections.** Turning a reason code into a message for a person, in their language.

*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# ENC-CAES: Caesar, ROT13, Atbash (core)

Covers three monoalphabetic substitutions: `caesar` (a shift), `rot13` (the fixed shift of 13), and `atbash` (the reversed alphabet). Research, history and attacks are in `../../docs/caesar.md`. Letters are numbered A=0 to Z=25 (ENC-TEXT-003), and `x` is a letter's number.

## Requirements

### ENC-CAES-001
**Status:** Draft
**Level:** MUST
A `caesar` key's `shift` MUST be an integer from 0 to 25. Any other integer MUST be rejected with reason `key.out-of-range`.
**Rationale:** One written form per key keeps keyspaces countable and crack results comparable. Reducing -3 or 29 to 23 or 3 silently would give one key several names.
**Contract:** `keys/caesar.json`
**Vectors:** `caes.json#caesar-accept-*`, `caes.json#caesar-reject-*`

### ENC-CAES-002
**Status:** Draft
**Level:** MUST
`caesar` encryption MUST map each letter `x` to `(x + shift) mod 26`, and decryption MUST map each letter `y` to `(y - shift) mod 26`.
**Rationale:** The standard definition. Caesar's own shift was 3.
**Contract:** `keys/caesar.json`
**Vectors:** `caes.json#caesar-encrypt-*`, `caes.json#caesar-decrypt-*`

### ENC-CAES-003
**Status:** Draft
**Level:** MUST
A `caesar` shift of 0 MUST be accepted, and MUST return the text unchanged.
**Rationale:** It is part of the cipher's arithmetic, and a cracker trying every key will try it. Whether to warn about it is the population's choice.
**Contract:** `keys/caesar.json`
**Vectors:** `caes.json#caesar-shift-0-identity`

### ENC-CAES-004
**Status:** Draft
**Level:** MUST
A `rot13` or `atbash` key MUST be the empty object. A key with any member MUST be rejected with reason `key.malformed`.
**Rationale:** Neither has a secret. Their own identifiers, with no key, make that plain, rather than hiding them as a named Caesar or Affine key.
**Contract:** `keys/rot13.json`, `keys/atbash.json`
**Vectors:** `caes.json#rot13-reject-key-member`, `caes.json#atbash-reject-key-member`

### ENC-CAES-005
**Status:** Draft
**Level:** MUST
`rot13` encryption and decryption MUST both map each letter `x` to `(x + 13) mod 26`.
**Rationale:** Since 13 + 13 = 26, ROT13 is its own inverse, which is why Usenet adopted it for hiding spoilers.
**Contract:** `keys/rot13.json`
**Vectors:** `caes.json#rot13-*`

### ENC-CAES-006
**Status:** Draft
**Level:** MUST
`atbash` encryption and decryption MUST both map each letter `x` to `25 - x`.
**Rationale:** The Hebrew reversal cipher applied to A-Z. It is its own inverse, and equals affine with a=25, b=25.
**Contract:** `keys/atbash.json`
**Vectors:** `caes.json#atbash-*`

## Notes

- **Keyspace.** `caesar`: 26 keys, including the identity. The README counts 25 by leaving the identity out. `rot13` and `atbash`: 1 each, with no secret.
- **Weakness.** Trying all 26 shifts breaks Caesar. Its unicity distance is about 2 letters on average (see `../../docs/caesar.md`, section 3).

## Population assumptions

This spec assumes the operator or population handles the following.

- **Readable output.** Any spaces or punctuation in a demo ("HELLO, WORLD!") are removed before encryption and restored after. The domain sees `HELLOWORLD`.

*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# ENC-AFF: Affine (core)

Covers `affine`, the substitution `x -> (a*x + b) mod 26`. Caesar is the case a=1, and Atbash is a=25, b=25. Research is in `../../docs/caesar.md`. Letters are numbered A=0 to Z=25 (ENC-TEXT-003).

## Requirements

### ENC-AFF-001
**Status:** Draft
**Level:** MUST
An `affine` key's `a` and `b` MUST each be an integer from 0 to 25. A key with either outside that range MUST be rejected with reason `key.out-of-range`.
**Rationale:** One written form per key, as for Caesar (ENC-CAES-001).
**Contract:** `keys/affine.json`
**Vectors:** `aff.json#affine-reject-a-out-of-range`, `aff.json#affine-reject-b-out-of-range`

### ENC-AFF-002
**Status:** Draft
**Level:** MUST
An `affine` key whose `a` shares a factor with 26 (0, any even number, or 13) MUST be rejected with reason `key.not-invertible`. When `a` is also out of range, the reason MUST be `key.out-of-range`.
**Rationale:** Without an inverse of `a` modulo 26, two plaintext letters map to the same ciphertext letter and decryption is ambiguous. The precedence gives one reason for a value that fails both checks.
**Contract:** `keys/affine.json` (`a` lists the 12 valid values)
**Vectors:** `aff.json#affine-reject-a-even`, `aff.json#affine-reject-a-13`, `aff.json#affine-reject-a-0`, `aff.json#affine-precedence-range-over-invertible`, `aff.json#affine-accept-all-invertible`

### ENC-AFF-003
**Status:** Draft
**Level:** MUST
`affine` encryption MUST map each letter `x` to `(a*x + b) mod 26`. Decryption MUST map each letter `y` to `a' * (y - b) mod 26`, where `a'` is the inverse of `a` modulo 26.
**Rationale:** The standard definition.
**Contract:** `keys/affine.json`
**Vectors:** `aff.json#affine-encrypt-*`, `aff.json#affine-decrypt-*`, `aff.json#affine-a1-equals-caesar`, `aff.json#affine-a25-b25-equals-atbash`

## Notes

- **Inverses.** 1->1, 3->9, 5->21, 7->15, 9->3, 11->19, 15->7, 17->23, 19->11, 21->5, 23->17, 25->25.
- **Keyspace.** 12 values of `a` times 26 values of `b`: 312 keys, including the identity (a=1, b=0).
- **Weakness.** Two known plaintext-ciphertext letter pairs recover the key (worked example in `../../docs/caesar.md`, section 6).

## Population assumptions

None beyond those in `text.md`.

*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# ENC-AUTO: Autokey (core)

Covers two ciphers that share a name and are easy to confuse:

- `autokey-ciphertext`: the key stream is the primer, then the **ciphertext**. This is the README's seed idea, with the IV as a one-letter primer.
- `autokey-plaintext`: the key stream is the primer, then the **plaintext**. This is Vigenere's 1586 autokey.

Research is in `../../docs/autokey.md`. Letters are numbered A=0 to Z=25 (ENC-TEXT-003). `P[i]`, `C[i]` and `K[i]` are the numbers of the i-th plaintext, ciphertext and key-stream letters, counting from 0, and `m` is the primer length.

## Requirements

### ENC-AUTO-001
**Status:** Draft
**Level:** MUST
An autokey key's `primer` MUST be one or more of the capital letters A-Z. A string that is not MUST be rejected with reason `key.not-letters`, including the empty string. A `primer` that is not a string is malformed (ENC-CIPH-007).
**Rationale:** The primer is the only secret. Letters, rather than numbers, make one shape cover the README's IV (IV 17 is `R`) and a keyword primer.
**Contract:** `keys/autokey.json`
**Vectors:** `auto.json#reject-primer-*`

### ENC-AUTO-002
**Status:** Draft
**Level:** MUST
`autokey-ciphertext` encryption MUST produce `C[i] = (P[i] + K[i]) mod 26`, where `K[i]` is the i-th primer letter for `i < m`, and `C[i - m]` otherwise.
**Rationale:** The README's recurrence `C[i] = (P[i] + C[i-1]) mod 26` with `C[-1] = IV` is the case m = 1. The general form also covers keyword primers.
**Contract:** `keys/autokey.json`
**Vectors:** `auto.json#ciphertext-readme-vector`, `auto.json#ciphertext-primer-*`

### ENC-AUTO-003
**Status:** Draft
**Level:** MUST
`autokey-plaintext` encryption MUST produce `C[i] = (P[i] + K[i]) mod 26`, where `K[i]` is the i-th primer letter for `i < m`, and `P[i - m]` otherwise.
**Rationale:** Vigenere's original autokey. Its key stream holds the message itself, which resists Kasiski examination but falls to crib dragging.
**Contract:** `keys/autokey.json`
**Vectors:** `auto.json#plaintext-primer-*`

### ENC-AUTO-004
**Status:** Draft
**Level:** MUST
Autokey decryption MUST produce `P[i] = (C[i] - K[i]) mod 26`, with `K[i]` formed as in encryption for the same variant.
**Rationale:** For the ciphertext variant, `K` is available from the ciphertext. For the plaintext variant, it is the plaintext recovered so far.
**Contract:** `keys/autokey.json`
**Vectors:** `auto.json#*-decrypt-*`, `auto.json#ciphertext-readme-vector-decrypt`, `auto.json#ciphertext-wrong-primer-only-first-letter-wrong`

### ENC-AUTO-005
**Status:** Draft
**Level:** MUST
Text shorter than the primer MUST be accepted. Only the first primer letters are used, one per text letter.
**Rationale:** A key's validity must not depend on the text (ENC-CIPH-008). The formula already defines this case.
**Contract:** `keys/autokey.json`
**Vectors:** `auto.json#*-text-shorter-than-primer`

## Notes

- **The README's seed vector.** Primer `R` (IV 17) on `CANGEMINIREADTHIS` gives `TTGMQCKXFWAADWDLD`, matching the README's ciphertext with spaces and punctuation removed. The README's numeric check line mixes A=1 plaintext with A=0 ciphertext and should not be used as a test (`../../docs/autokey.md`, section 6).
- **Weakness of the ciphertext variant.** `P[i] = (C[i] - C[i - m]) mod 26` needs no key for every `i >= m`. A primer of length m protects only the first m letters. `auto.json#ciphertext-wrong-primer-only-first-letter-wrong` shows it: the wrong IV breaks only the first letter.
- **Keyspace.** 26^m for a primer of length m: 26 for the README's single-letter IV.
- **Date-derived IV.** Not in core. The README's `(day + month*k + year) mod 26` leaves `k` and the date format open, and is a key-derivation step. Open question in `../README.MD`.

## Population assumptions

This spec assumes the operator or population handles the following.

- **Word breaks are letters.** With an `x` separator, the X letters are part of the chain like any other letter. The domain never sees spaces, so the chain never resets at a word.
- **Converting an IV.** Showing or entering the IV as a number (17) and converting it to its primer letter (R).

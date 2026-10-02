*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Autokey Cipher (Vigenere autokey and ciphertext autokey)

Research date: 2026-09-29. "Verified" means the URL was opened (fetched or returned by search) in this session and the stated facts were read from it. Fetch summaries were produced by a small model, so details that matter to a spec should be re-checked against the primary source. Test vectors marked "computed" were produced by a script in this session. These are research notes and rationale, not requirements.

---

## 1. History

- Wikipedia (verified) credits Blaise de Vigenere with the autokey cipher in 1586; his original used an agreed single letter as a primer, then the message itself as the rest of the key. This is a plaintext autokey. The name "Vigenere cipher" is popularly attached to the repeating-key cipher, which is a different, weaker construction; that misattribution is well known but I did not fetch a source for it. Earlier autokey-like ideas are credited to Cardano in secondary literature; unverified.
- The ciphertext-autokey variant (key stream from previous ciphertext letters) is the one in the domain README. I found no dated historical source for it; it is presented in the README as a variant and is the classical analogue of CFB/CBC feedback.

## 2. How it works

Alphabet A-Z, A=0 ... Z=25. P[i], C[i], K[i] are letter numbers, indices start at 0, and the primer is `k[0..m-1]`.

Plaintext autokey (Vigenere 1586): the key stream is the primer followed by the plaintext.
- `K[i] = k[i]` for i < m; `K[i] = P[i-m]` for i >= m.
- `C[i] = (P[i] + K[i]) mod 26`. `P[i] = (C[i] - K[i]) mod 26`, recovered sequentially since K[i] is a previously decrypted letter.

Ciphertext autokey (README variant): the key stream is the primer followed by the ciphertext.
- `K[i] = k[i]` for i < m; `K[i] = C[i-m]` for i >= m.
- `C[i] = (P[i] + K[i]) mod 26`. `P[i] = (C[i] - K[i]) mod 26`.
- README special case, m = 1, primer IV in 0..25: `C[i] = (P[i] + C[i-1]) mod 26` with `C[-1] = IV`, so `P[i] = (C[i] - C[i-1]) mod 26`.

Relation to block/stream mode chaining: with m = 1 the ciphertext autokey is a feedback chain where each output depends on the previous output, like CFB (ciphertext feeds the next key) with "block cipher" replaced by a Caesar shift and a 1-letter block. Decryption needs only previous ciphertext, so it is parallelisable and self-synchronising, as in CFB. Plaintext autokey feeds back plaintext and resembles a plaintext-feedback mode (the informal analogue of the rarely used PFB/autokey modes); errors propagate forward in it. CBC's chaining is `C[i] = E(P[i] XOR C[i-1])`; the analogy to CFB and CBC is structural, and the caveat is that the "encryption" step here is a keyless shift of the mixed value, so the chaining adds nothing beyond a running Caesar shift. This mode comparison is my reasoning, not from a fetched source.

## 3. Key and keyspace

- Ciphertext autokey with a one-letter IV (README): 26 keys (IV in 0..25), 25 non-identity.
- With an m-letter primer: 26^m.
- Plaintext autokey: 26^m for primer length m (Wikipedia example: primer QUEENLY, m = 7, 26^7 is about 8 x 10^9).
- Date-derived IV: the README suggests `(day + month*k + year) mod 26`; the keyspace is the image of that function, at most 26.

## 4. Known attacks

Ciphertext autokey, weakness stated precisely: `P[i] = (C[i] - C[i-1]) mod 26` for every i >= m needs no secret at all, because the key stream is the ciphertext itself, which the attacker holds. Only the first m plaintext letters depend on the primer; for m = 1 the IV protects exactly one plaintext letter. The other letters are recovered in linear time with no search. The first letter can be recovered by trying 26 values, or by context. Equivalently, the ciphertext-only attack has effective keyspace of 26 for one letter, and for m > 1 only the first m letters remain hidden. (Computed check: decrypting the README ciphertext with the wrong IV, say 0, yields one wrong first letter and all other letters correct.)

Plaintext autokey: the key stream contains the plaintext. Wikipedia (verified) states that resists Kasiski examination but "the plaintext is part of the key": guessing a probable word and sliding it along the ciphertext reveals key letters, which are plaintext letters offset by m, causing a cascade. Standard attack: try primer lengths, guess crib words, use dictionary or fitness scoring; short primers are searchable exhaustively.

Both variants: the primer is the only real secret and for short primers is brute-forceable.

## 5. Implementation ambiguities a spec must pin down

1. Which variant: plaintext-fed or ciphertext-fed key stream; the two share a name and are often confused.
2. Alphabet and offset: A=0 versus A=1 (a mixed A=1/A=0 convention produces different ciphertext; see the README check below).
3. Case, spaces and punctuation: the README code advances the running key only on letters and passes non-letters through; other conventions strip non-letters first. Whether the chain continues across a word boundary or restarts. Whether lowercase is folded to uppercase (the README `dec` function does not fold; `enc` does).
4. Primer/IV: length and form; whether it is a letter, an integer 0..25, or a keyword; is IV=0 (identity for the first letter) allowed.
5. IV derivation from a date: which date format and time zone, which function (the README only says "for example `(day + month*k + year) mod 26`", with k unspecified), and which date (message date, not the current date).
6. Whether the chain is over letters only or over the whole text, including a case where non-letters have numeric codes.
7. What happens with text shorter than the primer, or an empty message.
8. Whether the ciphertext or the plaintext of the previous position is used when the previous character was a non-letter that passes through.

## 6. Test vectors

Computed with A=0, uppercase, non-letters preserved and not advancing the chain.

Ciphertext autokey, single-letter IV:

| IV | Plaintext | Ciphertext |
|---|---|---|
| 17 (R) | CAN GEMINI READ THIS? | TTG MQCKXF WAAD WDLD? |
| 0 | HELLO | HLWHV |
| 3 (D) | ATTACKATDAWN | DWPPRBBUXXTG |
| 17 (R) | ATTACKATDAWN | RKDDFPPILLHU |

Plaintext autokey, keyword primer (m > 1):

| Primer | Plaintext | Ciphertext |
|---|---|---|
| QUEENLY | ATTACKATDAWN | QNXEPVYTWTWP (sourced: Wikipedia; also computed) |
| KEY | ATTACKATDAWN | KXRAVDAVNAPQ |
| K (m = 1) | ATTACKATDAWN | KTMTCMKTWDWJ |

Ciphertext autokey with keyword primer (computed):

| Primer | Plaintext | Ciphertext |
|---|---|---|
| KEY | ATTACKATDAWN | KXRKZBKSEKOR |
| QUEENLY | ATTACKATDAWN | QNXEPVYJQXAC |

Note the first m letters of the plaintext-fed and ciphertext-fed versions with the same primer are identical, and they diverge after that.

### README vector check (IV=17, CAN GEMINI READ THIS?)

- The ciphertext `TTG MQCKXF WAAD WDLD?` is correct with A=0: encryption gives exactly that string, and decrypting it with IV=17 returns the plaintext (computed).
- Correct numeric line with A=0: plaintext numbers `2 0 13 6 4 12 8 13 8 17 4 0 3 19 7 8 18`; ciphertext numbers `19 19 6 12 16 2 10 23 5 22 0 0 3 22 3 11 3` (this matches the README's second sequence).
- The README's "numeric check" line is inconsistent. Its plaintext numbers `3 1 14 7 5 13 9 14 9 18 5 1 4 20 8 9 19` are A=1 (C=3, A=1, N=14, ...). Its ciphertext numbers are A=0 (T=19). Applying the recurrence to the README's plaintext numbers with the A=1 convention (I recomputed the chain as A=1 throughout) gives `TUIPUHQENFKLPJRAT`, which does not match; the ciphertext listed is only reproduced by the A=0 plaintext numbers. The line mixes conventions and would fail if used as a test. Conclusion: the ciphertext is right; the numeric line is wrong as written and should be replaced with the A=0 plaintext numbers.
- Also of note: the README's `dec` does not upper-case its input while `enc` does.

## 7. References

- Autokey cipher, Wikipedia. https://en.wikipedia.org/wiki/Autokey_cipher . Verified (fetched; 1586, Vigenere, primer, QUEENLY example, crib-dragging weakness).
- Domain README, `domains/encryption/README.MD` in this repository. Verified (read in full); source of the ciphertext-autokey variant and the IV=17 vector.
- Cardano as an earlier autokey inventor. Unverified (from memory; not fetched).
- Mode-of-operation analogy (CBC/CFB), NIST SP 800-38A. Unverified (not fetched; cited only as the place to check the analogy).

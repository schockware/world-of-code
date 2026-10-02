*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Caesar Cipher, with ROT13, Atbash and Affine

Research date: 2026-09-29. "Verified" means the URL was opened (fetched or returned by search) in this session and the stated facts were read from it. Fetch summaries were produced by a small model, so details that matter to a spec should be re-checked against the primary source. Test vectors marked "computed" were produced by a script in this session; "sourced" means the value also appears in a fetched source. Priority: this doc feeds the `core` spec for the encryption domain (see `../README.MD`); these are research notes and rationale, not requirements.

---

## 1. History

- Attributed to Julius Caesar, who used a shift of 3 for military correspondence; his successor Augustus reportedly used a shift of 1 (Wikipedia, verified). The primary ancient source is Suetonius (Divus Julius 56, Divus Augustus 88); I did not open Suetonius, so that attribution is unverified beyond Wikipedia's summary.
- ROT13 (shift 13) became common on Usenet in the early 1980s to hide spoilers, jokes and puzzle answers (Wikipedia, verified). Its self-inverse property is why it stuck.
- Atbash is a Hebrew-alphabet reversal cipher; Wikipedia cites three places in Jeremiah where it appears to encode names, for example "Sheshach" for Babylon in Jeremiah 25:26 (verified via Wikipedia summary only).
- The affine cipher generalises Caesar to `a*x + b`; it is a textbook cipher, and no dated origin was found.

## 2. How it works

Alphabet: the 26 letters A-Z, with A=0 ... Z=25 (`x` denotes a letter's number).

- Caesar with key `k` in 0..25: `E(x) = (x + k) mod 26`, `D(y) = (y - k) mod 26`. Caesar's own shift is k=3. k=0 is the identity.
- ROT13: Caesar with k=13. Since 13+13 = 26, `E = D`.
- Atbash: `E(x) = (25 - x) mod 26`, keyless and self-inverse. It is the affine cipher with a=25, b=25.
- Affine with key (a, b): `E(x) = (a*x + b) mod 26`, `D(y) = a^-1 * (y - b) mod 26`, where `a^-1` is the multiplicative inverse of a modulo 26. It exists only if gcd(a, 26) = 1. Caesar is a=1.

Inverses for valid a: 1->1, 3->9, 5->21, 7->15, 9->3, 11->19, 15->7, 17->23, 19->11, 21->5, 23->17, 25->25 (computed for a=5: 21; sourced set of valid a matches Wikipedia).

## 3. Key and keyspace

- Caesar: 26 shifts, 25 non-trivial. ROT13: 1. Atbash: 1 (no key).
- Affine: 12 valid values of a (1, 3, 5, 7, 9, 11, 15, 17, 19, 21, 23, 25) times 26 values of b = 312 keys, of which 311 are non-identity (a=1,b=0 is the identity) (Wikipedia, verified, and the a-list computed).
- The README table lists Caesar as 25; that counts non-identity shifts only. The choice of counting convention is an ambiguity (section 5).
- Unicity distance of Caesar is very small: Wikipedia states about 2 characters on average, about 6 in practice (verified summary).

## 4. Known attacks

- Caesar: try all 26 shifts and score each candidate (dictionary hits, chi-squared against letter frequencies, quadgram fitness). Or align the most frequent ciphertext letter with E.
- ROT13 and Atbash: no attack needed; there is no secret.
- Affine: try all 312 keys; or known plaintext with two distinct letter pairs, solving `y1 = a*x1 + b`, `y2 = a*x2 + b` mod 26 (subtract to get `a*(x1 - x2) = y1 - y2`; this needs `x1 - x2` to be invertible or a small case search when it is even or 13) (Wikipedia, verified, states the two-pair weakness). Also frequency analysis of the two most common ciphertext letters.
- All are monoalphabetic substitutions, so letter frequencies and word patterns pass through unchanged.

## 5. Implementation ambiguities a spec must pin down

1. Alphabet and offset: A=0 (README convention) versus A=1. With A=1 the arithmetic mod 26 gives the same letters only if the mapping is consistent; mixed use gives wrong results (see the autokey doc for a real example).
2. Case: fold to upper, preserve case per letter, or reject lowercase. Output case for a lowercase input.
3. Non-letters (spaces, digits, punctuation, accented letters, non-ASCII): pass through unchanged, strip, or reject. Whether spaces are kept affects word-pattern leakage. Whether digits are rotated (ROT5, ROT47 are separate ciphers).
4. Valid keys: are shifts limited to 0..25, or any integer reduced mod 26 (so -3 and 23 are the same key)? Is k=0 valid? Is k=26 valid? Is ROT13 a distinct cipher or key 13?
5. Key form for affine: a and b as integers; what to do with an a that is not coprime to 26 (reject, since decryption is ambiguous); whether b is also reduced mod 26.
6. Whether Atbash and ROT13 are standalone ciphers with an empty key or registered Caesar/Affine keys.
7. Keyspace count convention: 26 versus 25 versus 312 versus 311.
8. Decrypt of a wrong key: it always yields some string; a cracker needs a scorer, not an error.

## 6. Test vectors

All computed by script (A=0, uppercase, non-letters preserved) unless marked sourced.

| Cipher | Key | Plaintext | Ciphertext |
|---|---|---|---|
| Caesar | k=3 | THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG | WKH TXLFN EURZQ IRA MXPSV RYHU WKH ODCB GRJ |
| Caesar | k=3 | HELLO, WORLD! | KHOOR, ZRUOG! |
| Caesar | k=3 | ATTACKATDAWN | DWWDFNDWGDZQ |
| ROT13 | none | HELLO, WORLD! | URYYB, JBEYQ! |
| ROT13 | none | THE QUICK BROWN FOX | GUR DHVPX OEBJA SBK |
| Atbash | none | HELLO, WORLD! | SVOOL, DLIOW! |
| Atbash | none | WIZARD | DRAZIW |
| Affine | a=5, b=8 | AFFINE CIPHER | IHHWVC SWFRCP (sourced: Wikipedia gives IHHWVCSWFRCP; also computed) |
| Affine | a=7, b=3 | HELLO | AFCCX |
| Affine | a=1, b=3 | HELLO | KHOOR (equals Caesar 3) |
| Affine | a=25, b=25 | HELLO | SVOOL (equals Atbash) |

Two-pair recovery example: from the a=5, b=8 vector, A->I (0->8) gives b=8; F->H (5->7) gives 5a + 8 = 7, so 5a = -1 = 25 mod 26, a = 25 * 21 mod 26 = 5. Computed by hand and consistent with the key.

## 7. References

- Caesar cipher, Wikipedia. https://en.wikipedia.org/wiki/Caesar_cipher . Verified (fetched; formula, Augustus shift 1, brute force, unicity distance).
- ROT13, Wikipedia. https://en.wikipedia.org/wiki/ROT13 . Verified (fetched; Usenet history, self-inverse).
- Atbash, Wikipedia. https://en.wikipedia.org/wiki/Atbash . Verified (fetched; Jeremiah, self-inverse).
- Affine cipher, Wikipedia. https://en.wikipedia.org/wiki/Affine_cipher . Verified (fetched; formula, 312 keys, AFFINE CIPHER example, two-pair weakness).
- Suetonius, Lives of the Caesars (Divus Julius 56, Divus Augustus 88). Unverified; cited only via Wikipedia's account, not opened.

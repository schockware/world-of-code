I just watched a documentary on Enigma and thought, "Why not?! This would be a fun domain to add. The presentation layer will also be fun to add."

# So our starting systems will be:
- Caesar Cipher
- Jefferson Disk
- Ciphertext autokey cipher
- Skytale
- M-209
- Enigma
- Book Cipher
- Anything our Claude companion contributed

# Claude Contributions - Classical Cipher Project: Brief for Coding Assistant

## Goal

Build an educational encryption project that walks from the Caesar cipher up to Enigma and the M-209. Each cipher's weakness motivates the next one. Vulnerabilities are a feature: every cipher should ship with a working attack.

The centerpiece is a **cracker harness** with a pluggable cipher and a pluggable scoring function, so anything with a keyspace under about 10^10 can be brute-forced and ranked.

## Conventions

- Alphabet A-Z with A=0. Preserve case and punctuation on output, or strip them consistently (pick one and document it).
- Each cipher exposes `encrypt(text, key)` and `decrypt(text, key)`.
- Each cipher declares its keyspace size and an iterator over keys where feasible.
- Each cipher has tests, including a known test vector.

## The seed idea: date-keyed ciphertext autokey

A starting key (for example a date in the top-right corner of the message) selects an initial value (IV). Each ciphertext letter is then shifted by the previous ciphertext letter.

```
C[i] = (P[i] + C[i-1]) mod 26      # C[-1] = IV, derived from the date
P[i] = (C[i] - C[i-1]) mod 26
```

This is the **ciphertext autokey** variant of Vigenère autokey (1586). It is conceptually the same as CBC/CFB chaining with an IV. Date-derived IV: any function of the date producing 0-25, for example `(day + month*k + year) mod 26`.

**Test vector** (IV = 17, i.e. R):

```
Plaintext:  CAN GEMINI READ THIS?
Ciphertext: TTG MQCKXF WAAD WDLD?
```

Numeric check: plaintext `3 1 14 7 5 13 9 14 9 18 5 1 4 20 8 9 19` (spaces and punctuation removed) becomes `19 19 6 12 16 2 10 23 5 22 0 0 3 22 3 11 3` for the letters as written in the original example.

**Weakness:** anyone with the ciphertext recovers every letter except the first, because `P[i] = C[i] - C[i-1]` needs no key. The IV protects only one character. A stronger exercise variant: use the date to seed a keyed substitution or Vigenère key, and use chaining as an additional layer.

```python
def enc(text, iv):
    prev, out = iv, []
    for ch in text:
        if ch.isalpha():
            prev = (ord(ch.upper()) - 65 + prev) % 26
            out.append(chr(prev + 65))
        else:
            out.append(ch)
    return ''.join(out)

def dec(text, iv):
    prev, out = iv, []
    for ch in text:
        if ch.isalpha():
            c = ord(ch) - 65
            out.append(chr((c - prev) % 26 + 65))
            prev = c
        else:
            out.append(ch)
    return ''.join(out)
```

## Ciphers under about 10^10 (exhaustively searchable)

| Cipher | Keyspace | Break |
|---|---|---|
| Caesar / ROT13 / Atbash | 25 / 1 / 1 | Try all keys |
| Affine | 312 (12 x 26) | Try all keys, or two known letter pairs |
| Autokey (above) | 26, or 26^n with a keyword primer | Difference of adjacent ciphertext letters |
| Trithemius | 1 | Progressive Caesar with a fixed shift sequence |
| Gronsfeld | 10^n | Vigenere with digit keys |
| Vigenere / Beaufort / Porta | 26^n (n up to 7 is about 8 x 10^9) | Kasiski, index of coincidence, per-column frequency analysis |
| Rail fence / Scytale | Tiny | Try all key sizes |
| Columnar transposition | n! (13 columns is about 6 x 10^9) | Anagramming or hill climbing on column order |
| Hill 2x2 | 157,248 invertible matrices | Known plaintext plus linear algebra |
| Playfair (keyword-limited) | about 26^5 if keywords are capped | Dictionary attack or simulated annealing |
| 32-bit LFSR stream cipher | 2^32, about 4.3 x 10^9 | Berlekamp-Massey with 64 bits of known keystream |
| Repeating-key XOR | 256^n (n up to 4 is about 4 x 10^9) | Hamming-distance key-length guessing |
| LCG keystream | Depends on modulus | Recover parameters from a few outputs |

## Deliberately over the line, but still weak

- **Simple substitution:** 26! is about 4 x 10^26 keys, yet frequency analysis or hill climbing breaks it in seconds. Demonstrates that keyspace size is not security.
- **Enigma (M3):** 60 rotor orders x 17,576 rotor positions x 676 ring settings is about 7 x 10^8. A 10-cable plugboard adds about 1.5 x 10^14, for roughly 1.6 x 10^20 total. The plugboard is a self-inverse substitution, so attacks (Turing Bombe, hill climbing) can separate it from the rotor settings and avoid a full search.
- **M-209:** six pin wheels with 26, 25, 23, 21, 19 and 17 pins give a key stream period of 26 x 25 x 23 x 21 x 19 x 17 = 101,405,850. The lug-cage and pin settings give a huge keyspace, but the key stream is statistically biased, and known-plaintext or depth attacks (Barker, Morris) recover the settings from a few thousand letters.

## Implementation quirks to preserve

**Enigma**
- A letter never encrypts to itself.
- The machine is reciprocal (same settings encrypt and decrypt).
- Middle-rotor double-stepping anomaly.
- Historical bonus: a "sloppy operator" mode with repeated message indicators, since that habit was a bigger break than the machine itself.

**M-209**
- Beaufort-style: `C = K - P`, so the same settings encrypt and decrypt.

## Suggested order

Caesar -> Affine -> Vigenere -> autokey -> columnar -> Playfair -> Hill -> LFSR/XOR -> Enigma -> M-209

## Suggested architecture

```
ciphers/      one module per cipher (encrypt, decrypt, keyspace, key iterator)
scorers/      quadgram fitness, index of coincidence, chi-squared, dictionary hit rate
attacks/      brute force, known plaintext, Kasiski, hill climbing, simulated annealing
cracker.py    harness: pick cipher + scorer, iterate keys, rank candidates
tests/        round-trip tests, known vectors, attack success tests
demo/         per-cipher walkthrough showing encrypt, then break
```

## First tasks for the assistant

1. Scaffold the project structure and the pluggable cipher and scorer interfaces.
2. Implement Caesar, Affine, Vigenere and the date-keyed autokey with tests.
3. Implement quadgram and index-of-coincidence scorers.
4. Build the brute-force cracker harness and demo it on the first four ciphers.
5. Continue down the suggested order, adding an attack for each cipher as it is added.
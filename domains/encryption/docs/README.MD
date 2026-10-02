*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Encryption research docs

What each cipher is based on, so we do not have to relearn it. These are research and rationale, never requirements. Specs cite them. Research date: 2026-09-29.

"Verified" in a reference means the page was fetched and read in the research session. Everything else is marked unverified. Test vectors were computed by throwaway simulators (kept outside the repo) and cross-checked against published examples where any exist; each doc says which vectors were reproduced and which are computed only.

| Cipher | Doc | Notes |
|---|---|---|
| Caesar, ROT13, Atbash, Affine | [caesar.md](caesar.md) | |
| Ciphertext autokey | [autokey.md](autokey.md) | Recomputes the README test vector |
| Book cipher | [book-cipher.md](book-cipher.md) | Several valid ciphertexts per message |
| Jefferson disk, M-94 | [jefferson-disk.md](jefferson-disk.md) | Jefferson's own 36 alphabets could not be found; the M-94's 25 are recorded |
| Skytale | [skytale.md](skytale.md) | Historical evidence is debated |
| Enigma | [enigma.md](enigma.md), [enigma-wiring-data.md](enigma-wiring-data.md) | Wiring tables with sources |
| M-209 | [m209.md](m209.md), [m209-wiring-data.md](m209-wiring-data.md) | No primary US Army example found |

## Corrections to the domain README found by this research

Not yet applied to `../README.MD`.

- Autokey vector: the ciphertext is right with A=0 and IV=17. The "numeric check" line mixes A=1 plaintext with A=0 ciphertext.
- Enigma: the ~1.6e20 total leaves out the 676 ring settings. With them it is ~1.07e23. The 60 rotor orders is for a pool of five rotors. The plugboard count peaks at 11 cables. The stepping period is 16,900.
- M-209: `C = K - P` holds only with plaintext A=1 and ciphertext A=0. With A=0 for both it is `C = (K-1-P) mod 26`. A letter can encipher to itself. "A few thousand letters" is loose: the published attacks range from about 500 to about 2,500 letters depending on the method.

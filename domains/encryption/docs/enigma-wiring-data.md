*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Enigma Historical Wiring Data

Research date: 2026-09-29. "Verified" means the URL was opened (fetched or returned by WebSearch) in this session and the stated facts were read from it. Fetch summaries were produced by a small model, so any string that matters to a conformance vector was additionally checked by computation (see below). Companion to `enigma.md`.

## How these tables were checked

- Every 26-letter string in sections 1 to 5 was tested by script: it is a permutation of A-Z. Every reflector string was tested as an involution with no fixed point (a letter never maps to itself, and A maps to B exactly when B maps to A). All pass.
- The strings for rotors I, II, III, IV, V, reflectors B, and the M3 double-notch behavior were exercised by a simulator that reproduced three published messages (see `enigma.md`, section 10). Rotors VI-VIII, A, C, and the thin reflectors are checked only as permutations; rotors V, VI, VII, VIII, and reflector C were not exercised by any published vector (see per-row notes).
- Convention for all tables: the string is the output letter for input contact A, B, ..., Z in that order, read for the signal traveling from the entry (right) side toward the reflector, with the rotor at position A and ring setting A. The reverse trip uses the inverse permutation.

## 1. Entry wheel (ETW / Eintrittswalze)

| Machine | ETW (keyboard letter to rotor contact A..Z) | Source |
|---|---|---|
| Enigma I, M3, M4 (Wehrmacht, Luftwaffe, Kriegsmarine) | ABCDEFGHIJKLMNOPQRSTUVWXYZ (identity) | Crypto Museum, verified |
| Commercial Enigma D / K (also railway) | QWERTZUIOASDFGHJKPYXCVBNML (keyboard order) | Crypto Museum, verified |

## 2. Rotors I-VIII (Enigma I and M3)

Turnover letter = the window letter shown while the rotor is at its turnover position, meaning the next keypress steps the neighbor to its left. Notch letter = the letter printed on the alphabet ring at the physical notch. In every row below, notch letter = turnover letter + 8 (mod 26), which is a consistency check on the two conventions.

| Rotor | Wiring | Turnover letter | Notch letter on ring | Notches | Machines |
|---|---|---|---|---|---|
| I | EKMFLGDQVZNTOWYHXUSPAIBRCJ | Q (Q to R) | Y | 1 | Enigma I, M3 |
| II | AJDKSIRUXBLHWTMCQGZNPYFVOE | E (E to F) | M | 1 | Enigma I, M3 |
| III | BDFHJLCPRTXVZNYEIWGAKMUSQO | V (V to W) | D | 1 | Enigma I, M3 |
| IV | ESOVPZJAYQUIRHXLNFTGKDCMWB | J (J to K) | R | 1 | Enigma I, M3 |
| V | VZBRGITYUPSDNHLXAWMJQOFECK | Z (Z to A) | H | 1 | Enigma I, M3 |
| VI | JPGVOUMFYQBENHZRDKASXLICTW | Z and M | H and U | 2 | M3 (Navy), M4 |
| VII | NZJHGRCXMYSWBOUFAIVLPEKQDT | Z and M | H and U | 2 | M3 (Navy), M4 |
| VIII | FKQHTLXOCBJSPDZRAMEWNIUYGV | Z and M | H and U | 2 | M3 (Navy), M4 |

Sources:
- Wiring for I-VIII: Wikipedia "Enigma rotor details" (verified) and Crypto Museum wiring page (verified for I-V and VI-VIII). The two sources agree string for string.
- Turnover letters: Wikipedia "Enigma rotor details" (verified: I Q, II E, III V, IV J, V Z, VI-VIII Z and M). Notch letters: Crypto Museum (verified: I Y, II M, III D, IV R, V H, VI-VIII H and U as presented by the fetch summary, which wrote "HU/ZM").
- Rotors I, II, III, IV, V, and the single-notch stepping were exercised by published-vector reproduction (Barbarossa message uses II, IV, V; Wikipedia example uses I, II, III). Rotors VI and VIII were exercised by the U-534 M4 vector (V, VI, VIII with beta and thin C). Rotor VII was not exercised by any published vector.
- Rotor VI-VIII double-notch behavior: exercised only by my own simulator (no published vector with ciphertext found).

## 3. M4 Greek wheels (Zusatzwalzen) and thin reflectors

The Greek wheel is the fourth rotor, thin, placed between the reflector and the leftmost regular rotor. It has no notch, never steps, and has a ring setting and a start position like any rotor.

| Component | Wiring | Notches | Source |
|---|---|---|---|
| Beta | LEYJVCNIXWPBQMDRTAKZGFUHOS | none, never steps | Wikipedia (verified), Crypto Museum (verified) |
| Gamma | FSOKANUERHMBTIYCWLQPZXVGJD | none, never steps | Wikipedia (verified), Crypto Museum (verified) |
| Reflector B thin (UKW B dunn) | ENKQAUYWJICOPBLMDXZVFTHRGS | not applicable | Wikipedia (verified), Crypto Museum (verified) |
| Reflector C thin (UKW C dunn) | RDOBJNTKVEHMLFCWZAXGYIPSUQ | not applicable | Wikipedia (verified), Crypto Museum (verified) |

Exercised: beta and thin C were exercised by the U-534 vector. Gamma and thin B were not exercised by any published vector; they are checked only as permutations, and the two sources agree.

A thin reflector with a Greek wheel set to position A and ring A is designed to be wire-compatible with a standard M3 reflector B (or C) for backward compatibility; this is widely stated (for example, on the Crypto Museum M4 page as returned by search) but I did not independently confirm the claim with a specific setting. Unverified as a design intent; the wiring above simply is what it is.

## 4. Reflectors (Umkehrwalzen), Enigma I and M3

| Reflector | Wiring | Source |
|---|---|---|
| A | EJMZALYXVBWFCRQUONTSPIKHGD | Wikipedia (verified), Crypto Museum (verified). Early, replaced in 1937 by B. Date not verified. |
| B | YRUHQSLDPXNGOKMIEBFZCWVJAT | Wikipedia, Crypto Museum (verified); exercised by two vectors |
| C | FVPJIAOYEDRZXWGCTKUQSBNMHL | Wikipedia, Crypto Museum (verified); not exercised by a published vector (used with the thin C in M4 is a different string) |

In the M3, a fixed reflector cannot be rotated; in the Enigma I the reflector has no ring and no positions. (The Enigma D commercial machine had a rotatable reflector and Abwehr G312 also; see section 6.)

## 5. Convention check for the M3 Army and Air Force machine

The primary target for a first implementation is Enigma I / Army-Air Force M3: rotors I-V, reflector B (and optionally C), identity ETW, three moving rotors, a 10-pair plugboard, and single-notch stepping. The two-notch rotors VI-VIII and the M4 Greek wheel are the naval extensions.

## 6. Commercial and Abwehr machines (extension, lower confidence)

| Machine | Component | Wiring | Notches (as reported) | Source |
|---|---|---|---|---|
| Commercial D / K | Rotor I | LPGSZMHAEOQKVXRFYBUTNICJDW | see note | Crypto Museum, verified |
| Commercial D / K | Rotor II | SLVGBTFXJQOHEWIRZYAMKPCNDU | see note | Crypto Museum, verified |
| Commercial D / K | Rotor III | CJGDPSHKTURAWZXFMYNQOBVLIE | see note | Crypto Museum, verified |
| Commercial D / K | UKW (rotatable) | IMETCGFRAYSQBZXWLHKDVUPOJN | not applicable | Crypto Museum, verified |
| Abwehr G312 | Rotor I | DMTWSILRUYQNKFEJCAZBPGXOHV | 17 (irregular) | Crypto Museum Enigma G page, verified |
| Abwehr G312 | Rotor II | HQZGPJTMOBLNCIFDYAWVEUSRKX | 15 (irregular) | same |
| Abwehr G312 | Rotor III | UQNTLSZFMREHDPXKIBVYGJCWOA | 11 (irregular) | same |
| Abwehr G312 | UKW | RULQMZJSYGOCETKWDAHNBXPVIF | not applicable | same |

Notes and unresolved points:
- Commercial notch data: the fetch summary reported inconsistent turnover data for the same rotors on different machines (for example the target of rotor I as Z for machine D and Y for machine K, and III as Z versus N). I could not resolve this from the pages read and will not tabulate turnover letters for the commercial machines. They need a primary-source check before use. The commercial machine is stated in the task as ETW-relevant only.
- Abwehr G312: the Enigma G machine has a rotor stepping mechanism based on a toothed wheel with missing teeth, giving 17, 15, and 11 effective notches per rotor as reported. That is irregular stepping and is not modeled by the single-notch rule; I did not model or test it. Also the wiring here is exactly as printed by the fetch summary and validated only as a permutation and an involution for the UKW.
- Also seen but out of scope and not tabulated: the Railway Enigma and the Japanese Tirpitz (T) machine, which Crypto Museum lists with different wiring.

## References

| # | Title | Publisher | URL | Status |
|---|---|---|---|---|
| 1 | Enigma wiring | Crypto Museum | https://www.cryptomuseum.com/crypto/enigma/wiring.htm | verified (fetched) |
| 2 | Enigma rotor details | Wikipedia | https://en.wikipedia.org/wiki/Enigma_rotor_details | verified (fetched) |
| 3 | Enigma G | Crypto Museum | https://www.cryptomuseum.com/crypto/enigma/g/index.htm | verified (fetched) |
| 4 | Enigma M4 | Crypto Museum | https://www.cryptomuseum.com/crypto/enigma/m4/index.htm | unverified (appeared in a search result only) |

*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# ENC-ENIG: Enigma M3 (core)

Covers `enigma-m3`. In this project, **M3 means the Army and Air Force Enigma I**: three rotors chosen from I-V, reflector B or C, an identity entry wheel, single-notch stepping, and a plugboard of up to 10 cables. The Navy's M3 added rotors VI-VIII but had the same weaknesses until the four-rotor M4 arrived, so the naval rotors wait for an M4 spec.

Only the machine is specified. The caller supplies the start position directly. The indicator procedure (doubled message keys before May 1940, operator-chosen start positions after) and the "sloppy operator" habits are an operator layer, not yet specified.

Fixed hardware data (wiring and turnover letters) is vendored in `../../standards/enigma/enigma-m3.json`. Research is in `../../docs/enigma.md` and `../../docs/enigma-wiring-data.md`. Letters are numbered A=0 to Z=25 (ENC-TEXT-003).

## Terms

| Term | Meaning |
|---|---|
| **Left, middle, right** | Rotor slots as the operator sees them. Every key member lists them left to right. The right rotor is the fast one. |
| **Window letter** | The letter showing for a rotor. `positions` gives the window letters at the start. |
| **Ring setting** | The ring's rotation against the wiring core. `rings` gives one letter per rotor, where A is no offset. Key sheets wrote 01-26, and 01 is A. |
| **Turnover letter** | The window letter showing when the next keypress also steps the rotor to its left. From the standards file: I Q, II E, III V, IV J, V Z. |
| **Offset** | For a rotor, `d = (window - ring) mod 26`. |

## Requirements

### ENC-ENIG-001
**Status:** Draft
**Level:** MUST
Each of the three `rotors` MUST be one of `I`, `II`, `III`, `IV`, `V`, and `reflector` MUST be `B` or `C`. Any other name MUST be rejected with reason `key.unknown-component`. A key that uses the same rotor twice MUST be rejected with reason `key.repeated-rotor`.
**Rationale:** The Army machine had one of each of the five rotors. Naval rotors (VI-VIII), reflector A, and thin reflectors belong to other machines, and accepting them quietly would blur M3 and M4.
**Contract:** `keys/enigma-m3.json`
**Vectors:** `enig.json#reject-unknown-*`, `enig.json#reject-repeated-rotor`

### ENC-ENIG-002
**Status:** Draft
**Level:** MUST
`rings` and `positions` MUST each be exactly three capital letters A-Z. Anything else MUST be rejected with reason `key.not-letters`.
**Rationale:** One letter per rotor, using the same alphabet as the text. Key-sheet numbers (01-26) are converted by the population.
**Contract:** `keys/enigma-m3.json`
**Vectors:** `enig.json#reject-rings-*`, `enig.json#reject-positions-*`

### ENC-ENIG-003
**Status:** Draft
**Level:** MUST
`plugboard` MUST hold at most 10 pairs, or else be rejected with reason `key.out-of-range`. Each pair MUST be exactly two capital letters, or else be rejected with reason `key.not-letters`. No letter may appear in more than one pair or be paired with itself, or else the key is rejected with reason `key.plugboard-conflict`. The order of pairs, and of the letters within a pair, MUST NOT change the result.
**Rationale:** Army issue was ten cables. Fewer were valid, and were used earlier, so 0 to 10 are accepted. A cable joins two different sockets, and each socket takes one cable.
**Contract:** `keys/enigma-m3.json`. The conflict rule is not expressible in the schema.
**Vectors:** `enig.json#accept-*`, `enig.json#reject-plugboard-*`, `enig.json#plugboard-*`

### ENC-ENIG-004
**Status:** Draft
**Level:** MUST
Before each letter is enciphered, the rotors MUST step, judged on the window letters before the step: the right rotor always steps; the middle rotor steps if the right rotor shows its turnover letter **or** the middle rotor shows its own turnover letter; the left rotor steps if the middle rotor shows its turnover letter.
**Rationale:** This is the pawl-and-notch mechanism, including the middle rotor's **double step**: it moves on two consecutive keypresses once per cycle, which makes the stepping period 16,900 rather than 26^3. Starting at ADU with rotors I II III, the windows run ADV, AEW, BFX, BFY (Wikipedia "Enigma rotor details"). Ring settings move the notch with the letters, so they do not change when stepping happens.
**Contract:** `../../standards/enigma/enigma-m3.json` (turnover letters)
**Vectors:** `enig.json#double-step*`, `enig.json#turnover-*`, `enig.json#left-rotor-wraps`, `enig.json#ring-does-not-move-turnover`, `enig.json#long-run-26`, `enig.json#published-rotor-details-aaa`

### ENC-ENIG-005
**Status:** Draft
**Level:** MUST
After stepping, a letter MUST pass through: the plugboard; the right, middle and left rotors; the reflector; the left, middle and right rotors in reverse; and the plugboard again. A rotor with wiring `W` and offset `d` MUST map contact `x` to `(W[(x + d) mod 26] - d) mod 26` on the way in, and use the inverse of `W` with the same `d` on the way back.
**Rationale:** The military signal path. The entry wheel is the identity on this machine, so it adds no step.
**Contract:** `../../standards/enigma/enigma-m3.json` (wiring)
**Vectors:** `enig.json#published-*`, `enig.json#hello-world-aaa`, `enig.json#reflector-c`, `enig.json#ring-does-not-move-turnover`

### ENC-ENIG-006
**Status:** Draft
**Level:** MUST
A plugboard pair MUST swap its two letters, both on the way into the rotors and on the way out. Letters not in any pair MUST pass through unchanged.
**Rationale:** The Steckerbrett is a self-inverse substitution applied at both ends. The Bombe's diagonal board exploited exactly that symmetry.
**Contract:** `keys/enigma-m3.json`
**Vectors:** `enig.json#plugboard-*`, `enig.json#published-barbarossa-decrypt`

### ENC-ENIG-007
**Status:** Draft
**Level:** MUST
Each letter of the text MUST step the machine once and produce exactly one letter.
**Rationale:** The operator pressed one key per letter. Since the text is A-Z only (ENC-TEXT-001), there are no keypresses that skip stepping.
**Contract:** None. This is behavior.
**Vectors:** `enig.json#published-barbarossa-decrypt` (174 letters)

### ENC-ENIG-008
**Status:** Draft
**Level:** MUST
`encrypt` and `decrypt` MUST be the same operation: for the same key, both MUST give the same output for the same input.
**Rationale:** The reflector makes every step's substitution its own inverse, so the same settings encipher and decipher. Operators used one procedure for both.
**Contract:** None. This is behavior.
**Vectors:** `enig.json#barbarossa-encrypt-reciprocal`, `enig.json#hello-world-decrypt-same-operation`, `enig.json#turnover-rotor-iv-v-decrypt`

### ENC-ENIG-009
**Status:** Draft
**Level:** MUST NOT
A letter MUST NOT be enciphered as itself.
**Rationale:** The reflector pairs every letter with a different one, and the plugboard and rotors cannot undo that. This is the weakness the Bletchley Park crib method relied on: any alignment of a guessed word with a matching letter is impossible. An implementation that ever outputs its input letter is wrong.
**Contract:** None. This is behavior.
**Vectors:** Every `ok` vector in `enig.json` (no output letter equals the input letter at the same place). `enig.json#long-run-26` shows it directly.

### ENC-ENIG-010
**Status:** Draft
**Level:** MUST
When a key has more than one fault, the reason MUST be from the first check that fails, in this order: the shape (`key.malformed`); rotor names (`key.unknown-component`); repeated rotors (`key.repeated-rotor`); the reflector (`key.unknown-component`); `rings` and `positions` (`key.not-letters`); the number of pairs (`key.out-of-range`); each pair's letters (`key.not-letters`); conflicting pairs (`key.plugboard-conflict`).
**Rationale:** One predictable reason per key, in the order an operator sets up the machine: pick the rotors, fit the reflector, set the rings and windows, then plug the cables.
**Contract:** None. This is behavior.
**Vectors:** `enig.json#precedence-*`

## Notes

- **Published vectors.** `published-rotor-details-*` are Wikipedia's examples. `published-barbarossa-decrypt` is the first part of a 1941 Barbarossa message, and decrypts to coherent German. Its start position BLA did not appear in the source excerpt the research read. The research supplied it from recollection, and the clean German strongly supports it, but a citation for it is still open (`../../docs/enigma.md`, section 11).
- **Computed vectors.** Every other `ok` vector agrees between two independent throwaway implementations. One of them reads `../../standards/enigma/enigma-m3.json`, so the published vectors also check the vendored data. Reflector C has only computed vectors.
- **The left ring is redundant.** The left rotor never turns another rotor, so its ring setting and window letter only matter through their difference. This follows from ENC-ENIG-005 and needs no rule of its own.
- **Keyspace.** 60 rotor orders x 17,576 start positions x 676 effective ring settings x 150,738,274,937,250 ten-pair plugboards is about 1.07 x 10^23. The widely quoted 1.6 x 10^20 leaves out the rings. The README's total mixes the two (`../../docs/enigma.md`, section 7).
- **Not modeled:** the indicator procedure, Kenngruppen, five-letter groups, and the M4.

## Population assumptions

This spec assumes the operator or population handles the following.

- **Key sheets.** Turning a printed key sheet (ring settings as 01-26, plugs as "AV BS CG") into a key.
- **Message keys and indicators.** Choosing a start position, enciphering it as an indicator, and stripping indicator and Kenngruppe groups from received traffic before decryption.
- **German text conventions.** X for a full stop or space, Q for CH (as in "AQT" for "acht" in the Barbarossa message's "EINSAQTDREINULL", 1830), and doubled X or spelled numbers ("EINS", "NULL"). These belong to `encoded`, and the domain does not interpret them.
- **Grouping.** Five-letter groups on transmission and display.

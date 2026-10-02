*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# Encryption specs

Specs say **what must be true** about the encryption domain, regardless of language or framework. How a nation achieves it is a design, and lives in `nations/{nation}/encryption/designs`. See the root `CLAUDE.md` for the spec-vs-design rule.

## Principle: the cipher sees only what the operator keyed

Historical ciphers ran on letters, after an operator had already turned a written message into something a Morse key or a machine keyboard could carry. The domain keeps that split:

- A **message** holds the text as written (`original`) and the text as keyed (`encoded`, A-Z only). Producing `encoded` from `original` (spelling out numbers, choosing X or spelled-out word breaks, dropping punctuation) is the operator's step. The domain records it and does not perform it.
- A **cipher** sees only A-Z. It never folds case, strips characters, or groups its output. Grouping ciphertext into fives, adding headers, and anything else about how text looks on a tape or a screen are population concerns.

## Spec sets

Specs are organized into named sets. The first is **`core`**, the minimum every nation must meet.

```
specs/
  core/              requirements for the core set
    operator/        operator text preparation, one area per machine
    conformance/     test vectors for the core set, cipher and operator
```

The core set covers the shared rules (text, message, cipher operations) and these ciphers: Caesar, ROT13, Atbash, Affine, both autokey variants, the skytale, the Enigma M3, and the M-209. The Jefferson disk and book cipher follow after prototyping. Their research is already in `../docs`.

Whether those later ciphers and the cracker become **enhancements** (added to core, versioned with it) or **extensions** (separate sets) is deliberately undecided, as in the weather domain. Nothing in core forces that decision.

## Relationship to the rest of the domain

| Path | Holds | Relationship to specs |
|---|---|---|
| `../contracts/domain` | JSON Schemas for `Message`, letters, keys, `CrackResult` | Specs reference these for shape. They do not restate them. |
| `../standards` | Vendored historical hardware data (Enigma wiring and turnover letters, M-209 wheel alphabets), shared by every nation | Specs require it. Nations read it rather than retyping it. |
| `../docs` | Per-cipher research: history, mechanism, ambiguities, sourced vectors | Specs cite it as rationale, never as a requirement. |
| `core/conformance` | Language-neutral test vectors | Every testable requirement has at least one vector. |
| `nations/{nation}/encryption/designs` | Per-nation designs | Each design cites the requirement IDs it answers. |

## Writing requirements

The same rules as the weather specs:

- Use RFC 2119 keywords (**MUST**, **SHOULD**, **MAY**) in capitals, with their standard meanings.
- One testable statement per requirement.
- State outcomes, not mechanisms. "Rejected with reason `key.out-of-range`", never "throws". Do not name language types, frameworks, or libraries.
- Give every requirement a rationale line.

### Requirement IDs

Format: `ENC-{AREA}-{NNN}`. IDs are stable. Once published, an ID is never reused or renumbered. A removed requirement is marked `Withdrawn`, and stays in the file.

| Area | File | Covers |
|---|---|---|
| `TEXT` | `core/text.md` | The letters a cipher accepts and returns, and their numbering |
| `MSG` | `core/msg.md` | The `Message` record: original, encoded, separator |
| `CIPH` | `core/ciph.md` | The three cipher operations, key validation, rejection precedence, determinism, round trip |
| `CAES` | `core/caes.md` | Caesar, ROT13, Atbash |
| `AFF` | `core/aff.md` | Affine |
| `AUTO` | `core/auto.md` | Ciphertext autokey and plaintext autokey |
| `SKY` | `core/sky.md` | Skytale |
| `JEFF` | | Jefferson disk and M-94. **Reserved.** |
| `BOOK` | | Book cipher. **Reserved.** |
| `ENIG` | `core/enig.md` | Enigma M3 (Army and Air Force Enigma I) |
| `M209` | `core/m209.md` | M-209 converter |
| `CRACK` | | Crackers and `CrackResult`. **Reserved.** The contract exists, but it has no requirements yet. |
| `OPER` | `core/operator/oper.md` | The `prepare` operation and token rules shared by every operator |
| `OPCL` | `core/operator/opcl.md` | `classical-x` and `classical-spelled` operators |
| `OPENIG` | `core/operator/openig.md` | `enigma-m3-army` operator |
| `OPM209` | `core/operator/opm209.md` | `m209-army` operator |

New areas are added here before they are used.

### Requirement entry format

```markdown
### ENC-CAES-001
**Status:** Draft
**Level:** MUST
A `caesar` key's `shift` MUST be an integer from 0 to 25. Any other integer MUST be
rejected with reason `key.out-of-range`.
**Rationale:** One written form per key keeps keyspaces countable and crack results comparable.
**Contract:** `keys/caesar.json`
**Vectors:** `caes.json#caesar-accept-*`, `caes.json#caesar-reject-*`
```

Status values are `Draft`, `Accepted`, `Withdrawn`.

## Operations

Cipher operations are defined in `core/ciph.md`, message operations in `core/msg.md`, and `prepare` in `core/operator/oper.md`.

| Operation | Input | Result |
|---|---|---|
| `validateKey` | `cipher`, `key` | `outcome: ok`, or `rejected` with a `reason` |
| `encrypt` | `cipher`, `key`, `text` | `outcome: ok` with `text`, or `rejected` with a `reason` |
| `decrypt` | `cipher`, `key`, `text` | `outcome: ok` with `text`, or `rejected` with a `reason` |
| `validate` | `message` | `outcome: ok`, or `rejected` with a `reason` |
| `roundtrip` | `message` | The message read and written back, as `outcome: ok` with `result` |
| `prepare` | `operator`, `tokens` | `outcome: ok` with `encoded` and `separator`, or `rejected` with a `reason` |

## Reason codes

Reason codes are the language-neutral form of what each nation implements as exceptions, error values, or status codes.

| Reason | Meaning | Defined in |
|---|---|---|
| `cipher.unknown` | The cipher name is not one this set defines | ENC-CIPH-006 |
| `key.malformed` | The key's shape does not match its contract: missing or extra member, wrong type, not an object | ENC-CIPH-007 |
| `key.out-of-range` | A numeric key member, or a count, is outside its allowed range | ENC-CAES-001, ENC-AFF-001, ENC-SKY-001, ENC-ENIG-003, ENC-M209-002 |
| `key.not-invertible` | An affine multiplier has no inverse modulo 26 | ENC-AFF-002 |
| `key.not-letters` | A letter key member is not the letters its cipher requires (an autokey primer, Enigma rings, positions, or a plug pair, M-209 pins or positions) | ENC-AUTO-001, ENC-ENIG-002, ENC-ENIG-003, ENC-M209-001, ENC-M209-003 |
| `key.unknown-component` | A machine part named in the key does not exist on that machine | ENC-ENIG-001 |
| `key.repeated-rotor` | The same rotor is used in two slots | ENC-ENIG-001 |
| `key.plugboard-conflict` | A plug pair joins a letter to itself, or a letter appears in two pairs | ENC-ENIG-003 |
| `key.not-on-wheel` | A letter is not on the wheel it is set for (M-209 wheels skip letters) | ENC-M209-001, ENC-M209-003 |
| `key.repeated-pin` | A wheel's pin letter is listed twice | ENC-M209-001 |
| `key.lug-conflict` | Both lugs of a bar face the same wheel | ENC-M209-002 |
| `text.empty` | The text has no letters | ENC-TEXT-002 |
| `text.not-letters` | The text contains anything other than A-Z | ENC-TEXT-001 |
| `msg.malformed` | The message's shape does not match its contract | ENC-MSG-003 |
| `msg.unknown-separator` | The separator is not `x`, `z` or `spelled` | ENC-MSG-002 |
| `operator.unknown` | The operator name is not one this set defines | ENC-OPER-008 |
| `operator.malformed` | `tokens` is not a list of one-member token objects | ENC-OPER-002 |
| `operator.not-letters` | A `word` or `name` is not one or more of A-Z, a-z | ENC-OPER-003 |
| `operator.not-digits` | A `number` is not one or more of 0-9 | ENC-OPER-004 |
| `operator.unsupported-punctuation` | The operator has no rendering for the mark | ENC-OPER-005 |
| `operator.gap-out-of-range` | A gap is below 0 or above the operator's maximum | ENC-OPER-006 |
| `operator.too-long` | The prepared letters exceed the operator's length limit | ENC-OPENIG-006, ENC-OPM209-004 |

## Conformance vectors

Vectors live in `core/conformance/` as JSON, one file per area. A vector looks like:

```json
{
  "id": "ciphertext-readme-vector",
  "requirements": ["ENC-AUTO-002"],
  "operation": "encrypt",
  "input": {"cipher": "autokey-ciphertext", "key": {"primer": "R"}, "text": "CANGEMINIREADTHIS"},
  "expect": {"outcome": "ok", "text": "TTGMQCKXFWAADWDLD"},
  "source": "domains/encryption/README.MD seed idea, IV 17 (spaces and punctuation removed)"
}
```

Rules:
- `operation` names a domain operation from the table above, never a function in a language.
- `outcome` is `ok` or `rejected`. A rejection carries a `reason` code.
- `source`, when present, says where the expected value was published outside the vector file. Vectors without it were computed.
- Each rejection vector has exactly one fault, except vectors for a precedence rule (ENC-CIPH-008, ENC-AFF-002, ENC-ENIG-010, ENC-M209-010, ENC-OPER-009).
- Vectors describe behavior only. How a nation loads and runs them is part of its design.

**Verification.** Every expected value agrees with two independent throwaway implementations (Python and JavaScript, kept out of the repo, as for the weather unit vectors). Every `encrypt` vector was also decrypted back to its input. Every vector key and message was checked against the JSON Schemas in `../contracts/domain`, and schema validity agrees with the vector's outcome in every case.

## Status of this folder

Drafted for `core`:

1. `core/text.md` (ENC-TEXT-001 to 004) with `conformance/text.json`
2. `core/msg.md` (ENC-MSG-001 to 005) with `conformance/msg.json`
3. `core/ciph.md` (ENC-CIPH-001 to 011) with `conformance/ciph.json`
4. `core/caes.md` (ENC-CAES-001 to 006) with `conformance/caes.json`
5. `core/aff.md` (ENC-AFF-001 to 003) with `conformance/aff.json`
6. `core/auto.md` (ENC-AUTO-001 to 005) with `conformance/auto.json`
7. `core/sky.md` (ENC-SKY-001 to 005) with `conformance/sky.json`
8. `core/enig.md` (ENC-ENIG-001 to 010) with `conformance/enig.json`, using `../standards/enigma/enigma-m3.json`
9. `core/m209.md` (ENC-M209-001 to 010) with `conformance/m209.json`, using `../standards/m209/m209.json`
10. `core/operator/oper.md` (ENC-OPER-001 to 010) with `conformance/oper.json`
11. `core/operator/opcl.md` (ENC-OPCL-001 to 005) with `conformance/opcl.json`
12. `core/operator/openig.md` (ENC-OPENIG-001 to 006) with `conformance/openig.json`
13. `core/operator/opm209.md` (ENC-OPM209-001 to 004) with `conformance/opm209.json`

Still to draft: `CRACK`. `JEFF` and `BOOK` wait until after prototyping, because they need more infrastructure: vendored disk alphabets and a hash-identified key text, plus a rule that makes the operator's choice (disk row, book locator) part of the key.

## Decisions

| Question | Decision |
|---|---|
| Lowercase, spaces, punctuation, digits in cipher text | Rejected (`text.not-letters`). Never folded or stripped. The README ground rules say capitals A-Z only, and turning a message into letters is the operator's step. |
| Empty text | Rejected (`text.empty`). |
| Letter numbering | A=0 through Z=25 everywhere. The research found real errors from mixing A=1 and A=0 (the README autokey numeric line, the M-209 `C = K - P`). |
| Word breaks | Declared per message: `x`, `z` or `spelled`, set by the operator. No single space marker works for every machine, so each machine has an operator spec. Ciphers never see the declaration. |
| Operator specs | In core, one area per machine family, under `core/operator/`. Text preparation first. Indicators come later. |
| Operator input | Marked-up tokens. The operator's judgment (names, kept punctuation, spacing) is the input, and `prepare` does only the mechanical part, so results are reproducible. |
| Decoding | Not specified. Undoing X and Z separators needs a reader's judgment, as it did historically. |
| Out-of-range numeric keys | Rejected, not reduced mod 26. One written form per key. |
| Identity keys (Caesar shift 0, affine 1/0, skytale 1 row) | Accepted. They are part of each cipher's arithmetic, and a cracker's keyspace includes them. |
| ROT13 and Atbash | Separate ciphers with an empty key, not named Caesar or Affine keys. They have no secret, and naming them hides that. |
| README IV for autokey | Written as a one-letter primer (IV 17 is `R`). A keyword primer is the same shape. |
| Which autokey | Both: `autokey-ciphertext` (the README's seed idea) and `autokey-plaintext` (Vigenere's 1586 original). They share a name historically and are easy to confuse, so each has its own identifier. |
| Skytale padding | None. The last row may be short, so ciphertext length equals plaintext length, and decrypt returns the exact plaintext. Padding with X would change the message. |
| Decrypt with a wrong key | Succeeds, returning some text. Classical ciphers have no integrity check, and a cracker needs a scorer, not an error. |
| Public surface | `validateKey`, `encrypt`, `decrypt` only. Inspection is opt-in and off by default (README ground rules). |
| What "M3" means | The Army and Air Force Enigma I: rotors I-V, reflector B or C, single notches, up to 10 plug pairs. The naval rotors VI-VIII had the same weaknesses until the M4, and wait for an M4 spec. |
| Enigma scope | The machine only. The start position is part of the key, and the indicator procedure and sloppy-operator habits are a later operator layer. |
| Enigma wiring | Vendored once in `../standards/enigma`, not restated in specs or retyped by nations. |
| M-209 scope | The converter only. Start positions are the key's internal indicator. Indicator derivation, the letter counter, grouping, and printing Z as a space are operator or display concerns, so decryption returns Z, not a space. |
| M-209 output formula | `C = (K - 1 - P) mod 26` with A=0, the form all four published checks reproduce. |
| Cracker | Contract now (`crack-result.json`). The `CRACK` area is reserved, with no requirements until the ciphers exist. |

## Population assumptions

Each spec file ends with a **Population assumptions** list, as in the weather domain. In this domain the population includes the operator: whoever turns a written message into letters, and whoever reads ciphertext off a tape or a screen. These lists are the to-do list for `populations/{population}/specs`.

## Open questions

- **Operator gaps to fill.** M-209 punctuation and digit words (TM 11-380 1942 and 1944 are online but unread), Enigma umlauts, CENTA, MILLE and MYRIA, and a Kriegsmarine operator for M4.
- **Date-derived IV.** The README suggests `(day + month*k + year) mod 26` with `k` open. If it is adopted, it is a key-derivation operation, and needs a spec of its own.
- **Inspection layer.** What "middleware" exposes (key schedule, rotor positions per step), and whether conformance vectors may use it. It matters most for Enigma.
- **Operator layer.** Enigma indicators (doubled before May 1940, single after), Kenngruppen, the README's "sloppy operator" mode, and the M-209 indicator tape. They likely sit on top of the machines as procedures, not inside them, and may share a design.
- **Corrections to `../README.MD`.** `../docs/README.MD` lists research corrections (the autokey numeric line, the Enigma total keyspace, the M-209 formula), and they are still unapplied. The specs here follow the corrected forms.

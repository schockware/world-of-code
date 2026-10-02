*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# Encryption domain contract

The shapes every nation exchanges: messages, keys, and crack results. Behavior lives in `../../specs`. This folder answers questions about shape ("is `primer` required?"), and the specs answer questions about behavior ("what happens to a lowercase primer?").

`domain/` is the only contract for now. A historical source format such as an Enigma key sheet or an M-209 key list would get its own folder next to it, as weather does with `us` and `international`.

## Schemas

| Schema | What it is | Spec area |
|---|---|---|
| `schemas/letters.json` | Text a cipher works on: one or more of A-Z | `TEXT` |
| `schemas/message.json` | A message as written (`original`), as keyed (`encoded`), and its word-break convention (`separator`) | `MSG` |
| `schemas/cipher-id.json` | Names of the core ciphers | `CIPH` |
| `schemas/keys/{cipher}.json` | One key shape per cipher. Both autokey variants share `autokey.json`. | `CAES`, `AFF`, `AUTO`, `SKY`, `ENIG`, `M209` |
| `schemas/crack-result.json` | One cracker candidate: plaintext, key, score, effort | `CRACK` (reserved) |
| `schemas/operator-id.json` | Names of the operators that prepare text | `OPER` |
| `schemas/tokens.json` | An operator's marked-up message: words, names, numbers, punctuation, gaps | `OPER` |

The cipher operations (`validateKey`, `encrypt`, `decrypt`) take a cipher identifier, a key, and letters, and return letters. They have no wire form yet, so there is no OpenAPI document. One gets added when a webapi nation needs it.

## Decisions recorded in the shapes

| Decision | Where |
|---|---|
| Ciphers see A-Z only. Encoding a written message into letters is the operator's step, and it is kept alongside the original. | `letters.json`, `message.json` |
| The separator depends on the machine (`x`, `z`, `spelled`), and is set by the operator. | `message.json` |
| The operator's judgment is input, as tokens. Nothing in a token is guessed from free text. | `tokens.json` |
| A key is an object per cipher, never a bare number or string. A cipher with no secret (ROT13, Atbash) takes the empty object. | `keys/` |
| Keys are canonical: a Caesar shift is 0-25, not any integer reduced mod 26, so every key has exactly one written form. | `keys/caesar.json`, `keys/affine.json` |
| The README's single-letter IV is written as a primer letter (IV 17 is `"R"`), so one shape covers IVs and keyword primers. | `keys/autokey.json` |
| Enigma keys list the machine left to right, as the operator reads it, with rings and positions as letters, not key-sheet numbers. | `keys/enigma-m3.json` |
| M-209 keys use the key list's own notation: effective pin letters per wheel, and 27 `a-b` bars with no shorthand. | `keys/m209.json` |
| A crack result carries no backend. The backend is how the cracker ran, and the result is what it found. | `crack-result.json` |

## Left out, and why

| Left out | Why |
|---|---|
| Five-letter groups, line breaks, message headers | Presentation of ciphertext for transmission. A population concern, not part of the text. |
| Keyspace size and key enumeration | Belong to cracking. The README limits a cipher to three operations. |
| Date-derived IV | The README's `(day + month*k + year) mod 26` leaves `k` and the date format open. It is a key-derivation procedure, and it waits for a decision. |
| Jefferson disk, book cipher keys | Waiting until after prototyping. Their research is in `../../docs`. |
| M-209 wheel alphabets | Hardware, not key. Vendored in `../../standards/m209`. |
| M-209 repeated pins | Not expressible in JSON Schema without backreferences. ENC-M209-001 enforces it. |
| Enigma wiring | Hardware, not key. Vendored in `../../standards/enigma`. The key names rotors and reflectors, and the standards file says what they are. |
| Enigma plugboard conflicts | A letter used twice, or paired with itself, cannot be expressed in JSON Schema without backreferences, which the recommended pattern subset leaves out. ENC-ENIG-003 enforces it. |

`*.notes.json` files sit next to the schemas they describe, keyed by JSON Pointer, as in the weather domain. `domains/weather/tools/check_notes.py` checks them: `python domains/weather/tools/check_notes.py domains/encryption/contracts`.

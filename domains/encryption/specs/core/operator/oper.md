*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# ENC-OPER: Operator text preparation (core)

Covers `prepare`, the operation that turns an operator's marked-up message into the letters a cipher receives: the **Encoded** state in `../../../README.MD` (Expectations). Each operator's own rules (separator letter, digit words, punctuation, limits) are in its own file:

| Operator | File | Serves |
|---|---|---|
| `classical-x` | `opcl.md` | Caesar, ROT13, Atbash, Affine, autokey, skytale |
| `classical-spelled` | `opcl.md` | The same ciphers |
| `enigma-m3-army` | `openig.md` | `enigma-m3` |
| `m209-army` | `opm209.md` | `m209` |

Research is in `../../../docs/operators.md`.

## Why operators need specs

No cipher here has a space, a digit, or a punctuation mark, so every message has to be turned into letters first, and no one convention works for every machine. The conventions came from three pressures. The cipher's alphabet forced spaces and numbers into letters. The machine set its own conventions: the M-209 printed Z as a space. The fight against cribs added rules: names written twice, length limits, and randomized spacing. Each operator file says which pressure is behind each rule.

## Judgment vs. mechanism

An operator did two kinds of work. **Judgment** covers which words are names, which punctuation is worth sending, what to abbreviate, and where to drop or double a separator. **Mechanism** is everything else: 0600 becomes NULLSEQSNULLNULL, and a full stop becomes X. The spec requires only the mechanism. The judgment arrives already made, as **tokens** (`../../../contracts/domain/schemas/tokens.json`):

| Token | Holds |
|---|---|
| `word` | Letters A-Z, any case |
| `name` | A proper name, letters A-Z, any case |
| `number` | Digits 0-9, as a string |
| `punct` | One punctuation mark |
| `gap` | How many separator letters to place here: the operator's spacing choice |

Turning free text into tokens is the caller's work, which might be a person, a population's interface, or a script.

## Operations

| Operation | Input | Result |
|---|---|---|
| `prepare` | `operator` (from `operator-id.json`), `tokens` | `outcome: ok` with `encoded` and `separator`, or `outcome: rejected` with a `reason` |

`prepare` is the operator's operation, not the cipher's, so ENC-CIPH-001 (three operations per cipher) is unaffected.

## Requirements

### ENC-OPER-001
**Status:** Draft
**Level:** MUST
`prepare` MUST render each token in order and join the renderings with nothing between them. Separators MUST appear only where a `gap` token places them.
**Rationale:** Spacing was a choice, and in 1947 M-209 practice a deliberately random one. An implicit separator would make that choice for the operator.
**Contract:** `tokens.json`
**Vectors:** `oper.json#concatenate-in-order-no-implicit-separator`, `oper.json#gap-tokens-place-separators`

### ENC-OPER-002
**Status:** Draft
**Level:** MUST
`tokens` that is not a list, or a token that is not an object with exactly one member named `word`, `name`, `number`, `punct` or `gap` of the right type, MUST be rejected with reason `operator.malformed`.
**Rationale:** Each token states one decision. A token claiming two kinds is ambiguous.
**Contract:** `tokens.json`
**Vectors:** `oper.json#reject-malformed-*`

### ENC-OPER-003
**Status:** Draft
**Level:** MUST
A `word` or `name` MUST be one or more of the letters A-Z in either case, and MUST be rendered uppercased. Anything else MUST be rejected with reason `operator.not-letters`. A `name` MUST be rendered as a word unless the operator's spec says otherwise.
**Rationale:** Uppercasing is mechanical. Transliterating umlauts, accents or apostrophes varied by operator and era, so it stays with the caller.
**Contract:** `tokens.json`
**Vectors:** `oper.json#word-uppercased`, `oper.json#name-is-word-by-default`, `oper.json#reject-word-*`, `opm209.json#name-is-word`

### ENC-OPER-004
**Status:** Draft
**Level:** MUST
A `number` MUST be one or more digits 0-9, and MUST be rendered digit by digit, using the operator's word for each digit, with nothing between them. Anything else MUST be rejected with reason `operator.not-digits`.
**Rationale:** No machine had digit keys. Digit-by-digit spelling is what the traffic shows (Barbarossa's 1830 is `EINSAQTDREINULL`), and it keeps leading zeros.
**Contract:** `tokens.json`
**Vectors:** `oper.json#number-digit-by-digit`, `oper.json#reject-number-*`

### ENC-OPER-005
**Status:** Draft
**Level:** MUST
A `punct` mark MUST be rendered with the operator's table. A mark not in the table MUST be rejected with reason `operator.unsupported-punctuation`.
**Rationale:** Each procedure named only the marks worth sending. Dropping a mark is the operator's judgment: leave the token out.
**Contract:** `tokens.json`
**Vectors:** `oper.json#reject-unsupported-punctuation`

### ENC-OPER-006
**Status:** Draft
**Level:** MUST
A `gap` of `n` MUST be rendered as the operator's separator letter repeated `n` times. A gap below 0 or above the operator's maximum MUST be rejected with reason `operator.gap-out-of-range`. A gap MAY appear anywhere, including first or last.
**Rationale:** The count is the operator's spacing choice. The maximum comes from each operator's procedure.
**Contract:** `tokens.json`
**Vectors:** `oper.json#gap-tokens-place-separators`, `oper.json#leading-and-trailing-gaps`, `oper.json#reject-gap-negative`

### ENC-OPER-007
**Status:** Draft
**Level:** MUST
When the rendered letters are empty, `prepare` MUST reject the input with reason `text.empty`.
**Rationale:** The result goes to a cipher, which would reject it anyway (ENC-TEXT-002). One reason for one fault.
**Contract:** `letters.json`
**Vectors:** `oper.json#reject-empty-tokens`, `oper.json#reject-only-zero-gap`

### ENC-OPER-008
**Status:** Draft
**Level:** MUST
An operator name not in `operator-id.json` MUST be rejected with reason `operator.unknown`. Names are case-sensitive.
**Rationale:** As for cipher names (ENC-CIPH-006).
**Contract:** `operator-id.json`
**Vectors:** `oper.json#reject-unknown-operator*`

### ENC-OPER-009
**Status:** Draft
**Level:** MUST
When an input has more than one fault, the reason MUST come from the first that applies: the operator name; then the tokens, in order, the first faulty token deciding; then empty output; then the operator's length limit.
**Rationale:** One predictable reason, in the order an operator works: pick up the procedure, write the message piece by piece, then check the result.
**Contract:** None. This is behavior.
**Vectors:** `oper.json#precedence-*`

### ENC-OPER-010
**Status:** Draft
**Level:** MUST
An `ok` result MUST carry `encoded`, which meets ENC-TEXT-001, and `separator`, the operator's value from `message.json`.
**Rationale:** The result is ready to become a `Message` with the caller's `original`. Ciphers accept `encoded` as it is.
**Contract:** `message.json`, `letters.json`
**Vectors:** `oper.json#result-is-message-ready`. Every `ok` vector in the operator files.

## Notes

- **Decoding is not specified.** Turning decrypted letters back into words is the reverse of `prepare`, and it is ambiguous. An X can be a separator, a full stop, or a letter, and an M-209 Z can be a space or a letter. Operators resolved this from experience (`opm209.md`, `../m209.md`). A decoding aid may come later, and it would offer guesses, not requirements.
- **Operators are not tied to ciphers by the domain.** Any operator's output is valid letters for any cipher. The table above says which pairings are historical.

## Population assumptions

This spec assumes the caller or population handles the following.

- **Tokenizing.** Splitting free text into words, names, numbers and punctuation, and deciding which punctuation to keep.
- **Transliterating.** Umlauts, accents, apostrophes, and letters outside A-Z.
- **Choosing gaps.** Including the M-209 1947 rule to vary spacing without a pattern.
- **Splitting long messages.** Into parts under the operator's limit, each with its own key.

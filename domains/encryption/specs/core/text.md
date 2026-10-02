*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# ENC-TEXT: Letters (core)

Covers the text that `encrypt` and `decrypt` accept and return (`../../contracts/domain/schemas/letters.json`). Every core cipher takes and returns this form. A cipher whose ciphertext has a different form, such as a book cipher's numbers, will say so in its own spec.

## Operations

Vectors in `conformance/text.json` use `encrypt` and `decrypt`, defined in `ciph.md`. They use the `caesar` cipher with shift 3 so that only the text varies.

## Requirements

### ENC-TEXT-001
**Status:** Draft
**Level:** MUST
Text given to `encrypt` or `decrypt` MUST consist only of the capital letters A-Z. Text containing anything else MUST be rejected with reason `text.not-letters`. That covers lowercase, spaces, digits, punctuation, line breaks, accented or non-Latin letters, and full-width forms.
**Rationale:** The README ground rules say capitals A-Z only. Folding or stripping would change the message on the operator's behalf, and that step belongs to the operator (see `msg.md`).
**Contract:** `letters.json`
**Vectors:** `text.json#accept-*`, `text.json#reject-not-letters-*`

### ENC-TEXT-002
**Status:** Draft
**Level:** MUST
Empty text MUST be rejected with reason `text.empty`.
**Rationale:** There is no message to encipher, and an empty input would leave each cipher to define its own edge case (an empty skytale grid, an unused autokey primer).
**Contract:** `letters.json` (one or more letters)
**Vectors:** `text.json#reject-empty-*`

### ENC-TEXT-003
**Status:** Draft
**Level:** MUST
Wherever a spec does arithmetic on letters, A MUST be 0 and Z MUST be 25, for plaintext, ciphertext and key letters alike.
**Rationale:** Mixing A=1 and A=0 caused real errors in the research: the README's autokey numeric check, and the M-209's `C = K - P` (see `../../docs/README.MD`). One numbering removes the ambiguity.
**Contract:** None. This is behavior.
**Vectors:** `text.json#numbering-a-is-zero`. Every vector in `caes.json`, `aff.json` and `auto.json` depends on it.

### ENC-TEXT-004
**Status:** Draft
**Level:** MUST
Text returned by `encrypt` or `decrypt` MUST consist only of A-Z, with no grouping, spacing, line breaks or padding added, unless the cipher's spec defines another ciphertext form.
**Rationale:** Five-letter groups and line layout are how ciphertext is presented for transmission, which is a population concern. Shared vectors compare output exactly.
**Contract:** `letters.json`
**Vectors:** Every `ok` vector with a `text` result.

## Notes

- **Regular expressions.** In several regex dialects, a pattern like `^[A-Z]+$` also matches `"HELLO\n"` because `$` can match before a final newline. Both throwaway references hit this. `text.json#reject-not-letters-trailing-newline` catches it, and `letters.json` adds a `not` rule for the same reason.
- **Unicode.** Only the 26 code points U+0041 to U+005A count. Full-width letters, the Kelvin sign, and other look-alikes are rejected. Normalization is the operator's job, not the cipher's.

## Population assumptions

This spec assumes the operator or population handles the following.

- **Preparing text.** Uppercasing, removing punctuation, spelling out numbers, and transliterating (for example Ä to AE, as German operators did) before text reaches a cipher.
- **Presenting ciphertext.** Five-letter groups, line lengths, and headers for transmission or display.
- **Restoring readability.** Turning decrypted letters back into readable text (finding word breaks, re-inserting punctuation) by using the message's `separator` or by reading it.

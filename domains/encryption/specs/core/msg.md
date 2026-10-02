*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# ENC-MSG: Message (core)

Covers the `Message` record (`../../contracts/domain/schemas/message.json`): a message as written (`original`), the letters an operator keyed (`encoded`), and how those letters mark word breaks (`separator`).

The README ground rules call for messages kept as JSON objects, holding the message before encoding and its Morse-era encoding. Morse itself is not emulated yet. `encoded` is the letters an operator would send.

## Terms

| Term | Meaning |
|---|---|
| **Original** | The message as written. Any text, kept for the record. |
| **Encoded** | The message as keyed: A-Z only. This is what goes into a cipher. |
| **Separator** | `x`: words are separated by the letter X. `z`: words are separated by the letter Z (M-209). `spelled`: word breaks and important punctuation are spelled out as words, or omitted. One convention per message, set by the operator that prepared it (`operator/oper.md`). |

## Operations

| Operation | Input | Result |
|---|---|---|
| `validate` | `message` | `outcome: ok`, or `outcome: rejected` with a `reason` |
| `roundtrip` | `message` | The same message read and written back, as `outcome: ok` with `result` |

Each rejection vector in `conformance/msg.json` contains exactly one fault.

## Requirements

### ENC-MSG-001
**Status:** Draft
**Level:** MUST
A message's `encoded` MUST meet ENC-TEXT-001 and ENC-TEXT-002. When it does not, the message MUST be rejected with the same reason the text would get (`text.not-letters` or `text.empty`).
**Rationale:** `encoded` is exactly what a cipher will receive. Reusing the text reasons means one fault has one name everywhere.
**Contract:** `message.json` (`encoded` refers to `letters.json`)
**Vectors:** `msg.json#reject-encoded-*`, `msg.json#accept-x-separator`

### ENC-MSG-002
**Status:** Draft
**Level:** MUST
A message's `separator` MUST be `x`, `z` or `spelled`, in lowercase. Any other value MUST be rejected with reason `msg.unknown-separator`.
**Rationale:** Operators used one convention per message (README ground rules), and the convention depends on the machine, so no single space marker is possible. A closed set lets every nation restore word breaks the same way.
**Contract:** `message.json`
**Vectors:** `msg.json#accept-*-separator`, `msg.json#reject-*-separator`

### ENC-MSG-003
**Status:** Draft
**Level:** MUST
A message that lacks `original`, `encoded` or `separator`, carries any other member, or has a member of the wrong type MUST be rejected with reason `msg.malformed`.
**Rationale:** A closed shape keeps messages exchangeable between nations without guessing at extra fields.
**Contract:** `message.json`
**Vectors:** `msg.json#reject-malformed-*`

### ENC-MSG-004
**Status:** Draft
**Level:** MUST
A message's `original` MUST be carried exactly as given, including whitespace, case, punctuation, non-ASCII characters, and the empty string.
**Rationale:** The original is the record of what was written. The domain does not clean it.
**Contract:** `message.json`
**Vectors:** `msg.json#roundtrip-original-preserved`, `msg.json#accept-empty-original`

### ENC-MSG-005
**Status:** Draft
**Level:** MUST NOT
An implementation MUST NOT reject a message because `encoded` does not appear to derive from `original`.
**Rationale:** Encoding is the operator's judgment (what to spell out, what to drop), and no rule could check it. Operators also made mistakes, and the record keeps them.
**Contract:** None. This is behavior.
**Vectors:** `msg.json#accept-encoded-unrelated-to-original`

## Notes

- **Ciphers never see the separator.** A cipher receives `encoded` as plain letters. The separator tells a reader how to restore word breaks after decryption.
- **An X separator is ambiguous by nature.** With `x`, a real X in a word ("XRAY", "BOX") cannot be told apart from a word break. That was true historically, and readers resolved it in context.
- **M-209 used Z for a space.** The machine printed Z as a space, so its operators keyed Z between words. That is the `z` value, set by the `m209-army` operator (`operator/opm209.md`).

## Population assumptions

This spec assumes the operator or population handles the following.

- **Encoding.** Producing `encoded` from `original`: choosing the separator, spelling out numbers and punctuation, uppercasing, transliterating.
- **Choosing an operator.** Which operator prepares a message. `prepare` (`operator/oper.md`) does the mechanical encoding once the tokens are chosen.
- **Decoding for reading.** Turning decrypted letters back into readable text, using the separator.

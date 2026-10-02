*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# ENC-OPENIG: Enigma Army operator (core)

Covers `enigma-m3-army`, the German Army and Air Force (Heer, Luftwaffe) text conventions for the `enigma-m3` machine. Source: Rijmenants, "Enigma Procedures", checked against the 1941 Barbarossa message (`../../../docs/operators.md`, section 1). Kriegsmarine conventions differ (four-letter groups, Y for a comma) and wait for an M4 operator.

## Requirements

### ENC-OPENIG-001
**Status:** Draft
**Level:** MUST
`enigma-m3-army` MUST use X as its separator letter, with a maximum gap of 1, and its `separator` value MUST be `x`.
**Rationale:** *Alphabet.* Barbarossa traffic separates words with X (`AUFKLXABTEILUNGXVONXKURTINOWA`) and sometimes runs them together (`UHRANGETRETEN`). A double X would read as the end of a name or a quotation.
**Contract:** `operator-id.json`, `message.json`
**Vectors:** `openig.json#barbarossa-time-phrase`, `openig.json#reject-gap-two`

### ENC-OPENIG-002
**Status:** Draft
**Level:** MUST
`enigma-m3-army` MUST spell digits as NULL EINS ZWO DREI VIER FUNF SEQS SIEBEN AQT NEUN.
**Rationale:** *Alphabet.* No digit keys. ZWO avoids confusing ZWEI with DREI. SEQS and AQT follow the CH-to-Q rule. EINS is taken from real traffic (Barbarossa's `EINSAQTDREINULL`) over the procedure page's EINZ.
**Contract:** None. This is behavior.
**Vectors:** `openig.json#german-digits`, `openig.json#barbarossa-time-phrase`

### ENC-OPENIG-003
**Status:** Draft
**Level:** MUST
Every CH in a `word` or `name` MUST be written as Q, after uppercasing.
**Rationale:** *Alphabet and length.* CH is a very common German pair, so writing it as one letter shortens messages. ACHT becomes AQT, and RICHTUNG becomes RIQTUNG, as in the Barbarossa message.
**Contract:** None. This is behavior.
**Vectors:** `openig.json#ch-to-q-*`, `openig.json#barbarossa-richtung`

### ENC-OPENIG-004
**Status:** Draft
**Level:** MUST
`enigma-m3-army` MUST render `.` as X, `,` as ZZ, `?` as FRAGE, `(` and `)` as KLAM, and `"` as X, and support no other marks.
**Rationale:** *Alphabet.* The Army table. FRAGE is chosen from FRAGE, FRAGEZ and FRAQ, the variants the page lists. YY ("point or dot") is left out because it has no mark distinct from the full stop.
**Contract:** None. This is behavior.
**Vectors:** `openig.json#punctuation-table`, `openig.json#full-stop-is-x`, `openig.json#reject-colon`

### ENC-OPENIG-005
**Status:** Draft
**Level:** MUST
A `name` MUST be rendered as X, the name, X, the name, X (XPARISXPARISX).
**Rationale:** *Garbles.* A garbled name cannot be recovered from context, so it was sent twice. It also marked names off from the text around them.
**Contract:** `tokens.json`
**Vectors:** `openig.json#name-delimited-twice`, `openig.json#ch-to-q-in-names`

### ENC-OPENIG-006
**Status:** Draft
**Level:** MUST
`encoded` longer than 250 letters MUST be rejected with reason `operator.too-long`.
**Rationale:** *Cribs and statistics.* Operators were "forbidden to use more than 250 characters in a single message". Longer texts were split into parts, each with its own message key, which limits how much text shares one rotor setting.
**Contract:** None. This is behavior.
**Vectors:** `openig.json#accept-250-letters`, `openig.json#reject-251-letters`, `openig.json#reject-too-long-after-expansion`

## Notes

- **Variants.** The procedure page writes EINZ, FUNF, and FRAGE, FRAGEZ or FRAQ. Practice varied. This spec picks one of each, and the research doc records the others.
- **The doubled X makes decoding harder.** With X as separator, full stop, quotation mark and name delimiter, `NAQXXPARISXPARISX` needs a reader's judgment to undo. That is history, not a defect.
- **Not modeled:** CENTA, MILLE and MYRIA (abbreviations for 00, 000 and 0000), umlauts (not on the source page), five-letter grouping, and Kenngruppen.

## Population assumptions

This spec assumes the caller or population handles the following.

- **Umlauts and ß.** Transliterating them before they become tokens (AE, OE, UE, SS, or dropped, as in FUNF).
- **Splitting.** Messages over 250 letters split into parts.

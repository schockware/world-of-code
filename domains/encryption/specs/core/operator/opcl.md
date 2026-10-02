*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# ENC-OPCL: Classical operators (core)

Covers `classical-x` and `classical-spelled`, the operators for the classical ciphers: Caesar, ROT13, Atbash, Affine, autokey and skytale. No operating procedures for these survive as military systems in the sources read (`../../../docs/operators.md`, section 4), so **every rule here is a project choice, not history**. They follow the two separator conventions in the README ground rules.

## Requirements

### ENC-OPCL-001
**Status:** Draft
**Level:** MUST
`classical-x` MUST use X as its separator letter, with a maximum gap of 1, and its `separator` value MUST be `x`.
**Rationale:** The README ground rules: spaces were encoded as X. More than one X would read as a word containing X.
**Contract:** `operator-id.json`, `message.json`
**Vectors:** `opcl.json#x-*`

### ENC-OPCL-002
**Status:** Draft
**Level:** MUST
`classical-spelled` MUST have no separator letter and a maximum gap of 0, and its `separator` value MUST be `spelled`.
**Rationale:** The README ground rules' other convention: words run together, and important breaks are spelled out (STOP).
**Contract:** `operator-id.json`, `message.json`
**Vectors:** `opcl.json#spelled-*`

### ENC-OPCL-003
**Status:** Draft
**Level:** MUST
Both classical operators MUST spell digits as ZERO ONE TWO THREE FOUR FIVE SIX SEVEN EIGHT NINE.
**Rationale:** The ground rules say numbers were always spelled out. English words are a project choice.
**Contract:** None. This is behavior.
**Vectors:** `opcl.json#english-digits`, `oper.json#number-digit-by-digit`

### ENC-OPCL-004
**Status:** Draft
**Level:** MUST
Both classical operators MUST render `.` as STOP, `,` as COMMA and `?` as QUERY, and support no other marks.
**Rationale:** Telegraph-style words, a project choice. STOP already appears in the message vectors.
**Contract:** None. This is behavior.
**Vectors:** `opcl.json#punctuation-table`, `opcl.json#reject-colon`

### ENC-OPCL-005
**Status:** Draft
**Level:** MUST NOT
Classical operators MUST NOT limit message length.
**Rationale:** No historical limit is known. Classical ciphers have no key schedule that wears out with length, though longer messages do make frequency analysis easier.
**Contract:** None. This is behavior.
**Vectors:** `opcl.json#no-length-limit`

## Population assumptions

None beyond those in `oper.md`.

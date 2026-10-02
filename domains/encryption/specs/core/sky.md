*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# ENC-SKY: Skytale (core)

Covers `skytale`, the rod-and-strip transposition traditionally attributed to Sparta. Whether Spartans used it as a cipher at all is disputed (`../../docs/skytale.md`, section 1). The spec models the traditional account as a grid permutation, not a physical strip.

## Terms

For text of length `L` and a key with `rows` = `r`:

| Term | Meaning |
|---|---|
| **Columns** | `n = ceil(L / r)`, the letters along the rod |
| **Grid** | `r` rows by `n` columns. Cell (row `i`, column `j`) holds plaintext letter `i*n + j` if `i*n + j < L`, and is otherwise empty. |

## Requirements

### ENC-SKY-001
**Status:** Draft
**Level:** MUST
A `skytale` key's `rows` MUST be an integer of at least 1. A smaller integer MUST be rejected with reason `key.out-of-range`.
**Rationale:** A rod has at least one letter around it. No upper bound is set, because whether `rows` is large depends on the text, and key validity must not (ENC-CIPH-008).
**Contract:** `keys/skytale.json`
**Vectors:** `sky.json#reject-rows-*`

### ENC-SKY-002
**Status:** Draft
**Level:** MUST
`skytale` encryption MUST write the plaintext into the grid row by row, and read the ciphertext column by column: columns left to right, each column top to bottom, skipping empty cells.
**Rationale:** Writing along the rod and unwinding the strip reads the columns. This is the convention in the research, and it reproduces its vectors.
**Contract:** `keys/skytale.json`
**Vectors:** `sky.json#encrypt-*`

### ENC-SKY-003
**Status:** Draft
**Level:** MUST
`skytale` encryption MUST NOT add padding: the ciphertext MUST have exactly as many letters as the plaintext.
**Rationale:** Padding with X would change the message, and decryption could not tell padding from a real trailing X. Without padding, the round trip is exact (ENC-CIPH-010).
**Contract:** None. This is behavior.
**Vectors:** `sky.json#encrypt-short-last-row`, `sky.json#encrypt-hello-two-rows`, `sky.json#encrypt-empty-last-row`

### ENC-SKY-004
**Status:** Draft
**Level:** MUST
`skytale` decryption MUST reverse ENC-SKY-002: place the ciphertext into the same grid's non-empty cells column by column, then read it row by row.
**Rationale:** The receiver winds the strip onto a rod of the same size. Because the empty cells are fixed by `L` and `r`, the receiver knows where they are.
**Contract:** `keys/skytale.json`
**Vectors:** `sky.json#decrypt-*`

### ENC-SKY-005
**Status:** Draft
**Level:** MUST
A key with `rows` of 1, or at least the text length, MUST be accepted, and MUST return the text unchanged.
**Rationale:** Both follow from the grid: one row, or one column. Rejecting them would make key validity depend on the text.
**Contract:** `keys/skytale.json`
**Vectors:** `sky.json#rows-*`

## Notes

- **Different keys can give the same cipher.** For 25 letters, `rows` 5 and 6 both give 5 columns, and the same ciphertext (`sky.json#encrypt-empty-last-row` matches `encrypt-square-grid`). A cracker should try column counts, not row counts.
- **Rail fence.** With 2 columns, the skytale equals a 2-rail fence (`sky.json#encrypt-two-columns-equals-rail-fence`). With more, they differ.
- **Keyspace.** At most `L - 1` distinct non-identity permutations for text of length `L`, and fewer once duplicates are removed. Trying every key is trivial.
- **Padded variant.** The research also records an X-padded variant (`WSDOECFNAOLCRVEEEEEXDRAXIETX`). It is not core, for the reason in ENC-SKY-003.

## Population assumptions

None beyond those in `text.md`.

*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Skytale (Spartan Scytale)

Research date: 2026-09-29. "Verified" means the URL was opened (fetched) in this session and the stated facts were read from it. Fetch summaries were produced by a small model, so quotations and citation details that matter should be re-checked against the primary text. "Unverified" means seen only as a web-search snippet, could not be opened (HTTP 403, connection error, unreadable PDF), or is cited from another page's reference list. Priority notes: the evidence debate (section 1) decides how the project should describe the cipher; the ambiguity list (section 5) decides the spec.

This is research and rationale. It contains no requirements.

Spellings vary: scytale, skytale, skutale, plural scytalae or skytalai. This doc uses "skytale" for the device and keeps the sources' spellings in quotations.

---

## 1. History and the evidence debate

### The traditional account
A skytale is a rod of fixed diameter. A strip of leather or parchment is wound spirally around it, the message is written along the rod's length across the coils, and the strip is unwound. Off the rod the letters look scrambled. The recipient wraps the strip on a rod of the same diameter to read it. Plutarch and Aulus Gellius tie this to the Spartan ephors and generals in the field.

### The ancient sources
- Archilochus, 7th century BC: Wikipedia lists him as mentioning the device indirectly, with no detailed description surviving (Wikipedia, verified as read; the fragment itself was not checked).
- Apollonius of Rhodes, 3rd century BC: Wikipedia says he gives "a clear indication of its use as a cryptographic device". The passage was not looked up. Kelly's abstract (below, search snippet) says the cryptographic idea arises only in the 3rd century BC, which is consistent with Apollonius being the first source that reads that way.
- Plutarch, Life of Lysander 19.4-7 (about 100 AD): the fullest account. The ephors make two matching rods, write the message on a strip wound round one and give the other to the general; the story attached is the summoning of Lysander home. Plutarch's word for the writing material is biblion. Wikipedia quotes a translation: "they make a scroll of parchment long and narrow, like a leathern strap, and wind it round their scytale". Verified through the Antigone article and Wikipedia.
- Aulus Gellius, Attic Nights 17.9.6-16 (2nd century AD): technical detail, including that broken and partial letters appear when the strip is unwound. Verified through the Antigone article.
- Thucydides (late 5th century BC): a search snippet says this earlier historian "ostensibly offers a slightly clearer picture" but the picture remains ambiguous. The passages were not identified or opened. Unverified.
- Both full descriptions are from about the 2nd century AD, several centuries after the period of use (5th and 4th centuries BC). The Antigone article states there is no surviving physical evidence of the process.

### The scholarly doubt
- Thomas Kelly, "The Myth of the Skytale", Cryptologia 22(3), July 1998, pp. 244-260. Search-result abstract: the article examines the word skytale in Greek and Latin authors and concludes that the notion that it was a cryptograph used by the Spartans rests on no reliable ancient evidence, that the idea appears only in the 3rd century BC, and that in early authors the skytale is either a plaintext message or a device for keeping records. Wikipedia summarizes Kelly as proposing that the skytale carried plaintext messages and that Plutarch's description is mythological, citing difficulty reconciling Plutarch with earlier accounts. The journal page itself returned HTTP 403, so both are secondhand.
- An article titled "Why did Aeneas Tacticus Never Discuss the Spartan scytale?" (Diepenbroek, Ancient History Bulletin) exists per a search result. The PDF fetch returned undecodable binary, so its argument was not read. Unverified. The title suggests the argument from silence in a 4th-century BC military writer; that is an inference from the title only.
- A book, "The Spartan Scytale and Developments in Ancient and Modern Cryptography" (Bloomsbury), appeared in search results only. Unverified.

Summary for the project: the device is widely described, but whether classical Spartans used it as a cipher, as opposed to a plain message token or authentication, is disputed and cannot be settled from the sources reached here. Calling it "the earliest military cipher" reports a tradition, not an established fact.

## 2. How it works

Let the message be L letters P_0..P_{L-1}. Let the rod present r rows around its circumference and n columns along the length used, so a full grid has r x n cells, with L = r*n after any padding.

Writing: rows run along the rod's length. Cell (row i, column j) holds P_{i*n + j}, for 0 <= i < r and 0 <= j < n.

Reading the strip: the strip winds so that it carries the columns in order. Ciphertext position q (0-indexed) is the cell in row (q mod r), column floor(q / r):

C_q = P_{(q mod r) * n + floor(q / r)}.

The cipher is a fixed permutation of positions determined by (r, n), hence by L and r. Decryption is the inverse: P_{i*n+j} = C_{j*r + i}. Equivalently, the receiver writes the ciphertext column by column into r rows and reads row by row. Letter frequencies are unchanged.

### Relationship to rail fence and columnar transposition
- A skytale is a columnar transposition on an r by n grid in which the columns are read in natural order. A general columnar cipher adds a permutation of the columns as a key.
- Rail fence (zigzag) is a different permutation in general. It writes the plaintext down and up across r rails and reads rail by rail. For WEAREDISCOVEREDFLEEATONCE with 3 rails the result is WECRLTEERDSOEEFEAOCAIVDEN (computed here; matches the usual textbook example).
- The two coincide in the two-rail case. Rail fence with 2 rails gives positions 0, 2, 4, ... then 1, 3, 5, .... A skytale with n = 2 columns (r = L/2 rows, L even) reads column 0 (positions 0, 2, 4, ...) then column 1, the same order. Checked by script on HELPMEIAMUNDERATTACK: both give HLMIMNEATCEPEAUDRTAK. For 3 or more rails they differ.
- A skytale with (r, n) is the inverse of a skytale with the dimensions swapped: decrypting with the wrong-way dimensions is what a brute-force attacker tries.

## 3. Key and keyspace

The key is the rod circumference, the number of rows r. For a message of length L, r ranges over 2..L-1 with a non-trivial result (r = 1 or r = L is the identity). When padding to a full grid is used, n = ceil(L/r) follows from r. Under 5 bits for L = 25: r in 2..24 is 23 values, log2(23) is about 4.5 bits. If r must divide L (no padding), the keyspace is the divisor count: for L = 28 the divisors between 2 and 14 are 2, 4, 7, 14, so 4 keys.

The project's brief lists rail fence and scytale together as "Tiny" keyspace with "Try all key sizes" as the break, which agrees.

## 4. Known attacks

- Exhaustive search over r: decrypt with each candidate and keep the output that reads as language. Computed example: ciphertext WSDOECFNAOLCRVEEEEEXDRAXIETX (28 letters, true r = 4). Decrypting with r = 2 gives WDEFALREEEDAITSOCNOCVEEXRXEX, r = 4 gives WEAREDISCOVEREDFLEEATONCEXXX, r = 7 gives WNERSAEADOEXOLEIECEECRXTFVDX. Only r = 4 is readable. (Dimensions that do not divide the length were not tried.)
- Frequency check: a transposition keeps the plaintext letter frequencies, so a ciphertext with language-like frequencies signals transposition rather than substitution.
- Known plaintext: one known word at a known place, or a repeated stride between its letters in the ciphertext, fixes r.
- With so few keys, the cost is negligible at any message size.

## 5. Implementation ambiguities a spec must pin down

1. Meaning of the key: r as rows (letters per turn) or as columns (letters along the rod). The two conventions produce ciphertexts that are each other's inverse.
2. Reading order: rows first or columns first when writing; columns read left to right or right to left; top to bottom or bottom to top within a column (a strip can be wound either way).
3. Padding: a full grid needs L = r*n. Options are a fixed pad character (X in the vectors here), random letters, or a short last row. The rule must also say how padding is recognized and removed on decrypt.
4. Incomplete last row: if left short, columns have different lengths and the decryptor must know which cells are empty. n = ceil(L/r) is then part of decryption. Computed unpadded ciphertext for the 25-letter message at r = 4: WSDOECFNAOLCRVEEEEEDRAIET, versus the padded WSDOECFNAOLCRVEEEEEXDRAXIETX.
5. Whole padding rows: with L = 25 and r = 6, n = 5 and there are 30 cells; ceil(25/6) = 5 gives n = 5, so only 5 padding letters, but for other (L, r) pairs the padding may fill an entire row. Whether that is allowed or such r values are rejected is a design choice.
6. Degenerate keys: r = 1, r >= L, and r not fitting the length.
7. Non-letters: whether spaces and punctuation are written on the strip or stripped and restored; case.
8. Alphabet: a skytale works on any symbol sequence; is the domain letters only (A-Z, as elsewhere in this project) or arbitrary characters.
9. Long messages: whether the message is split into blocks of r*n or one grid grows along n.
10. Wrong key behavior: every r produces some output, so decrypt cannot detect a wrong key.
11. Physical fidelity: Plutarch and Gellius describe letters broken across the coils; modeling that needs geometry, not only a grid. Whether to model a pure permutation or a physical strip.

## 6. Test vectors

Computed with `vec.py` in the scratchpad directory. Convention: r rows, n = ceil(L/r) columns; plaintext written row by row; ciphertext read column by column, top to bottom within a column and columns left to right; padded with X to fill the last row. The round trip (decrypt returns the padded plaintext) was verified. No primary-source vectors exist: there is no surviving artifact and the sources give no worked example.

| Plaintext | L | r | n | Ciphertext |
|---|---|---|---|---|
| HELPMEIAMUNDERATTACK | 20 | 4 | 5 | HENTEIDTLAEAPMRCMUAK |
| WEAREDISCOVEREDFLEEATONCE | 25 | 5 | 5 | WDVFTEIELOASRENRCEECEODAE |
| WEAREDISCOVEREDFLEEATONCE | 25 | 4 | 7 | WSDOECFNAOLCRVEEEEEXDRAXIETX (3 padding X) |
| HELLO | 5 | 2 | 3 | HLEOLX (1 padding X) |
| HELPMEIAMUNDERATTACK | 20 | 10 | 2 | HLMIMNEATCEPEAUDRTAK (equals rail fence, 2 rails) |

Short-last-row variants under the same convention: WEAREDISCOVEREDFLEEATONCE with r = 4 gives WSDOECFNAOLCRVEEEEEDRAIET; HELLO with r = 2 gives HLEOL.

Rail fence cross-check: WEAREDISCOVEREDFLEEATONCE with 3 rails gives WECRLTEERDSOEEFEAOCAIVDEN.

## 7. References

1. Wikipedia, "Scytale". https://en.wikipedia.org/wiki/Scytale . Verified (fetched; summarized by a small model). Archilochus, Apollonius and Plutarch mentions, the Plutarch quotation, and the Kelly summary with the Kelly 1998 and Russell 1999 citations.
2. Antigone, "Ancient Cybersecurity? Deciphering the Spartan Scytale" (2021). https://antigonejournal.com/2021/06/deciphering-spartan-scytale/ . Verified. Plutarch Lysander 19 and Gellius Attic Nights 17.9 citations, no surviving physical evidence. The fetch reported that it does not discuss Archilochus, Apollonius, Thucydides or Aeneas Tacticus.
3. Kelly, T., "The Myth of the Skytale", Cryptologia 22(3), 1998, pp. 244-260. https://www.tandfonline.com/doi/abs/10.1080/0161-119891886902 . Unverified: HTTP 403 on fetch; abstract seen only in a search result (a Semantic Scholar page also failed with a connection error).
4. Diepenbroek, "Why did Aeneas Tacticus Never Discuss the Spartan scytale?", Ancient History Bulletin. https://www.ancienthistorybulletin.org/subscribed-users-area/wp-content/uploads/2022/12/Diepenbroek.pdf . Unverified: fetched but the PDF came back as unreadable binary; only the title was seen.
5. Plutarch, Life of Lysander 19; Aulus Gellius, Attic Nights 17.9; Apollonius of Rhodes; Thucydides. Unverified: primary texts not opened; passages cited as given by Antigone and Wikipedia.
6. The Spartan Scytale and Developments in Ancient and Modern Cryptography (Bloomsbury), and historyofinformation.com "The Skytale". Unverified: search results only.

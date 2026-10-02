*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Book Cipher

Research date: 2026-09-29. "Verified" means the URL was opened (fetched or returned by search) in this session and the stated facts were read from it. Fetch summaries were produced by a small model, so details that matter to a spec should be re-checked against the primary source. Test vectors marked "computed" were produced by a script in this session. These are research notes and rationale, not requirements.

---

## 1. History

- Wikipedia (verified) describes the general method, uses from the Revolutionary War (Benedict Arnold with Blackstone's commentaries) to WWII, and Eli Cohen's use of *The Three Musketeers*. Details of each episode were not checked against primary sources.
- The Beale ciphers are the famous example. Wikipedia (verified): three ciphertexts in an 1885 pamphlet, *The Beale Papers*, published by James B. Ward, about treasure said to be buried by Thomas J. Beale in Bedford County, Virginia around 1820; the story runs Beale, 1822 iron box left with innkeeper Robert Morriss, opened 1845. Cipher No. 2 was solved using the Declaration of Independence as the key text; ciphers 1 and 3 are unsolved. The same source lists reasons for skepticism (anachronistic words such as "stampeding", no confirmed Thomas J. Beale in census records, statistical patterns suggesting invention, Ward as likely author); the whole story may be a hoax.
- Search results (verified) say cipher 2 has about 800 numbers beginning 115, 73, 24, 807, 37 ..., each number naming a word of the Declaration whose first letter is used, and give the decoded text starting "I have deposited in the county of Bedford, about four miles from Buford's, in an excavation or vault, six feet below the surface of the ground ...". I did not re-run the decipherment because the exact edition of the Declaration was not obtained (see section 5).
- Ciphers 1 and 3 have been discussed in a recent IACR ePrint paper, "Beale Cipher 1 and Cipher 3: Numbers With No Messages" (https://eprint.iacr.org/2024/695.pdf), found via search; I did not open it, so I make no claims about its content.

## 2. How it works

A book cipher is a substitution in which the key is a shared text (the "book") and each plaintext unit is replaced by a locator into that text. Let the key text be indexed by a locator scheme L; `pos(u)` is a locator whose text at that place equals `u`.

- Word-based: `E(word) = any locator i such that book word i = word` (index from 1 or 0 by convention). Decrypt: `D(i) = book word i`. Fails if a word is absent from the book.
- Letter-based (first-letter, as in Beale No. 2): `E(c) = any locator i such that first letter of book word i = c`, `D(i) = first letter of book word i`. Letter-in-text variant: locator is a position among all letters of the book. Wikipedia (verified) describes the letter-based form as technically a homophonic substitution cipher.
- Structured locators: triples such as page/line/word or chapter/verse/word (verified via Wikipedia) that avoid counting from the beginning.
- Encryption is one-to-many: the sender picks one of several eligible locators, and this choice is not determined by the cipher. Decryption is deterministic. So the ciphertext is not a function of (plaintext, key) alone; a spec must state the selection rule for reproducible vectors (for example lowest locator, highest locator, or a supplied random source).

Code book versus book cipher: a code book is a fixed dictionary mapping whole words, phrases or names to code groups (numbers or five-letter groups), prepared for the purpose and distributed as a secret; it needs a distinct entry for everything to be said. A book cipher uses an existing ordinary text, indexed by position, as the key; the book was not written for cryptography, and its coverage of the message's vocabulary is accidental. The distinction is my summary of standard usage; the fetched Wikipedia summary does not itself draw it.

## 3. Key and keyspace

- The key is the book edition together with the locator scheme. It is not a small integer. The keyspace is the number of candidate books and editions the adversary must consider, which is bounded by what the adversary can obtain; a well-known text is a weak key.
- For a fixed book, the number of ciphertexts for one message is the product over its units of the number of eligible locators (in the vector below, 1 x 4 x 6 x 4 x 1 x 6 x 4 x 6 = 13,824 ciphertexts for "BIT OF WIT" without spaces).
- A cracker harness cannot iterate a keyspace of this kind directly; it needs a candidate-book list.

## 4. Known attacks

- Obtain the key text: identify the book (from context, the sender's possessions, or by trying well-known texts). Wikipedia (verified) says book ciphers are quite easily broken without sophisticated means.
- Partial solution: guess a few words, see which locator numbers correspond, and infer the locator scheme; letters/words at nearby numbers reveal the book's structure.
- Statistical: in a first-letter scheme, locator frequencies follow the frequencies of initial letters in the book, so the number distribution leaks the alphabet mapping. Beale cipher 2 was solved by finding the right text (verified via search).
- Edition mismatch is an accidental attack: an adversary with a different edition gets garbage; this is the same effect that lets a receiver fail (see below).
- Unsolved Beale ciphers 1 and 3 show that lacking the text (or the text existing at all) defeats the attack.

## 5. Implementation ambiguities a spec must pin down

1. The book: exact edition, text encoding, translation, printing. Wikipedia (verified) states sender and receiver must use exactly the same edition. For a repository the book should be a vendored, hash-identified text, not a title.
2. Tokenisation: what is a word (hyphens, apostrophes, numerals, "times," with trailing comma); case folding; whether headings, footnotes, page numbers and front matter count. Beale-derived accounts mention modified/corrected numbering of the Declaration (search summary said "modified version"; not investigated).
3. Index base: 1-based versus 0-based, and counting from the start of the book versus from a page/line origin.
4. Locator granularity: word, first letter of word, letter position, page/line/word triples.
5. Choice rule among multiple locators: lowest, highest, cyclic, pseudo-random with a stated seed. Without this, no fixed ciphertext vectors exist. The cipher's tests may need round-trip checks (decrypt equals plaintext) rather than ciphertext equality.
6. Missing letters/words: reject, substitute, spell out, or fall back to a null. In the vector below C, K, D and N do not occur as initial letters, so "ATTACKATDAWN" cannot be encoded.
7. Non-letters, spaces and digits: dropped, passed through, or coded.
8. Ciphertext syntax: separators, number format, and whether locators are separate tokens.

## 6. Test vectors

Key text (24 words, words numbered from 1, punctuation ignored for tokenisation): "It was the best of times, it was the worst of times, it was the age of wisdom, it was the age of foolishness".

Word index: 1 It, 2 was, 3 the, 4 best, 5 of, 6 times, 7 it, 8 was, 9 the, 10 worst, 11 of, 12 times, 13 it, 14 was, 15 the, 16 age, 17 of, 18 wisdom, 19 it, 20 was, 21 the, 22 age, 23 of, 24 foolishness. The text is my paraphrase of the Dickens opening (public domain), used only as a self-contained key; it is not a claim about any edition.

All computed by script.

First-letter scheme, plaintext BITOFWIT (spaces removed):
- Eligible locators per letter: B {4}; I {1, 7, 13, 19}; T {3, 6, 9, 12, 15, 21}; O {5, 11, 17, 23}; F {24}; W {2, 8, 10, 14, 18, 20}.
- Lowest-locator rule: `4 1 3 5 24 2 1 3`.
- Highest-locator rule: `4 19 21 23 24 20 19 21`.
- Both decrypt to BITOFWIT.

Word-based scheme, plaintext "it was the age of wisdom": eligible locators are it {1, 7, 13, 19}; was {2, 8, 14, 20}; the {3, 9, 15, 21}; age {16, 22}; of {5, 11, 17, 23}; wisdom {18}. Lowest-locator ciphertext: `1 2 3 16 5 18`. Highest: `19 20 21 22 23 18`.

Failure case: ATTACKATDAWN under the first-letter scheme cannot be encoded because C, K, D and N are not initial letters in the key text.

Beale No. 2 (sourced, search result, not independently reproduced): the numbers begin 115, 73, 24, 807, 37 and decode against the Declaration of Independence to text beginning "I have deposited in the county of Bedford". The key edition is the open issue, so I list it as a reference vector, not a computed one.

## 7. References

- Book cipher, Wikipedia. https://en.wikipedia.org/wiki/Book_cipher . Verified (fetched; definition, variants, edition requirement, Arnold, Cohen, weakness).
- Beale ciphers, Wikipedia. https://en.wikipedia.org/wiki/Beale_ciphers . Verified (fetched; 1885 pamphlet, Ward, cipher 2 solved with the Declaration, skepticism).
- Simon Singh, The Beale Treasure Ciphers. https://simonsingh.net/media/articles/maths-and-science/the-beale-treasure-ciphers/ . Appeared in search results only; not opened. Unverified.
- Beale Papers transcription, The Cipher Foundation. http://cipherfoundation.org/older-ciphers/beale-papers/beale-papers-transcription/ . Search results only; not opened. Unverified.
- How Cipher No. 2 Was Decoded, bealetreasurestory.com. http://www.bealetreasurestory.com/id26.html . Search results only; not opened. Unverified (the "115, 73, 24, 807, 37" and decoded-text facts came from the search summary, which cited several of these sites).
- "Beale Cipher 1 and Cipher 3: Numbers With No Messages", IACR ePrint 2024/695. https://eprint.iacr.org/2024/695.pdf . Search results only; not opened. Unverified.
- Code book versus book cipher distinction: my own summary, no source fetched. Unverified.

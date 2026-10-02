*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Jefferson Disk (Wheel Cypher) and the US Army M-94

Research date: 2026-09-29. "Verified" means the URL was opened (fetched) in this session and the stated facts were read from it. Fetch summaries were produced by a small model, so anything that matters to a test vector or a spec (especially the disk alphabets below) should be re-checked against a primary document before being frozen. "Unverified" means the source was seen only as a web-search snippet, or could not be opened (HTTP 403, connection error, or unreadable PDF), or is cited from another source's citation list. Priority notes: the M-94 alphabets (section 4.2) and the ambiguity list (section 6) matter most for a spec; history is context only.

This is research and rationale. It contains no requirements.

---

## 1. History

- Thomas Jefferson described a wheel cypher in the early 1790s while Secretary of State (Monticello search snippet; Wikipedia says "1790s"). His manuscript, held in the Library of Congress Jefferson Papers ("Cipher Wheel, Notes and Copy"), reportedly specifies 36 wheels, each with the 26 letters in a different random order, on a common spindle; Wikipedia's summary says he estimated 36 to 48 wheels. No device is known to have been built for actual use. The manuscript was found in 1922 by Edmund C. Burnett (Wikipedia; not checked against the manuscript).
- Jefferson reportedly dropped the idea around 1803 after learning of columnar transposition from Robert Patterson (Wikipedia only).
- Etienne Bazeries independently reinvented a cylinder cipher around 1891 (20 disks in Wikipedia's account). Gaetan de Viaris is said to have solved a Bazeries cylinder in 1893 (Wikipedia).
- US Army: Col. Parker Hitt and Maj. Joseph Mauborgne developed the M-94 (25 aluminum disks, 26 letters each) between 1916 and 1917; introduced to the Army around 1921-1922; replaced by the M-209 around 1942 (Wikipedia M-94, Crypto Museum). The Navy and State Department used a strip variant, the M-138-A.
- Wikipedia says the NSA holds an incomplete 19th-century device with 35 surviving disks (originally 40) and 42 characters per disk. The Monticello page is said to state that only one original Jefferson wheel is known (search snippet, unverified). These were not reconciled here.

"Jefferson disk", "Jefferson wheel cypher", "Bazeries cylinder", "cylinder cipher" and "M-94" name one family. Jefferson's is the earliest known description here, not necessarily the earliest invention (Wikipedia mentions a 1786 Swedish device by Fredrik Gripenstierna with 57 disks, unverified).

## 2. How it works

Let the alphabet be A-Z, indexed 0..25. Let there be a set of disks D_1..D_m. Each disk D_j is a permutation of the alphabet, written as the sequence of 26 letters around its rim: D_j = (D_j[0], ..., D_j[25]). Indices on a disk are taken modulo 26 (it is a ring).

Key: an ordered selection of n distinct disks, written (j_1, ..., j_n). Position p (1..n) on the spindle holds disk j_p. A message block has exactly n letters.

Encryption of a block P = (P_1..P_n) with an offset k in {1..25}:
1. For each position p, let i_p be the unique index with D_{j_p}[i_p] = P_p. Each disk is rotated so that P_p lies on the reference line. This is the plaintext row.
2. The ciphertext letter at position p is C_p = D_{j_p}[(i_p + k) mod 26], the letter k places along the rim, in a fixed direction, from the plaintext letter. One k applies to all positions because the sender reads a single row of the cylinder, and the rows are k places apart on every disk.

The other 25 rows of the cylinder are the 25 candidate ciphertexts; the sender picks one. Offset k = 0 is the plaintext row itself.

Decryption: set the ciphertext on the reference line. Then P_p = D_{j_p}[(i'_p - k) mod 26], where i'_p is the index of C_p. The receiver does not need k: all other rows are examined and the one that reads as language is the plaintext. This "recognize the plaintext" step is a human judgment, not a mathematical step, and it is what allows the sender to choose k freely without transmitting it.

Longer messages are split into blocks of n letters. The last block is short unless padded (section 6).

Equivalently: each position is a mixed-alphabet Caesar shift, C = D_j[(D_j^{-1}[P] + k) mod 26], with the shift measured along the disk's own ordering. Positions use different alphabets, so the whole is a polyalphabetic cipher with period n (within a block).

## 3. Key and keyspace

Arithmetic (exact integers, computed in the scratchpad script):

- 36 disks, all used, order is the key: 36! = 371,993,326,789,901,217,467,999,448,150,835,200,000,000, about 3.72 x 10^41, which is 2^138.09.
- 25 disks (M-94), order is the key: 25! = 15,511,210,043,330,985,984,000,000, about 1.55 x 10^25, which is 2^83.68 ("about 84 bits"; Wikipedia gives the same 25! value).
- 10 disks (Wikipedia toy example): 10! = 3,628,800.
- Choosing 30 of 100 strips in order, as the M-138-A does: C(100,30) x 30! = 7,791,097,137,057,804,874,587,232,499,277,321,440,358,327,700,684,800,000,000, about 7.79 x 10^57. (The 100 strips and 30 selected are from Wikipedia; the computation is ours.)
- The offset k (25 choices) is chosen per message and travels implicitly with the ciphertext, so it adds no key strength.
- The disk alphabets are fixed by the manufacturer for the M-94 and are not part of the per-message key. If they were secret and free, each disk would be one of 26! permutations.

These counts are brute-force bounds, not security levels; see attacks.

## 4. Disk data

### 4.1 Jefferson's 36 alphabets

Not found. The Monticello encyclopedia (HTTP 403, unverified), the Library of Congress item page (HTTP 403, unverified) and ciphermachines.com (fetched; it gives no alphabets) did not yield them, and Wikipedia does not list them. Reports describe Jefferson's alphabets as random. Whether his manuscript writes any out is unconfirmed. A Library of Congress image page (mtj1.056_0048_0051, seen in search results only) is where a primary check should start. This doc records no Jefferson alphabets; an implementation calling itself "Jefferson's disks" would have to generate its own.

### 4.2 M-94 alphabets (25 disks)

Source: https://www.prc68.com/I/M94.shtml (fetched; read via a summarizing model). The page says the data come from "Cryptologia M-94 issues" photographs (issue not specified) and notes that disk 7 had "a fixed typo 6/23/01". Cross-checks: the Crypto Museum page (fetched) gives R-17 as ARMYOFTHEUSZJXDPCWGQIBKLNV and Y-24 as AYJPXMVKBQWUGLOSTECHNZFRID, matching. The prc68 test-message page (fetched) says it lists the same 25 alphabets, but it was not compared letter by letter. Each row below was script-checked to be a permutation of A-Z. That does not prove it matches the historical wheel: a single swap typo would still pass. Treat as good but unaudited.

Wheel stamp = the letter following A on that wheel, and the number is its position in the set (B-1 through Z-25).

| Wheel | Alphabet |
|---|---|
| B-1 | ABCEIGDJFVUYMHTQKZOLRXSPWN |
| C-2 | ACDEHFIJKTLMOUVYGZNPQXRWSB |
| D-3 | ADKOMJUBGEPHSCZINXFYQRTVWL |
| E-4 | AEDCBIFGJHLKMRUOQVPTNWYXZS |
| F-5 | AFNQUKDOPITJBRHCYSLWEMZVXG |
| G-6 | AGPOCIXLURNDYZHWBJSQFKVMET |
| H-7 | AHXJEZBNIKPVROGSYDULCFMQTW |
| I-8 | AIHPJOBWKCVFZLQERYNSUMGTDX |
| J-9 | AJDSKQOIVTZEFHGYUNLPMBXWCR |
| K-10 | AKELBDFJGHONMTPRQSVZUXYWIC |
| L-11 | ALTMSXVQPNOHUWDIZYCGKRFBEJ |
| M-12 | AMNFLHQGCUJTBYPZKXISRDVEWO |
| N-13 | ANCJILDHBMKGXUZTSWQYVORPFE |
| O-14 | AODWPKJVIUQHZCTXBLEGNYRSMF |
| P-15 | APBVHIYKSGUENTCXOWFQDRLJZM |
| Q-16 | AQJNUBTGIMWZRVLXCSHDEOKFPY |
| R-17 | ARMYOFTHEUSZJXDPCWGQIBKLNV |
| S-18 | ASDMCNEQBOZPLGVJRKYTFUIWXH |
| T-19 | ATOJYLFXNGWHVCMIRBSEKUPDZQ |
| U-20 | AUTRZXQLYIOVBPESNHJWMDGFCK |
| V-21 | AVNKHRGOXEYBFSJMUDQCLZWTIP |
| W-22 | AWVSFDLIEBHKNRJQZGMXPUCOTY |
| X-23 | AXKWREVDTUFOYHMLSIQNJCPGBZ |
| Y-24 | AYJPXMVKBQWUGLOSTECHNZFRID |
| Z-25 | AZDNBUHYFWJLVGRCQMPSOEXTKI |

Wheel R-17 begins ARMYOFTHEUS ("ARMY OF THE US"), the one non-random-looking wheel. The order above is the reading order along the rim as the source prints it; the source does not say which physical direction "one row further" runs (section 6). With 25 disks, an M-94 block is 25 letters (Wikipedia: 25-letter blocks).

### 4.3 Wikipedia toy example (10 disks)

Wikipedia's Jefferson disk page gives a 10-disk illustrative Bazeries-style set and says the offset between plaintext and ciphertext letters is the same on every disk (six in its example). The alphabets, as returned by the summarizing fetch:

1 ZWAXJGDLUBVIQHKYPNTCRMOSFE; 2 KPBELNACZDTRXMJQOYHGVSFUWI; 3 BDMAIZVRNSJUWFHTEQGYXPLOCK; 4 RPLNDVHGFCUKTEBSXQYIZMJWAO; 5 IHFRLABEUOTSGJVDKCPMNZQWXY; 6 AMKGHIWPNYCJBFZDRUSLOQXVET; 7 GWTHSPYBXIZULVKMRAFDCEONJQ; 8 NOZUTWDCVRJLXKISEFAPMYGHBQ; 9 XPLTDSRFHENYVUBMCQWAOIKZGJ; 10 UDNAJFBOWTGVRSCZQKELMXYIHP.

Each was script-checked as a permutation. It is an illustration, not a historical device, and it passed through a small model, so re-check against the page before use.

## 5. Known attacks

- Known plaintext or a crib: for each position, a (plaintext, ciphertext) letter pair constrains which disk sits there and which k was used; only some disk and k combinations are consistent, and over several known blocks with the same key the wheel at each position is identified. Wikipedia names the constant offset as "one major weakness" (fetched).
- Depth: blocks enciphered with the same disk order are, position by position, the same mixed-alphabet shift with different k values. With enough same-key messages, position-wise analysis applies. Friedman's 1918 "Several Machine Ciphers and Methods for their Solution" (cited on Wikipedia; not opened) is the classic treatment.
- The unknown k does not help against someone who holds the disks and key: they try all 25 rows and read the one that is language.
- Bazeries cylinders were reportedly solved by de Viaris in 1893 (Wikipedia, unverified beyond that page).
- Keyword-derived orders: Crypto Museum (fetched) says the M-94 disc order was usually derived from a key word, repeated if shorter than 25 characters. A short keyword is a far smaller keyspace than 25! in practice.

## 6. Implementation ambiguities a spec must pin down

1. Number and identity of disks: Jefferson wrote 36 (estimated 36-48), the M-94 used 25, Bazeries used 20, the toy uses 10. Is the block length n the number of disks selected, and is n fixed?
2. Alphabets: no Jefferson alphabets were found; the M-94 set in 4.2 is the only historical set located. Fixed data, or supplied per instance? Wheel identification (stamp letter and number) is part of the data.
3. Disk order as key: a full permutation, or an ordered selection of n from m (M-138-A style)? How a keyword maps to an order (Crypto Museum says only "usually derived from a key word"; the mapping is not stated on the fetched page).
4. Row-offset choice: which of the 25 rows is used and who picks it (random by the sender in the historical procedure). Does an implementation take k as input, draw it at random, or report all 25 rows? Random choice makes encryption non-deterministic, which matters for test vectors.
5. Offset direction: does row k mean k places later or earlier along the rim. This depends on how a disk is read.
6. Whether k = 0 (the plaintext row) is rejected.
7. Decryption: does it take k, or return all 25 candidate rows for a human to choose? An automated version needs a scoring function to pick.
8. Input rules: case, spaces, punctuation, digits. The M-94 alphabet is A-Z with no I/J merge.
9. Block handling: length not a multiple of n; padding letter and rule, or a short final block using only the first p disks; how to mark padding.
10. Empty message, or a message shorter than n.
11. Spindle position numbering (first disk at which end).
12. Multiple blocks: whether the same disk order is reused for every block (M-94 practice appears to be one order per message period, but this was not confirmed) or the order changes.

## 7. Test vectors

Computed with `vec.py` in the scratchpad directory, using the M-94 alphabets from 4.2. No primary-source cipher/plaintext pairs were found: the prc68 test-message page (fetched) gives 25 ciphertexts and the alphabets, but per the summary does not give the disk order, row or offset, and shows only plaintext fragments (message 1 begins "chlorine and oxygen have not b..."). Those messages are not usable as vectors without the SRH-366 document that page mentions (not opened).

Convention used: the wheel at position p is wheel number order[p] from 4.2 (numbering is the second column of the Wheel names, B-1 = 1). The plaintext letter is found at index i on its wheel in the printed order of 4.2, and the ciphertext letter is at index (i + k) mod 26. Round trip verified for every row below. This is one choice among the ambiguities in section 6, not a historical fact.

Plaintext (25 letters): ATTACKATDAWNFROMTHENORTHX

Order = wheels 1 through 25:

| k | Ciphertext |
|---|---|
| 1 | BLVEYVHDSKDFESWWHAKHXJUNT |
| 6 | DYKFMGBPVFGCIDLXJNQGSXMDD |
| 13 | HRERNUOVYTAZGUVKQLFRLACVW |
| 25 | NKRSHFWGJCUMPYXIFXSSGNDCE |

Order = wheels 25 down to 1:

| k | Ciphertext |
|---|---|
| 1 | ZEUWLATFPQFYEDHTZPZDPUVFS |
| 6 | HFMLPXFHITJAIAZVYKPBRTKLB |
| 13 | GPCRGBCERVHVGGBCXEDMEAEZF |
| 25 | ISDYQCQYXYOGPSNNVIJRDMRER |

Order = 24 17 13 2 8 23 3 20 11 15 19 22 5 25 7 21 12 16 1 14 10 9 4 6 18 (a shuffle from a fixed seed in the script):

| k | Ciphertext |
|---|---|
| 1 | YHSCVWDRIPHRNCGUBDIYNANWH |
| 6 | VJOIETUYKYRMOOLZXPVAQQSFC |
| 13 | LQCUGLCSLTDYHZANWBKVWFFGP |
| 25 | DFZBKXLUWMGKAGRJJSCGHCPZW |

Toy set (4.3), disks 1 to 10 in order, same convention, plaintext HELLOWORLD: k=1 KLONTPNJTN; k=6 CDMFDBTSHO; k=25 QBPPUIEVPU. Disks 10 down to 1, k=6: JMFFARVFTQ. These depend on the toy data being transcribed correctly by the summarizer; the permutation check passed.

## 8. References

1. Wikipedia, "Jefferson disk". https://en.wikipedia.org/wiki/Jefferson_disk . Verified (fetched; summarized by a small model). Timeline, offset weakness, toy alphabets, and the bibliography in item 9.
2. Wikipedia, "M-94 (cipher machine)". https://en.wikipedia.org/wiki/M-94_(cipher_machine) . Verified. 25 aluminum disks, stamps B-1 to Z-25, disk 17, 25! value, dates.
3. Crypto Museum, "M-94". https://www.cryptomuseum.com/crypto/usa/m94/index.htm . Verified. Procedure, keyword-derived order, dates, replacement by the M-209.
4. prc68.com, "M-94 Cipher Wheels". https://www.prc68.com/I/M94.shtml . Verified. The 25 alphabets; provenance stated only as "Cryptologia M-94 issues".
5. prc68.com, "The M-94 Test Messages". https://www.prc68.com/I/M94TM.htm . Verified. 25 ciphertexts and alphabets; plaintext fragments only.
6. ciphermachines.com, "Thomas Jefferson - Cipher Machines". https://ciphermachines.com/ciphermachines/jefferson.html . Verified. Conceptual description; 36! = 3.72 x 10^41; no alphabets.
7. Monticello, "Wheel Cipher". https://www.monticello.org/encyclopedia/wheel-cipher and https://www.monticello.org/research-education/thomas-jefferson-encyclopedia/wheel-cipher/ . Unverified: HTTP 403 on fetch; search snippets only.
8. Library of Congress, "Thomas Jefferson, no date, Cipher Wheel, Notes and Copy". https://www.loc.gov/item/mtjbib025756 . Unverified: HTTP 403 on fetch; seen in search results only. Likely the primary source for the manuscript.
9. Kahn, D. (1967), The Codebreakers, pp. 192-195; Friedman, W. F. (1918), "Several Machine Ciphers and Methods for their Solution"; Kruh, L. (1981), "The Genesis of the Jefferson/Bazeries Cipher Device", Cryptologia 5(4):193-208; Gaddy, D. W. (1995), "The Cylinder-Cipher", Cryptologia 19(4):385-391. Unverified: cited as listed on the Wikipedia Jefferson disk page; none was opened.

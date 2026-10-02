*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# M-209 wiring data and test vectors

Research date: 2026-09-29. "Verified" means the source was opened in this session and the stated facts were read from it (fetch summaries were produced by a small model, so re-check numbers that matter to a contract). "Reproduced" means my own independent simulator (scratchpad only, not in the repo) produced the published value. Companion: `m209.md`. Research notes only; no MUST/SHOULD.

Numbering used below: letters A=0 ... Z=25 unless stated. "Wheel n" counts left to right as an operator faces the machine.

---

## 1. Key wheels

| Wheel | Letters (pins) | Alphabet (in wheel order) | Missing from A-Z | Sensed-pin letter when `A` is displayed | Sensed offset (letters ahead of display, in the wheel's own alphabet) |
|---|---|---|---|---|---|
| 1 | 26 | `ABCDEFGHIJKLMNOPQRSTUVWXYZ` | none | `P` | 15 |
| 2 | 25 | `ABCDEFGHIJKLMNOPQRSTUVXYZ` | W | `O` | 14 |
| 3 | 23 | `ABCDEFGHIJKLMNOPQRSTUVX` | W, Y, Z | `N` | 13 |
| 4 | 21 | `ABCDEFGHIJKLMNOPQRSTU` | V to Z | `M` | 12 |
| 5 | 19 | `ABCDEFGHIJKLMNOPQRS` | T to Z | `L` | 11 |
| 6 | 17 | `ABCDEFGHIJKLMNOPQ` | R to Z | `K` | 10 |

Source and status:

- Letter sets: Wikipedia "M-209", verified: "26 letters, from A to Z; 25 letters, from A to Z, excepting W; 23 letters, from A to X, excepting W; 21 letters, from A to U; 19 letters, from A to S; 17 letters, from A to Q". Note wheel 3 is A to X excluding W, which is 23 letters (A-V is 22, plus X).
- Sensed pin: Wikipedia, verified: "when 'AAAAAA' is showing on the key wheels, the pins that are in play are those associated with the letters 'PONMLK', from left to right."
- Both are independently confirmed by computation: the 26-letter checks below reproduce only with these alphabets and offsets (an incorrect offset or alphabet would change the output).
- Wheel 3's alphabet ends `...UVX` with no `W`: confirmed by Wikipedia text and by the m209 library data (verified).
- Position within a wheel means the index in the wheel's own alphabet. On wheel 2, `X` is position 22 (0-based), not 23; on wheel 3 likewise. The sensed pin is `(display index + offset) mod size` in that wheel's own alphabet: for wheel 2 with `V` displayed, the sensed index is (21+14) mod 25 = 10, letter `K`.
- The pin count per wheel equals the number of letters (131 total).
- Wheel advance: each wheel steps by one position in its own alphabet per letter, wrapping at the end of its alphabet (wheel 2 goes `V`, `X`, `Y`, `Z`, `A`). Direction inferred from data: the check vectors reproduce only with the displayed letter moving to the next letter, not the previous one. Physical direction of rotation was not read from a source.
- Period: 26 x 25 x 23 x 21 x 19 x 17 = 101,405,850, verified by computation and by Wikipedia.

## 2. Pin settings

A pin setting for wheel n is the set of letters (from wheel n's alphabet) whose pin is in the effective (right) position. Left = ineffective (Wikipedia, Blair, virtualcolossus, verified).

Set-of-letters encoding is the form used in every printed key list I saw and in the m209 library. Example pin sets (key list FM): wheel 1 `BCEJOPSTUVXY`, wheel 2 `ACDHJLMNOQRUYZ`, wheel 3 `AEHJLOQRUV`, wheel 4 `DFGILMNPQS`, wheel 5 `CEHIJLNPS`, wheel 6 `ACDFHIMN`. In the printed FM table these appear as columns 1 to 6, each row showing the letter when effective, a dash otherwise; wheel 2's `Y` and `Z` appear in rows 24 and 25 because the table rows are wheel positions, not alphabet positions (raw file read in full, verified).

## 3. Lug cage

- 27 bars (Wikipedia, Blair ch. 3, virtualcolossus, Barker text, verified); 2 lugs per bar, 54 lugs in total (Barker text, Blair ch. 3, verified).
- Each lug sits in one of 8 fixed slots: slots numbered 1 to 6 (opposite the guide arm of wheel 1 to 6) and 2 neutral slots numbered 0 (virtualcolossus, verified: "6 different active positions (indicated 1-6) or 2 inactive ones (indicated 0)").
- Effect: for each letter, wheel n is "active" if its sensed pin is effective. A bar is displaced if at least one of its lugs is in slot n and wheel n is active. K = number of displaced bars (0 to 27). A bar with a lug on wheel a and another on wheel b is displaced once even if both a and b are active (one bar, counted once).
- Written form: `a-b` with a, b in 0..6; `0` = neutral. The printed key lists write one or two lugs; examples: `1-0`, `2-0`, `0-3`, `0-4`, `3-4`, `4-5`, `1-5`, `1-6`. Shorthand `a-b*k` means k bars with that pair (m209 library docs, verified).
- Physical placement counting (28 ways per bar) vs functional counting (22 effects per bar) is in `m209.md` section 5.
- Blair ch. 3 (verified via fetch summary) says the two neutral slots are functionally equivalent "except when adjacent to positions 1-2 or 5-6". I did not find the precise physical constraint, which explains why printed pairs put the neutral lug on the left for wheels 1 to 3 and the right for 4 and up (`1-0` but `0-4`); output depends only on the multiset of wheel numbers per bar.
- Example lug lists (all four sum to 27 bars):
  - FM: `1-0 2-0*8 0-3*7 0-4*5 0-5*2 1-5 1-6 3-4 4-5` (1+8+7+5+2+1+1+1+1 = 27). Matches the FM.m209key table row by row (rows 01 to 27).
  - AA: `0-4 0-5*4 0-6*6 1-0*5 1-2 1-5*4 3-0*3 3-4 3-6 5-6` (1+4+6+5+1+4+3+1+1+1 = 27).
  - YL: `1-0 2-0*4 0-3 0-4*3 0-5*3 0-6*11 2-5 2-6 3-4 4-5` (1+4+1+3+3+11+1+1+1+1 = 27).
  - AB: `0-4*4 0-5*6 1-0*10 2-0*2 3-0 3-5*2 3-6 4-5` (4+6+10+2+1+2+1+1 = 27).

## 4. The output mapping

- Machine-level statement (Wikipedia, verified): the plaintext alphabet `ABCDEFGHIJKLMNOPQRSTUVWXYZ` maps to `ZYXWVUTSRQPONMLKJIHGFEDCBA` (atbash), shifted by K.
- Computed and reproduced against all four published checks:

      C = (K - 1 - P) mod 26        with A=0 ... Z=25
      P = (K - 1 - C) mod 26        (same operation, reciprocal)

  Equivalently `C = K - P` if plaintext letters are numbered A=1 ... Z=26 and ciphertext letters A=0 ... Z=25.
- K = 26 acts as K = 0 and K = 27 as K = 1 (mod 26).
- Sample of mapping for K = 0: A->Z, B->Y, ... Z->A. For K = 1: A->A, B->Z, C->Y, ... Z->B. (Direct from the formula.)

## 5. Letter counter, C/D lever, Z rule, grouping

- Letter counter: shows how many letters have been entered; set to `0000` before enciphering or deciphering, and reset to `0000` before each 26-letter check and each indicator step (Blair ch. 3 and 4, virtualcolossus, verified). It counts letter cycles; whether it advances on `Z` (a letter to the machine) is inferred from the m209 library, where it counts every keyed letter, including `Z` (source read; not confirmed against the physical machine). Its width beyond four digits and wrap behavior were not found.
- C/D lever: in C, the tape is printed in five-letter groups; in D, no grouping and `Z` prints as a space (virtualcolossus, Blair M-209-B page, verified). The key stream and wheel movement are identical in both positions.
- `Z` as space: the operator enters `Z` for each word space (Wikipedia, Blair, Barker text, m209 tutorial, verified). Barker's book text gives the example: `HELPZNEEDEDZONZHILLZSIXZONEZZEROZZERO` printing on the tape as `HELP NEEDED ON HILL SIX ONE ERO ERO` (verified via fetch summary; the printed-tape wording there follows the D-mode behavior). Plaintext `Z`s therefore disappear or become spaces (e.g. "ZERO" prints as " ERO"); the m209 library's own tutorial output shows `PIZZA` decrypting as `PI  A`.
- Grouping: five letters per group in C mode. In the m209 library the final short group is padded with `X` to five (ciphertext letters, which decrypt to arbitrary plaintext letters; I did not find the procedural source for this).
- Message layout (Blair ch. 4, verified): `[sys sys e1 e2 e3] [e4 e5 e6 k1 k2] [cipher groups] [sys sys e1 e2 e3] [e4 e5 e6 k1 k2]` with e1..e6 the external message indicator and k1 k2 the key list indicator.
- Internal indicator (Blair ch. 4, verified): set wheels to the external indicator, letter counter 0000, mode C; encipher the system letter 12 times; reset counter; set wheels 1 to 6 in order from the 12-letter tape, crossing out any tape letter not present on the wheel being set, taking the next letter instead.

## 6. Test vectors

All key data are published third-party data. None is a US Army primary source I opened; the provenance is stated for each. Each row says whether my simulator reproduced it.

### 6.1 Converter 26-letter checks (encipher `A` x 26 with all six wheels at `AAAAAA`, letter counter 0)

| Key list | Lugs | Pins wheel 1..6 (effective letters) | Published check | Source | Reproduced |
|---|---|---|---|---|---|
| FM | `1-0 2-0*8 0-3*7 0-4*5 0-5*2 1-5 1-6 3-4 4-5` | `BCEJOPSTUVXY`, `ACDHJLMNOQRUYZ`, `AEHJLOQRUV`, `DFGILMNPQS`, `CEHIJLNPS`, `ACDFHIMN` | `TNMYS CRMKK UHLKW LDQHM RQOLW R` | Blair key table FM (raw file fetched and read in full; pins and lugs read from the printed grid); also m209 unit tests | yes |
| AA | `0-4 0-5*4 0-6*6 1-0*5 1-2 1-5*4 3-0*3 3-4 3-6 5-6` | `FGIKOPRSUVWYZ`, `DFGKLMOTUY`, `ADEFGIORTUVX`, `ACFGHILMRSU`, `BCDEFJKLPS`, `EFGHIJLMNP` | `QLRRN TPTFU TRPTN MWQTV JLIJE J` | m209 library unit tests (data attributed there to Blair's AA key list; the Blair AA file itself was not opened) | yes |
| YL | `1-0 2-0*4 0-3 0-4*3 0-5*3 0-6*11 2-5 2-6 3-4 4-5` | `BFJKLOSTUWXZ`, `ABDJKLMORTUV`, `EHJKNPQRSX`, `ABCHIJLMPQR`, `BCDGJLNOPQS`, `AEFHIJP` | `OZGPK AFVAJ JYRZW LRJEG MOVLU M` | m209 library unit tests (attributed to Blair's YL list; not opened separately) | yes |
| AB | `0-4*4 0-5*6 1-0*10 2-0*2 3-0 3-5*2 3-6 4-5` | `BDFGIKRSTUWX`, `BCEJKLORSUX`, `CFHJKLMQSTU`, `ABCDHIJMOPRTU`, `BCEFINPS`, `ACDEHJN` | `GZWUU SFYQN NFAKK FXSEN FAFMF B` | m209 library documentation tutorial page (fetched) | yes |

The FM check was reproduced from a fresh reading of the printed key table grid (pins and lugs) as well as from the library's pin lists; those two agree letter for letter.

Because the check uses `A` plaintext and C = K - 1 - 0, the check letters give K directly. FM first 26 K values (start `AAAAAA`): 20, 14, 13, 25, 19, 3, 18, 13, 11, 11, 21, 8, 12, 11, 23, 12, 4, 17, 8, 13, 18, 17, 15, 12, 23, 18 (computed by my simulator; `T`=19=20-1 and so on match the check). The values are independent of the output-mapping convention.

### 6.2 Internal indicator example (Blair ch. 4)

Published: the 12-letter tape `FPKFW MHUPL CD` gives internal indicator `FPKFMH` (W skipped for wheel 5, which lacks it; then M for wheel 5, H for wheel 6). The chapter's worked example does not state its own key list in the text I read (fetch summary). My simulator, using FM, external indicator `ABCDEF` and system letter `G`, produces the tape `FPKFWMHUPLCD` and internal indicator `FPKFMH`, an exact match. The same key, external and system letter also appear in the m209 library's message test (6.3), so the agreement supports that Blair's example uses FM with `ABCDEF` and `G`, but the text I read does not say so: treat the pairing as inferred. Reproduced: yes (matches the published tape).

### 6.3 Full message vectors, key list FM

| Item | Value | Source | Reproduced |
|---|---|---|---|
| Plaintext (spaces to `Z`) | `ATTACK AT DAWN` (`ATTACKZATZDAWN`) | m209 unit test `test_procedure.py` | n/a |
| System letter, external indicator, key list indicator | `G`, `ABCDEF`, `FM` | same | n/a |
| Internal indicator | `FPKFMH` | derived; see 6.2 | yes |
| Ciphertext groups | `NQHNL CAARZ OLTVX` (14 letters + 1 `X` pad) | same test | yes |
| Full transmitted message | `GGABC DEFFM NQHNL CAARZ OLTVX GGABC DEFFM` | same test | yes |
| Also with 24 tape letters (m209 library behavior) | same output | derived | yes (identical because the first six usable tape letters lie within 12) |

Decryption example (m209 library test, attributed to Blair's page):

    Message: DDGPD UCOFM JSCPS XZTGR HHWJG BDKKK SHISC IMDFK RLUVH TWGAW SUYMM VZBQP OEBJE KPMBW GPGNI OFGAL VRYJC LSPLJ GRFYE UQVZT PSNDT OAPYG SKGKM CKQTD JCPBE NHYRX DDGPD UCOFM
    System letter D, external indicator GPDUCO, key list FM
    Tape (12 letters): PLIHKWZVIHJE ; wheels skipped for letters W, Z, V; internal indicator PLIHKI
    Plaintext, D mode with Z shown as space: MISSION ACCOMPLISHED X ALL ENEMY FORCES NEUTRALI ED X  ERO CASUALTIES X EIGHT PRISONERS TAKEN X AWAITING FURTHER ORDERSO

Reproduced: yes, character for character. This is a good conformance vector because it exercises tape skipping (W, Z, V), Z-to-space in D mode (real Z lost in "NEUTRALIZED" and "ZERO"), and the padding letter (`O` at the end).

### 6.4 Vector I computed but for which I have no published value

- Key AA, start positions `YGXREL`, plaintext `ATTACK AT DAWN` (`ATTACKZATZDAWN`), no indicator procedure: my simulator gives `SYCSR PJZGB DVPX`. The m209 library tests use exactly this start and text but assert only that variants agree, and print no value. Consider this an unverified computed vector until a second implementation is run on it.
- Reciprocity check (FM key, start `ABCDEF`): `THEQUICKBROWNFOX` enciphers to `SOMVIKLQUAUNNITQ` and deciphers back. Computed only.

### 6.5 What I could not find

- An authoritative full US Army key-setting example with real (not third-party) key list and message (TM 11-380 and TF 11-1400 were not opened).
- The Blair ch. 5 example content (page not fetched).
- Barker's book contains problem sets but no single end-to-end example key with ciphertext in the text I read (Internet Archive text, fetch summary).
- Independent published vectors for start-position handling beyond the checks (all wheels `A`) and the FM message.
- Physical-machine confirmation of the wheel-advance direction and of the padding rule; the checks constrain the first but do not distinguish physical turn directions.

## References

1. Wikipedia, "M-209". https://en.wikipedia.org/wiki/M-209 . Wikimedia Foundation. Verified (fetched 2026-09-29).
2. Mark J. Blair (NF6X), "Practical Use of the M-209 Cipher Machine", chapters 3 and 4. https://www.nf6x.net/2013/03/practical-use-of-the-m-209-cipher-machine-chapter-3/ ; https://www.nf6x.net/2013/04/practical-use-of-the-m-209-cipher-machine-chapter-4/ . Verified (fetched; via fetch summaries).
3. Mark J. Blair, key table FM. https://gitlab.com/NF6X_Crypto/m209-key-tables/raw/master/FM.m209key ; collection page https://www.nf6x.net/2013/03/a-collection-of-m-209-key-tables/ . Verified (fetched, raw file read in full).
4. Mark J. Blair, "Converter M-209-B". https://www.nf6x.net/2009/02/converter-m-209-b/ . Verified.
5. Brian Neal, "m209", GitHub (MIT). https://github.com/gremmie/m209 . Verified: repository cloned, data and tests read (this is where the AA, YL, FM lists and the message vectors come from). Docs: https://m209.readthedocs.io/en/latest/tutorial.html , verified (AB key list and check).
6. "Hagelin M-209 - Technical" tutorial. https://m209.virtualcolossus.co.uk/tutorial.html . Verified (fetched).
7. Wayne G. Barker, "Cryptanalysis of the Hagelin Cryptograph", Aegean Park Press, 1977. https://archive.org/details/hagelin . Verified (text fetched).
8. Crypto Museum, "Hagelin M-209". https://www.cryptomuseum.com/crypto/hagelin/m209/index.htm . Verified; gives pin counts only, no wheel letters.
9. US Army TM 11-380 and training film TF 11-1400. Not opened: unverified.

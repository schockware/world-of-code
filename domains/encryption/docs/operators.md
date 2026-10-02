*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# Operator procedures: preparing text

Research date: 2026-10-02. "Verified" means the page was opened in this session and the quoted text was read from it. Fetch summaries were produced by a small model, so exact spellings were fetched a second time where they matter. These are research notes and rationale, not requirements.

The theme running through this doc: **operator rules were shaped by the cipher's alphabet, the machine's mechanics, and the fight against cribs.** No machine had a space key or digits, so spaces and numbers had to become letters. Every convention for doing that leaves patterns, so later manuals added rules to break the patterns up.

---

## 1. Enigma, German Army and Air Force (Heer, Luftwaffe)

Source: Dirk Rijmenants, "Enigma Procedures", Cipher Machines and Cryptology. https://www.ciphermachinesandcryptology.com/en/enigmaproc.htm . Verified (fetched twice; the lines below are as the page gives them).

| Item | Rule on the page |
|---|---|
| Numbers | "Numbers were written out as NULL EINZ ZWO DREI VIER FUNF SEQS SIEBEN AQT NEUN" |
| Number abbreviations | "CENTA (00), MILLE (000) and MYRIA (0000)" |
| Full stop | "X = Full stop (end of sentence)" |
| Comma | "ZZ = Comma" |
| Point or dot | "YY = Point or dot" |
| Question mark | "usually abbreviated to FRAGE, FRAGEZ or FRAQ" |
| Parenthesis | "KLAM = Parenthesis" |
| Quotation marks | "X*****X = Inverted commas" (the quoted text sits between the Xs) |
| Names | "Foreign names, places, etc. are delimited twice by "X", as in XPARISXPARISX or XFEUERSTEINX." |
| CH | "The letters CH were written as Q. ACHT became AQT, RICHTUNG became RIQTUNG." |
| Grouping | Messages were "transmitted ... always in five-letter group[s]". |
| Length | "forbidden to use more than 250 characters in a single message". "Longer messages were divided into several parts, each part using its own message key." |
| Umlauts, CK, word spaces | Not on the page. |

Notes:
- **EINZ or EINS.** The page says EINZ, but the 1941 Barbarossa message (`enigma.md`, vector V3) has `EINSAQTDREINULL` (1830), with EINS. Real traffic and the procedure summary disagree, or practice varied. ZWO for ZWEI is the well-known spoken form that avoids confusion with DREI.
- **FUNF.** The page writes FUNF, with no umlaut and no UE. No traffic was checked for FUENF.
- **SEQS** is SECHS with the CH-to-Q rule applied. **AQT** is ACHT the same way.
- **Word spaces.** The page gives X only as a full stop. The Barbarossa message also uses X between words and phrases (`AUFKLXABTEILUNGXVONXKURTINOWA`), so X served as both separator and full stop in practice.
- **Why these rules exist.** No digit or punctuation keys, so everything became letters. CH to Q shortens a very common German pair. Doubling names (XPARISXPARISX) guards against garbles in names, which cannot be guessed from context. The 250-letter limit and per-part message keys limit how much text shares one key: the cipher's statistical weaknesses grow with length.

## 2. Enigma, Kriegsmarine

Same source, verified. Formatted "in four-letter groups". X period, Y comma, UD question mark, XX colon, YY dash, hyphen or slant, KK...KK parenthesis, J...J stress mark. Spaces, numbers, CH and umlauts are not on the page. Out of scope while `enigma-m3` means the Army machine, and recorded so the M4 work can start from it.

## 3. M-209, US Army

Sources:
- J.-F. Bouchaudy, "M-209: Security measures", quoting TM 11-380 (1942, 1944, 1947). http://www.jfbouch.fr/crypto/m209/security.html . Verified (fetched over HTTP; the HTTPS certificate does not match the host).
- J.-F. Bouchaudy, "M-209: Manual". http://www.jfbouch.fr/crypto/m209/manual.html . Verified. Lists the editions: 27 April 1942 (33 pages), 20 September 1943 (42), 17 March 1944 (78), May 1947 (170, restricted).
- Wikipedia "M-209", Blair, and the `m209` library, as already cited in `m209.md`: Z keyed for a space, numbers spelled out, phonetic words for single letters.

| Item | Rule |
|---|---|
| Word spaces | Z between words. "Converter M-209-(*) was originally designed to encipher one Z between each word so that the deciphered text would appear on the tape in word lengths." (TM 11-380, 1947) |
| Spacing variation | "As a security measure, the following variations of this spacing will be used for every message. Between some words, omit the Z; between other words, encipher two Z's; between remaining words space normally (one Z)." (1947) |
| Caution | "NEVER USE MORE THAN TWO Z'S BETWEEN WORDS. NEVER USE A DISPROPORTIONATE NUMBER OF ANY ONE OF THE VARIATIONS. NEVER CHOOSE CHARACTERISTIC POINTS FOR PLACING ANY ONE OF THE VARIATIONS. (For example, do not consistently place double Z's before and after an internal address or signature.)" (1947) |
| Length | "Messages exceeding 100 groups in length must be divided into two or more approximately equal parts so that no parts exceed 100 groups [500 letters]." (1947) |
| Key volume | One key "will not normally exceed 10,000 groups [50,000 letters]", to limit overlaps (messages in depth). (1947) |
| Re-enciphering | A transmitted message is never re-enciphered unless "the entire message is paraphrased", with a new indicator. (1947) |
| Numbers, single letters | Spelled out, single letters as phonetic words (Blair's M-209-B page and the virtualcolossus tutorial, as cited in `m209.md`). The exact words were not found in the pages read. |
| Punctuation | Not found in the pages read. The 1942 and 1944 manuals are online (Crypto Museum and iLord, linked from the manual page) and were not opened. |
| Padding | The `m209` library pads the last group with X. No manual text read on this. |

Notes:
- **Why the spacing variation exists.** One Z per space makes word lengths visible after a break-in and makes Z the most frequent plaintext letter, a gift to statistical attacks. TICOM interrogations after the war found that Germans had read more than 10% of M-209 traffic, and the 1947 manual's security rules followed (Bouchaudy's summary).
- **Why it matters for this project.** The variation is deliberately random, and the operator chooses per message. For a reproducible spec, the choice has to be an input, the same way the Jefferson disk row and the book cipher locator will be.
- **Edition matters.** The variation rule is quoted from the 1947 edition. Whether 1942-1944 operators were required to vary the spacing was not confirmed. A WWII-faithful profile may use one Z throughout.

## 4. Classical ciphers (Caesar, Affine, autokey, skytale)

No operating procedures survive for these as military systems in the sources read. Their text preparation is a project choice, not a historical one: the `x` and `spelled` separators already in `message.json`.

## 5. Ambiguities an operator spec must pin down

1. **Judgment vs. mechanism.** Deciding which words are names, which punctuation is "important", and what to abbreviate is operator judgment. Mapping a number, a punctuation mark or a name to letters is mechanical. A spec can require only the mechanical part.
2. **Random choices.** M-209 spacing variation (1947) is random by design. It must be an input to stay deterministic.
3. **Variant spellings.** EINZ or EINS, FUNF or FUENF, FRAGE or FRAGEZ or FRAQ.
4. **Overloaded letters.** Enigma's X is both full stop and word separator. M-209's Z is both space and a real letter. Decoding needs human judgment in both.
5. **Umlauts and ß.** Not in the Enigma source read. The usual German transliteration is AE, OE, UE, SS, but FUNF suggests umlauts were sometimes just dropped.
6. **Edition and era.** Rules changed: Enigma before and after 1940, M-209 from 1942 to 1947.

## References

1. Rijmenants, D., "Enigma Procedures", Cipher Machines and Cryptology. https://www.ciphermachinesandcryptology.com/en/enigmaproc.htm . Verified (fetched 2026-10-02).
2. Bouchaudy, J.-F., "M-209: Security measures". http://www.jfbouch.fr/crypto/m209/security.html . Verified (fetched over HTTP 2026-10-02). Quotes TM 11-380 (1942, 1944, 1947).
3. Bouchaudy, J.-F., "M-209: Manual". http://www.jfbouch.fr/crypto/m209/manual.html . Verified (fetched over HTTP 2026-10-02).
4. War Department, TM 11-380, Converter M-209 (1942, 1943, 1944, 1947). Not opened: unverified. Quoted only through reference 2.
5. Enigma Barbarossa message, `enigma.md` section 10, V3: evidence for EINS and for X as a word separator.

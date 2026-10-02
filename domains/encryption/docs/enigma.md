*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Enigma: Research Notes

Research date: 2026-09-29. "Verified" means the URL was opened (fetched, or returned by WebSearch) in this session and the stated facts were read from it. Fetch summaries were produced by a small model, so anything that matters to a contract was cross-checked by computation with a simulator written for this research (kept in the scratchpad, not in the repo). Where a claim comes from my background knowledge and no opened source, it is marked "unconfirmed here". Priority = relevance to a technology-agnostic Enigma spec and shared conformance vectors. This is research and rationale, not a spec, so it has no MUST/SHOULD.

Primary target: the Army/Air Force Enigma I, in the three-rotor form (called M3 by the task; see 2.1 for the naming wrinkle). Extension: the Navy M3 (rotors VI-VIII) and the four-rotor M4. Wiring tables are in `enigma-wiring-data.md`.

---

## 1. History (short)

- Arthur Scherbius applied for a patent on 23 February 1918 and, with E. Richard Ritter, founded Scherbius & Ritter; the successor company Chiffriermaschinen AG marketed the commercial Enigma from 1923 (Wikipedia "Enigma machine", verified).
- Reichsmarine adopted a version in 1926 (Funkschluessel C); the Army's Enigma G by July 1928; the Wehrmacht Enigma I by June 1930; the Navy M3 in 1934; the four-rotor M4 for U-boats from 1 February 1942 (same source, verified).
- Polish mathematician Marian Rejewski first broke it around December 1932 using permutation theory and the doubled-indicator procedure; on 26-27 July 1939 the Poles shared their methods and reconstructed machines with the French and British (same source, verified).
- The doubled message-key indicator was dropped just before the offensive of 10 May 1940, commonly dated 1 May 1940 (the task and Wikipedia "Cryptanalysis of the Enigma" agree on the change and on "just before 10 May 1940"; the exact 1 May date is not stated in the text returned by the fetch, so it is unconfirmed here).

## 2. Variants and naming

### 2.1 Naming wrinkle: what "M3" means

- The Army and Air Force machine is properly Enigma I with three rotors selected from five (I-V).
- "M3" is the Navy's designation (adopted 1934 per Wikipedia), which added rotors VI, VII, VIII to the pool (eight in total), and which reads as the three-rotor naval machine.
- Casual usage (and our README) uses "M3" for a three-rotor Enigma. The 60-orders figure in the README (5P3) is the Army/Air Force pool of five. A naval M3 pool of eight gives 8P3 = 336 orders (computed).

Decision for the user: whether our spec names the primary target "enigma-i" (5 rotors, single notch) with "m3-naval" and "m4" as configurations, or treats "M3" as a superset. Recorded as an ambiguity in section 9.

### 2.2 Variants table

| Variant | Rotor pool | Moving rotors | Reflector | Ring setting | Notes |
|---|---|---|---|---|---|
| Commercial D/K | 3 | 3 | rotatable | yes | keyboard-order ETW; out of scope |
| Enigma I (Army, Air Force) | I-V | 3 | B, C (A early) | yes | primary target |
| Naval M3 | I-VIII | 3 | B, C | yes | VI-VIII have two notches |
| M4 (U-boat) | I-VIII plus beta, gamma | 3 moving plus 1 fixed Greek wheel | thin B, thin C | yes | Greek wheel never steps |
| Abwehr G312 | 3, irregular stepping | 3 | rotatable | yes | not modeled; see wiring doc |

Sources: wiring and variants from Crypto Museum wiring page and Enigma G page (verified); M4 description from Crypto Museum message page (verified) and the search summary of the M4 page.

## 3. Mechanism

Signal path for one keypress on the M3/Enigma I (verified against Crypto Museum "working" page for the general description; the specifics below are what my simulator implements and reproduced published messages with):

1. Stepping happens first, then the letter is enciphered. The rotors are stepped before the current flows.
2. The key letter goes through the plugboard (Steckerbrett), then the entry wheel (ETW; identity on the military machine), then the right rotor, middle rotor, left rotor, then the reflector (UKW), back through left, middle, right rotors on inverse wiring, back through the ETW, through the plugboard again, and lights a lamp. (On M4, the Greek wheel is a fourth rotor between the left rotor and the thin reflector.)
3. Each rotor is a fixed substitution whose effective mapping is rotated by its offset. With the offset d = (position - ring setting) mod 26, the signal entering at contact x leaves at (wiring[(x + d) mod 26] - d) mod 26, where letters are numbered A=0 ... Z=25. The return trip uses the inverse wiring with the same d.
4. The plugboard swaps up to 10 pairs of letters, applied on entry and again on exit (a self-inverse substitution).

## 4. Key structure

| Element | German | What it is | Values (M3 Enigma I) | Changes |
|---|---|---|---|---|
| Rotor order | Walzenlage | which rotors, left to right | ordered choice of 3 from 5 = 60 | daily |
| Ring setting | Ringstellung | rotation of the alphabet ring (and its notch) relative to the wiring core | one letter (or number 1-26) per rotor | daily |
| Start position | Grundstellung | letters shown in the windows when the message-key encryption begins | 3 letters | daily (pre-1940 basic setting); chosen by operator per message afterward |
| Message key | Spruchschluessel | 3 letters chosen by the operator; the window start for the message text | 3 letters | per message |
| Plugboard | Steckerbrett / Stecker | letter pair swaps | up to 10 pairs, 20 letters used | daily |
| Key identifier | Kenngruppen | groups used to identify which key sheet applies | sent in clear | per key sheet |

Sources: Cipher Machines and Cryptology "Enigma Procedures" (verified) for the key sheet elements Walzenlage, Ringstellung, Steckerverbindungen, Kenngruppen and the Grundstellung / message key description.

Distinctions that are easy to conflate:
- Ring setting moves the wiring core relative to the letter ring and notch. It changes the mapping (via d above) but does not change which window letter triggers the turnover.
- Start position is what the windows show. It changes both mapping and, over time, when the turnover happens.
- The leftmost rotor's ring setting has no effect on the output for a three-rotor machine, because the left rotor never triggers another rotor and the window letters, not the wiring core, are what the operator sets. (Its effect is only a shift in what the window letter means; the effective state depends on position minus ring. Consequently the ring on the left rotor is redundant with its start position. This follows from the offset formula in section 3; I did not find a source stating it.)

## 5. Stepping

### 5.1 Single and double notches

- Rotors I-V have one turnover notch each. Turnover letters (window letter at which the next keypress also steps the rotor to the left): I Q, II E, III V, IV J, V Z. On the ring, the corresponding notch letters are Y, M, D, R, H (turnover + 8 mod 26, checked for all).
- Rotors VI, VII, VIII have two notches, at window letters Z and M, so they trigger the neighbor twice per revolution (roughly every 13 steps). Notch letters on the ring are H and U.
- Beta and gamma (M4) have no notch and never step. The fourth wheel never moves during encipherment.

### 5.2 Rule, precisely

Let R, M, L be the right, middle, left rotors; "at notch" means the window letter is one of that rotor's turnover letters, evaluated on the positions before this keypress. On each keypress, before enciphering:

- R always advances by one.
- M advances by one if R is at its notch, or if M is at its own notch.
- L advances by one if M is at its notch.

### 5.3 The double-stepping anomaly

Because M advances when R is at its notch and also when M itself is at its notch, a middle rotor that has just been moved onto its notch by R turns again on the very next keypress, carrying L with it. So M moves on two consecutive keypresses (the "double step"), which is what makes the middle rotor's turnover come one keypress early relative to the naive odometer. Exactly, with rotors I (left), II (middle), III (right), turnovers E (middle), V (right):

Window positions after each keypress, starting from ADU: ADV, AEW, BFX, BFY.

- Press 1: R at U, not at notch. Only R moves: ADV.
- Press 2: R at V is at its notch, M at D is not: R moves, M moves (D to E): AEW.
- Press 3: R at W is not at notch, but M at E is at its notch: M moves again (E to F), L moves (A to B), R moves: BFX.
- Press 4: no notch engaged: BFY.

This sequence is published in Wikipedia "Enigma rotor details" (verified: ADV, AEW, BFX) and reproduced by the simulator. Consequences: the middle rotor steps on two consecutive keypresses once per cycle, and the cycle length of the three-rotor stepping is 26 x 25 x 26 = 16,900, not 26^3 = 17,576. I confirmed the period 16,900 by counting steps until the state repeated and counting 16,900 distinct states. The Crypto Museum fetch summary quoted "16,926" for the reduced number; I did not reproduce that value and treat it as a likely summary error (unresolved; needs a look at the primary page text).

Ring setting does not change this sequence: with ring BCD and the same start ADU, the position sequence is identical (simulated).

### 5.4 Whether the pawl engages on the position shown in the window

In the model above, the decision uses the window letters before the step, and the notch is engaged when the window letter equals the turnover letter, so "the pawl engages on the position shown in the window" means: when the window shows Q on rotor I and you press a key, the neighbor to its left steps as the rotor moves Q to R. Ring setting is irrelevant to this decision because the notch is on the ring, which moves with the window letters. This is the model that reproduced the published Barbarossa and U-534 M4 messages, so it is consistent with those.

## 6. Cryptographic properties: reflector, reciprocity, no self-encryption

- Let P_t be the full substitution at step t: P_t = S R_t^-1 ... U ... R_t S, where S is the plugboard, R the rotor stack, and U the reflector. Because U is an involution (U = U^-1) and S is an involution, P_t is a conjugate of U by a permutation: P_t = T^-1 U T with T = R_t S. A conjugate of an involution is an involution, so P_t = P_t^-1.
- Reciprocity: since each P_t is an involution, encrypting the ciphertext with the same settings returns the plaintext, provided the rotor stepping is identical (same start state, and stepping depends only on the count of keypresses, not on the letters). Checked by simulation: 300 random settings, 200 letters each, all round-trip.
- No fixed points: U has no fixed points (a reflector wiring pairs 13 pairs of distinct letters), and conjugation preserves the absence of fixed points. So P_t(x) never equals x: a letter never encrypts to itself. Checked: 60,000 letters over 300 random settings, zero self-encryptions.
- Consequences for attackers: cribs can be slid against the ciphertext and any alignment with a self-match is impossible (Wikipedia "Bombe", verified: "The reflector in the scrambler prevented a letter from being enciphered as itself"). Consequences for a spec: the ciphertext is not uniformly distributed over the non-plaintext letters in an obviously different way, and an implementation that ever emits the input letter is wrong.
- A side effect of the reflector: a letter and its cipher are symmetric (if A maps to B at step t then B maps to A at step t), which the Bombe's diagonal board exploited.

## 7. Key-count arithmetic (and correction to the README)

All numbers below were computed by script.

| Quantity | Value | Computation |
|---|---|---|
| Rotor orders, 3 of 5 | 60 | 5P3 = 5 x 4 x 3 |
| Rotor orders, 3 of 8 (naval) | 336 | 8P3 |
| Start positions | 17,576 | 26^3 |
| Ring settings | 676 | 26^2 (the left ring is redundant, so 26^2 not 26^3, as in the README) |
| Rotors x positions x rings | 712,882,560 | 60 x 17,576 x 676 (README: "about 7 x 10^8": correct) |
| Plugboard, exactly 10 pairs | 150,738,274,937,250 | 26! / (6! x 10! x 2^10) (README: "about 1.5 x 10^14": correct) |
| Rotors x positions x plugboard (no ring) | 158,962,555,217,826,360,000 | 60 x 17,576 x 150,738,274,937,250 = 1.5896 x 10^20 |
| Rotors x positions x rings x plugboard | 107,458,687,327,250,619,360,000 | 60 x 17,576 x 676 x 150,738,274,937,250 = 1.0746 x 10^23 |

Correction: the README says the total including the ring settings, "60 rotor orders x 17,576 rotor positions x 676 ring settings ... 10-cable plugboard ... roughly 1.6 x 10^20". That product is about 1.07 x 10^23, not 1.6 x 10^20. The 1.6 x 10^20 figure (158,962,555,217,826,360,000) is the widely quoted one (Wikipedia "Enigma machine", verified) and equals rotor orders x start positions x plugboard, leaving the ring settings out. The README's sub-figures (7 x 10^8, 1.5 x 10^14, and the prefix of 60 = 5P3) are correct. Its 1.6 x 10^20 total is correct only if the ring settings are excluded; if they are included, 1.07 x 10^23. I did not edit the README (out of scope); it needs a one-line fix.

Other plugboard counts (computed): 0 cables 1; 1 cable 325; 11 cables 205,552,193,096,250; 12 cables 102,776,096,548,125; 13 cables 7,905,853,580,625. The count peaks at 11 cables, not 10, which is why the number of cables is a genuine spec parameter.

Effective keyspace notes: only positions of the sequence matter for stepping (16,900 states, section 5.3), and the left ring and the left window position are partly redundant, so the true number of distinct behaviors is smaller than the raw product. I did not compute the exact number of distinct machines.

## 8. Procedure and cryptanalysis

### 8.1 Daily key sheet, message key, and indicator

- The key sheet (monthly distribution) listed Walzenlage, Ringstellung, Steckerverbindungen and Kenngruppen; before mid-1940 the daily Grundstellung was also fixed by the sheet (Cipher Machines and Cryptology, verified).
- Before 1940 (the Poles' era): the operator set the daily Grundstellung, chose a random three-letter message key, enciphered it twice in a row (six letters) at the Grundstellung, sent those six letters, then set the windows to the message key and enciphered the text. Cipher Machines and Cryptology (verified): "The message key is encrypted twice, resulting in a relation between first and fourth, second and fifth, and third and sixth character."
- From 1940 (Wehrmacht): the operator chose his own random Grundstellung, set it, encrypted the message key once, sent the Grundstellung in clear along with the encrypted key, then enciphered the text from the message key. A five-letter group (two random letters plus a three-letter Kenngruppe from the sheet) identified the key and was not enciphered (same source, verified).
- Kriegsmarine (M4): the indicator went through bigram tables before being enciphered. In the U-534 message P1030681, the indicator groups DUHF TETO, put through tables and the basic setting, produced the message key CDSZ (Crypto Museum, verified, via fetch summary; the summary was internally inconsistent on the basic setting, NAEM versus NEAM, so the indicator path is not reproduced by me).

### 8.2 Rejewski's cycle attack (doubled indicators)

Precise statement (Wikipedia "Cryptanalysis of the Enigma", verified in outline): with a doubled indicator, the six enciphered letters are the same three plaintext letters enciphered at steps 1-3 and 4-6. Let A, B, C, D, E, F be the (plugboard-and-rotor) permutations at steps 1..6. Then letters 1 and 4 are related by the product AD (as A followed by D, using that each is an involution), letters 2 and 5 by BE, letters 3 and 6 by CF. Collecting a day's indicators lets one write these three permutations in cycle notation. The cycle lengths of a permutation (its "characteristic") are unchanged when the permutation is conjugated by the plugboard, so they depend only on rotors, ring settings, and start position and not on the plugboard. Rejewski used this to solve for the wiring (with a few pieces of additional material, including key-sheet information obtained from a spy) and then, from a catalog of characteristics (the "card catalog", using the cyclometer, and later the bomba), to recover the daily rotor order and start positions without the plugboard. The plugboard was then recovered from the rest. Rejewski's first break: around December 1932 (Wikipedia, verified).

### 8.3 Zygalski sheets

Perforated sheets exploiting "females": indicator pairs in which the same letter appears in a doubled position (a fixed point of a cycle, meaning a letter enciphered identically both times), independent of the plugboard. Per the fetched summary: sets of 26 sheets for each of the six possible wheel orders (three rotors at the time), covering the positions of the middle and right rotors, about one message in eight yields a female. They stopped being useful when the doubling was dropped in May 1940.

### 8.4 The Turing-Welchman Bombe and cribs

- A crib is a suspected plaintext fragment at a known offset in the ciphertext. Because a letter never encrypts to itself, impossible crib alignments can be discarded.
- From crib and ciphertext one builds a menu, a graph of letter relationships; loops in the graph give constraints that hold independent of the plugboard. More loops mean fewer false stops (Turing's analysis, per Wikipedia "Bombe", verified: a menu of 8 letters and 3 loops gave about 2.2 stops per rotor order, versus 40,000 with no loops).
- The Bombe tries each rotor order and each starting position (the Wikipedia summary says 26^3 = 17,576 positions per order) against the menu, and tests by reductio ad absurdum whether the assumed plugboard value for some letter leads to a contradiction; positions that do not contradict are "stops" for manual checking. The plugboard is thus treated as an unknown to be deduced, not enumerated, which is the reason the 10^14 plugboard factor does not cost anything in the search.
- Welchman's diagonal board (early 1940) used the reciprocity of the plugboard (if A is plugged to B then B is plugged to A) to sharply cut down false stops. The first Bombe, "Victory", was delivered to Bletchley Park on 18 March 1940; the second, "Agnes", with the diagonal board, by August 1940 (Wikipedia, verified).

### 8.5 Banburismus

Turing's statistical method (naval) that used overlapped ciphertexts on perforated sheets to reduce which rotor orders had to be tried on Bombes, saving Bombe time (Wikipedia "Cryptanalysis of the Enigma", verified in outline: "which of the many possible wheel orders could be omitted"). I did not read the details of the scoring; the exact statistic and the assertion about repeats aligned in depth are unconfirmed here.

### 8.6 Operator errors after May 1940

- After the indicator change, cryptanalysts "rely on exploiting the operator weaknesses" (Wikipedia, verified).
- Cillies: operators picked easy message keys such as AAA, BBB, or a keyboard sequence such as QWE; Bletchley Park called these cillies (same source, verified).
- The Herivel tip: John Herivel (arrived January 1940) reasoned that after setting the rings and closing the lid, the operator might not turn the rotors more than a few places when choosing the first message key, so first-message keys on a given day would cluster near the ring setting; this is a way to guess the day's ring-related state. Most useful after May 1940 (same source, verified).
- The "sloppy operator" of the README is the accumulated set of these habits: repeated or predictable message keys, and reused indicators.

## 9. Ambiguities a spec must pin down

Each is a decision; the convention in the last column is what my simulator used and reproduced all three published vectors with, so it is safe as a default, but the choice belongs to the user.

| # | Ambiguity | Options | Simulator convention |
|---|---|---|---|
| 1 | Rotor position letter convention | window letter (A-Z), or number 0-25 or 1-26 | letter shown in the window, A=0 |
| 2 | Ring setting offset direction | offset d = position - ring, or position + ring | d = (position - ring) mod 26; ring letter A = no offset |
| 3 | Ring setting notation | letters, or numbers 01-26 (A=01), or 0-25 | letters; numeric form is a UI mapping |
| 4 | When the turnover is tested | before the step (window letter equals turnover letter now), or after | before, on the pre-step window letters, with stepping done before enciphering |
| 5 | Turnover letter versus notch letter | which the spec stores | turnover letter (window letter); notch letter = turnover + 8 |
| 6 | Double stepping | required, or optional flag (some simulations omit it) | implemented per 5.2 |
| 7 | Rotor and reflector naming | I-VIII, beta/gamma, A/B/C, B-thin/C-thin; which ETW | as in the wiring doc; ETW identity |
| 8 | Left-to-right order | list order of rotors: left to right (Walzenlage as written) or right to left | left to right; the last listed is the fast rotor |
| 9 | M4 fourth wheel position | ring and start for the Greek wheel | 4 letters, first is Greek |
| 10 | Plugboard validity | max pairs, letter twice, self-pair | at most 10 pairs; a letter may appear at most once; a pair of a letter with itself is invalid (the simulator asserts this). Whether fewer than 10 is valid (historically yes, and the count peaks at 11) is a spec choice |
| 11 | Input text handling | strip non-letters, reject them, or pass through; case; X for space, umlauts (AE OE UE), digits spelled out | the simulator filters to A-Z; the spec must say whether it rejects, strips, or preserves; historical practice used X as a separator and spelled digits |
| 12 | Output grouping | groups of five, or continuous | continuous in the vectors; grouping is presentation |
| 13 | Keypress vs. character counting | stepping on non-letter input | steps only on letters |
| 14 | Message key procedure | model only the machine, or also the indicator / bigram tables | machine only; the indicator procedure is a separate layer |
| 15 | "M3" naming | Enigma I vs naval M3 vs generic three-rotor | see 2.1 |

## 10. Test vectors

Each vector: rotors listed left to right, reflector, ring setting (left to right), start position (left to right), plugboard. "Reproduced" = my simulator's output equals the published value (independent of the source, since I computed it separately). "Simulator only" = computed by my simulator, no published value found, so it is a consequence of my convention and needs a second independent implementation before it is treated as truth.

| ID | Machine and settings | Input | Output | Status |
|---|---|---|---|---|
| V1 | Enigma I, rotors I II III, reflector B, ring AAA, start AAA, no plugboard | AAAAA | BDZGO | Reproduced. Published in Wikipedia "Enigma rotor details" (verified). |
| V2 | as V1 but ring BBB | AAAAA | EWTYX | Reproduced. Same source (verified). The page said "ring settings in B-position"; I read that as BBB and it matched. |
| V3 | Enigma I, rotors II IV V, reflector B, ring BUL, start BLA, plugboard AV BS CG DL FU HZ IN KM OW RX | EDPUD NRGYS ZRCXN UYTPO MRMBO FKTBZ REZKM LXLVE FGUEY SIOZV EQMIK UBPMM YLKLT TDEIS MDICA GYKUA CTCDO MOHWX MUUIA UBSTS LRNBZ SZWNR FXWFY SSXJZ VIJHI DISHP RKLKA YUPAD TXQSP INQMA TLPIF SVKDA SCTAC DPBOP VHJK | AUFKLXABTEILUNGXVONXKURTINOWAXKURTINOWAXNORDWESTLXSEBEZXSEBEZXUAFFLIEGERSTRASZERIQTUNGXDUBROWKIXDUBROWKIXOPOTSCHKAXOPOTSCHKAXUMXEINSAQTDREINULLXUHRANGETRETENXANGRIFFXINFXRGTX | Reproduced (decrypts to coherent German). Barbarossa message, published by Geoff Sullivan and Frode Weierud. Ciphertext, rotors, rings, and plugboard read from a WebSearch result excerpt of the Franklin Heath wiki page (my WebFetch of that page failed twice, so it is verified via search excerpt only). The start position BLA was not in the excerpt; I supplied it from recollection and it produced clean German, which is strong evidence but not a citation. The excerpt's Kenngruppen RFUGZ and FNJAU are skipped; the ciphertext above begins after them. I did not read a published plaintext to compare character by character. |
| V4 | M4: reflector thin C, wheels beta V VI VIII (left to right, "568" with beta), ring EPEL, start CDSZ, plugboard AE BF CM DQ HU JN LX PR SZ VW | the U-534 message P1030681 body: LANO TCTO UARB BFPM HPHG CZXT DYGA HGUF XGEW KBLK GJWL QXXT GPJJ AVTO CKZF SLPP QIHZ FXOE BWII EKFZ LCLO AQJU LJOY HSSM BBGW HZAN VOII PYRB RTDJ QDJJ OQKC XWDN BBTY VXLY TAPG VEAT XSON PNYN QFUD BBHH VWEP YEYD OHNL XKZD NWRH DUWU JUMW WVII WZXI VIUQ DRHY MNCY EFUA PNHO TKHK GDNP SAKN UAGH JZSM JBMH VTRE QEDG XHLZ WIFU SKDQ VELN MIMI THBH DBWV HDFY HJOQ IHOR TDJD BWXE MEAY XGYQ XOHF DMYU XXNO JAZR SGHP LWML RECW WUTL RTTV LBHY OORG LGOW UXNX HMHY FAAC QEKT HSJW | KRKRALLEXXFOLGENDESISTSOFORTBEKANNTZUGEBENXXICHHABEFOLGELNBEBEFEHLERHALTENXXJANSTERLEDESBISHERIGXNREICHSMARSCHALLSJGOERINGJSETZTDERFUEHRERSIEYHVRRGRZSSADMIRALYALSSEINENNACHFOLGEREINXSCHRIFTLSCHEVOLLMACHTUNTERWEGSXABSOFORTSOLLENSIESAEMTLICHEMASSNAHMENVERFUEGENYDIESICHAUSDERGEGENWAERTIGENLAGEERGEBENXGEZXREICHSLEITEIKKTULPEKKJBORMANNJXXOBXDXMMMDURNHFKSTXKOMXADMXUUUBOOIEXKP | Reproduced. The simulator output matches the plaintext printed on the Crypto Museum page (verified via fetch; the page text was returned by a small model, and I compared the plaintext as returned, including the odd letter groups, and they match exactly). The indicators DUHF TETO at start and end are not part of the body. Settings from the same page: reflector C, Greek wheel Beta, rotor order 568, ring EPEL, plugs as above, message key CDSZ. The page's rotors "568" I read as beta then rotors V, VI, VIII left to right. |
| V5 | Enigma I, rotors I II III, reflector B, ring AAA, start AAA | 26 x A | BDZGO WCXLT KSBTM CDLPB MUQOF X | Simulator only (first five letters match V1) |
| V6 | same as V5 | HELLOWORLD | ILBDAAMTAZ | Simulator only |
| V7 | rotors I II III, reflector B, ring AAA, start ADU | 10 x A | EQIBM GFJBW; window positions after each press: ADV AEW BFX BFY (first four presses) | Positions are from the published double-step sequence (ADV, AEW, BFX per Wikipedia, verified). Ciphertext is simulator only. Demonstrates double-stepping: the middle rotor moves on the 2nd and 3rd presses. |
| V8 | as V7 | HELLOWORLD | IBXXXNVDFL | Simulator only |
| V9 | as V7 but ring BCD | HELLOWORLD | ZITVAJTJFA | Simulator only. Window position sequence identical to V7, showing ring does not shift notches. |
| V10 | rotors I II VI, reflector B, ring AAA, start AAL | 5 x A | NSRID | Simulator only. Demonstrates the two-notch rotor VI: right rotor at M steps the middle rotor; window positions after each press: AAM ABN ABO ABP. |
| V11 | rotors I II VI, reflector B, ring AAA, start AAY | 6 x A | HEQPI U | Simulator only. Window positions: AAZ ABA ABB ABC (the Z notch, the other one). |
| V12 | rotors I II III, reflector B, ring AAA, start AAA, plugboard AB CD EF | 10 x A | BJLDS YJIEK | Simulator only. |
| V13 | naval M3: rotors IV V VII, reflector C, ring CDE, start XYZ, plugboard AB CD EF GH IJ KL MN OP QR ST | THEQUICKBROWNFOXJUMPSOVERTHELAZYDOG | XPAVJ QKFHO YJLMP NYZUL ZCDLV BZLWY GRHSO | Simulator only. Uses reflector C with rotor VII, which no published vector I read exercises. |

Not reproduced: none of the vectors I attempted failed. I did not attempt the unbroken U-534 message P1030680 (72 letters) or any pre-1940 doubled-indicator example with published bytes; those remain to be sourced. I also did not obtain a published vector for the first-message-key Herivel scenario, Abwehr G312, or the commercial machines.

Simulator: a small script in the session scratchpad (not in the repository). It implements the model in sections 3 and 5 and asserts plugboard validity.

## 11. Open items for the user or a follow-up

1. Exact date of the indicator change (1 May 1940 versus "just before 10 May 1940").
2. Crypto Museum's "16,926" figure versus my computed 16,900 for the stepping period (primary page not read closely).
3. Commercial machine notch/turnover data (inconsistent in the fetch).
4. A second source for V4's full plaintext and settings (only Crypto Museum was read), and for V3's start position BLA.
5. Whether the spec includes the indicator procedure and bigram tables (section 9, item 14).

## References

| # | Title | Publisher | URL | Status |
|---|---|---|---|---|
| 1 | Enigma machine | Wikipedia | https://en.wikipedia.org/wiki/Enigma_machine | verified (fetched) |
| 2 | Enigma rotor details | Wikipedia | https://en.wikipedia.org/wiki/Enigma_rotor_details | verified (fetched) |
| 3 | Cryptanalysis of the Enigma | Wikipedia | https://en.wikipedia.org/wiki/Cryptanalysis_of_the_Enigma | verified (fetched) |
| 4 | Bombe | Wikipedia | https://en.wikipedia.org/wiki/Bombe | verified (fetched) |
| 5 | Enigma wiring | Crypto Museum | https://www.cryptomuseum.com/crypto/enigma/wiring.htm | verified (fetched) |
| 6 | Enigma: working principle | Crypto Museum | https://www.cryptomuseum.com/crypto/enigma/working.htm | verified (fetched; the summary said it contains no worked example) |
| 7 | Enigma G | Crypto Museum | https://www.cryptomuseum.com/crypto/enigma/g/index.htm | verified (fetched) |
| 8 | Enigma M4 message P1030681 | Crypto Museum | https://www.cryptomuseum.com/crypto/enigma/msg/p1030681.htm | verified (fetched) |
| 9 | Enigma Procedures | Cipher Machines and Cryptology (Dirk Rijmenants) | https://www.ciphermachinesandcryptology.com/en/enigmaproc.htm | verified (fetched) |
| 10 | Enigma/Sample Messages (Barbarossa) | Franklin Heath Ltd wiki | http://wiki.franklinheath.co.uk/index.php/Enigma/Sample_Messages | verified via WebSearch excerpt only; two fetch attempts failed |
| 11 | Enigma M4 | Crypto Museum | https://www.cryptomuseum.com/crypto/enigma/m4/index.htm | unverified (search result only) |
| 12 | U-534 Enigma M4 Messages Cracked | Rijmenants blog | https://rijmenants.blogspot.com/2012/08/u-534-enigma-m4-messages-cracked.html | unverified (search result only) |
| 13 | The original U 534 Enigma M4 messages | Michael Hoerenberg | https://enigma.hoerenberg.com/index.php?cat=The+U534+messages&page=The+messages | unverified (search result only) |

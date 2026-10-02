*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# M-209 data

`m209.json` holds the fixed hardware of the US Army Converter M-209: each key wheel's letters, the offset of the pin that is read (sensed) from the letter shown, and the number of bars in the lug cage. It is vendored here so every nation builds the same machine.

- **Source:** `../../docs/m209-wiring-data.md`. The wheel letters and the `PONMLK` sensed pins come from Wikipedia "M-209". Four published 26-letter key-list checks reproduce only with exactly these alphabets and offsets.
- **Used by:** `ENC-M209-*` in `../../specs/core/m209.md`.
- **Wheel alphabets skip letters.** Wheel 2 has no W. Wheel 3 has no W, Y or Z. Wheels 4-6 stop at U, S and Q. A start position or pin letter that is not on its wheel does not exist on the machine.
- **Sensed offset.** With `AAAAAA` showing, the pins read are `PONMLK`: 15, 14, 13, 12, 11 and 10 letters ahead in each wheel's own alphabet.
- **Not data.** The output mapping (`C = (K - 1 - P) mod 26`) and the stepping rule are behavior, so they are in the spec.
- **Refreshing:** edit the file, note why here, and re-run the conformance vectors in every nation.

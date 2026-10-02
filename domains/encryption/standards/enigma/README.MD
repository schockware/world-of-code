*Authored by Claude Opus 5.5 (Anthropic), with Steven Chock as co-author.*

# Enigma data

`enigma-m3.json` holds the fixed hardware of the Army and Air Force Enigma I, which this project calls M3: the entry wheel, rotors I-V, reflectors B and C, and the plugboard cable limit. It is vendored here so every nation builds the same machine.

- **Source:** `../../docs/enigma-wiring-data.md`, which takes the wiring from Wikipedia "Enigma rotor details" and the Crypto Museum wiring page. The two sources agree string for string.
- **Used by:** `ENC-ENIG-*` in `../../specs/core/enig.md`.
- **Checked:** every rotor string is a permutation of A-Z. Each reflector pairs every letter with a different letter. Rotors I-V and reflector B are exercised by published messages (the Wikipedia examples and the 1941 Barbarossa message). Reflector C has no published vector, only computed ones.
- **Turnover vs. notch.** `turnover` is the window letter showing when the next keypress also steps the rotor to the left. `notchOnRing` is the letter printed on the ring at the notch, always `turnover` + 8. The spec uses `turnover`, and `notchOnRing` is kept for anyone matching a physical machine.
- **Left out.** Naval rotors VI-VIII (two notches), the M4 Greek wheels and thin reflectors, reflector A (replaced before the war), and the commercial and Abwehr machines. Their data is in `../../docs/enigma-wiring-data.md` until a spec uses it.
- **Refreshing:** edit the file, note why here, and re-run the conformance vectors in every nation.

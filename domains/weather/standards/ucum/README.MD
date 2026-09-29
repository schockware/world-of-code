*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# UCUM data

`ucum-essence.xml` is the machine-readable list of UCUM prefixes and units, version 2.2 (revision date 2024-06-17). It is vendored here so every nation validates units against the same data.

- **Source:** https://github.com/ucum-org/ucum (`ucum-essence.xml`), the Unified Code for Units of Measure maintained by the Regenstrief Institute and the UCUM Organization. The specification is at https://ucum.org/ucum.
- **Used by:** `WX-UNIT-001` in `../../specs/core/unit.md`. Each nation writes its own parser, and reads this data (or a form generated from it) to know which atoms and prefixes exist.
- **Refreshing:** replace the file, note the new version here, then re-run each nation's generator or rebuild, and re-run the conformance vectors.
- **Case-sensitive form only.** The `CODE` (upper-case) attribute in the file is the case-insensitive form, which the domain does not use.
- **Grammar** used by the nations' parsers (case-sensitive form, from the UCUM specification, section 2):
  - a unit is an optional leading `/`, then a term
  - a term is components joined by `.` (multiply) or `/` (divide)
  - a component is an annotatable unit with an optional annotation, or an annotation alone, or a whole number (a factor), or a parenthesized term
  - an annotatable unit is a simple unit with an optional exponent (a signed integer)
  - a simple unit is an atom, or a prefix followed by an atom whose `isMetric` is `yes`
  - an annotation is `{`, printable ASCII other than braces, then `}`

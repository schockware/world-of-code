*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# world-of-code: project rules

See `README.MD` for the vision (nations, populations, domains, and the football satire goal) and `nations/README.MD` for how populations attach to nations.

## Layout

```
domains/{domain}/
  contracts/           machine-readable schemas and OpenAPI (peer of specs)
  specs/{set}/         requirements and behaviors, technology-agnostic; the first set is `core`
  specs/{set}/conformance/  language-neutral test vectors (input/expected pairs)
populations/{population}/
  specs/               requirements for how the population is served (conversion, rounding, formatting, i18n, a11y)
nations/{nation}/
  README.MD            the nation's culture, plus Pride and Rivals
  {domain}/designs/    how this nation meets the domain's specs
```

## Spec-first

- **Spec** is what must be true, regardless of technology. **Design** is how one nation achieves it. Never mix them.
- Specs live only at `domains/{domain}/specs`, and designs live only at `nations/{nation}/{domain}/designs`.
- Specs use RFC 2119 language (MUST, SHOULD, MAY) and stable requirement IDs, such as `WX-CONV-003`. Designs cite the IDs they answer.
- Specs state outcomes, not mechanisms. Do not put language- or framework-specific concepts in a spec: exceptions vs. error values, `null`/`nil`/`Option`, `decimal` vs. `float64`, `Task`/goroutines/`Promise`, or mutability. Each nation decides those in its design.
- Specs stay faithful to the source. Unit conversion, rounding, precision, and locale formatting are population concerns, handled where the population interacts, and are not domain requirements.
- Each domain spec file ends with a **Population assumptions** list naming what it expects the population to handle. This makes assumptions made on a population's behalf visible, and feeds `populations/{population}/specs`.
- Specs begin as a `core` set. Whether later additions are enhancements (versioned with core) or extensions (separate sets) is undecided, and nothing goes into `core` that would force the choice.
- Conformance vectors in `specs/{set}/conformance/` are shared by every nation, so results are comparable.
- Contracts stay in `contracts/`, and specs reference them rather than absorbing them.

## Nations

- A nation is a language or framework with its own culture (opinions and ceremonies). If it carries its own culture, it is a separate nation, even when it shares a language with another.
- Names are spelled out and say the language or framework and its shape: `csharp-lib`, `dotnet-cli`, `dotnet-mvc`, `aspnet-webapi`, `golang-lib`, `golang-webapi`, `react`, `vue`. Use `golang`, never `go`, in names.
- Population follows the nation. Do not reference populations in code that the framework already implies. Libraries have no population, and leaking one into a library is an anti-pattern. Supported populations are stated in READMEs.
- Nation READMEs use these sections in order: Culture, Ceremonies and tooling, Populations, Domain translation, Border crossings, Pride, Rivals, Domains.
- Technical sections are the truth. Pride and Rivals are affectionate banter that must stay grounded in real differences.
- Ambiguous nation boundaries (TypeScript, Blazor) are decided as they come up and recorded in the root `README.MD`.

## Authorship

Put a byline at the top of files. If Claude created the file fresh, Claude is the author and Steven Chock is co-author. If Steven created it first, Steven is the author and Claude is co-author. If ownership of the sections has become muddied, call it joint authorship. Use the current Claude model name.

## Files

- Follow the existing `README.MD` naming for READMEs. This file is `CLAUDE.md`, lowercase extension, because Claude Code looks for that exact name on case-sensitive filesystems.

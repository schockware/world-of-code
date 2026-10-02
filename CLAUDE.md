*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# world-of-code: project rules

See `README.MD` for the vision (nations, populations, domains, and the football satire goal) and `nations/README.MD` for how populations attach to nations.

## Layout

```
domains/{domain}/
  contracts/           machine-readable schemas and OpenAPI (peer of specs)
  standards/           vendored data from external standards (for example UCUM), shared by every nation
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
- Names are spelled out and say the language or framework and its shape: `csharp-lib`, `dotnet-cli`, `dotnet-mvc`, `aspnet-webapi`, `golang-lib`, `golang-webapi`, `rust-lib`, `typescript-lib`, `react`, `vue`. Use `golang`, never `go`, in names.
- Population follows the nation. Do not reference populations in code that the framework already implies. Libraries have no population, and leaking one into a library is an anti-pattern. Supported populations are stated in READMEs.
- Nation READMEs use these sections in order: Culture, Ceremonies and tooling, Populations, Domain translation, Border crossings, Pride, Rivals, Domains.
- Technical sections are the truth. Pride and Rivals are affectionate banter that must stay grounded in real differences.
- Ambiguous nation boundaries (Blazor, plain JavaScript) are decided as they come up and recorded in the root `README.MD`. TypeScript is already decided: it is its own nation.

## Authorship

Put a byline at the top of files. If Claude created the file fresh, Claude is the author and Steven Chock is co-author. If Steven created it first, Steven is the author and Claude is co-author. If ownership of the sections has become muddied, call it joint authorship. Use the current Claude model name.

## Implementer actors and the prompt history

A chat acts as an **implementer actor** when it takes specifications and performs code (designs, implementations, conformance runs) rather than authoring specs. Every implementer actor MUST log each prompt it handles. Definitions, disclaimers, and rationale live in `PROMPT_HISTORY_README.MD`; read it before the first entry.

Two files at the repository root:

- `PROMPT_HISTORY.LOG` is committed. It holds the **summary prompt**, never the original text.
- `PROMPT_HISTORY.REDACTED` is gitignored. It holds the **original prompt, redacted**, under the same entry heading.
- `PROMPT_HISTORY.AUDIT.EXAMPLE` is committed and holds a sample audit line for reference. Never write real audit lines to it.
- `PROMPT_HISTORY.AUDIT` is gitignored. It holds one line per entry whose redaction needs supplemental review. It never contains the redacted content, only categories.

Rules:

- **Order:** newest entry first in both files. Prepend directly under the header line; never append and never rewrite older entries.
- **Every prompt:** log each prompt, including questions and discussion. When no files changed, the action is `no code changes`. Skip only prompts whose purpose is to change the logging process itself (these files, their rules, or `PROMPT_HISTORY_README.MD`).
- **When:** write the entry at the end of handling a prompt, before the final reply. If several prompts arrive in one turn, write one entry per prompt. Include the log in the same commit as the work when a commit is made.
- **Time:** get the local time from the shell (`date "+%Y-%m-%d %H:%M"`). Do not guess it.
- **Gitignore check:** before writing to `PROMPT_HISTORY.REDACTED` or `PROMPT_HISTORY.AUDIT`, confirm `.gitignore` lists it. If it does not, add it first.
- **Redaction:** replace secrets, credentials, tokens, keys, identifiers (UUIDs, account IDs), personal data, and pasted third-party content with `[REDACTED:{category}]` in the `.REDACTED` file. Redaction is best effort, and the gitignore is the backstop.
- **Entry format** for `PROMPT_HISTORY.LOG` (entries separated by a line of `---`):

```
## {YYYY-MM-DD HH:MM} | {model name and ID}
Summary prompt: {one to three sentences of what was asked, in your words; no verbatim quotes}
Redaction check: {passed | what was removed, by category}; original in PROMPT_HISTORY.REDACTED
Actions: {files created or changed, requirement IDs answered, commands run, anything skipped or failing; or "no code changes"}
Tokens: ~{N} (derived|estimated); remaining ~{N}
---
```

- **Audit line:** if anything was redacted, or you are unsure whether something should have been, prepend one line to `PROMPT_HISTORY.AUDIT`: `{YYYY-MM-DD HH:MM} | {model} | {categories} | {why review is needed} | OPEN`. Entries with nothing redacted get no line. When a human reviews one, prepend a new line `{date time} | REVIEWED {heading time of original} | {outcome}`; never edit older lines.
- **Entry format** for `PROMPT_HISTORY.REDACTED`: the same heading line, then `Prompt: {original prompt, redacted}`, then `---`.
- **Token tally:** use `~` on every figure and round to the nearest 100. Prefer a derived figure, the drop in the visible "tokens left" counter across the prompt; otherwise label it `estimated`. The tally excludes the cost of writing the entry itself. The counter can move unpredictably, so treat it as approximate.
- **Model:** use the current model name and ID, as in the Authorship rule.
- Neither log has a byline, because the newest entry must stay at the top and each entry names its model.

## Files

- Follow the existing `README.MD` naming for READMEs. This file is `CLAUDE.md`, lowercase extension, because Claude Code looks for that exact name on case-sensitive filesystems.

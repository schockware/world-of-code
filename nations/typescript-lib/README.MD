*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Nation: typescript-lib

TypeScript packages and isolated business layers.

## Culture
- Structural, gradual typing on top of JavaScript. If the shape matches, the type matches.
- The types are erased before the code runs. Nothing checks them at runtime, so anything from outside the program (JSON, a network response) must be validated by hand before being trusted.
- Discriminated unions and literal types model states precisely. `any` is the escape hatch, and a strict configuration is the culture's way of forbidding it.
- Immutability is a convention (`readonly`), not a guarantee.
- `null` and `undefined` both exist, and the difference between them is a decision the team makes explicitly.

## Ceremonies and tooling
- `tsconfig.json` with `strict`, plus stricter flags such as `noUncheckedIndexedAccess` and `exactOptionalPropertyTypes`.
- `package.json` and npm, ES modules versus CommonJS, and a compile or type-stripping step before running.
- A linter (ESLint) and a formatter (Prettier), with a test runner chosen per project.
- Declaration files (`.d.ts`) describe shapes for consumers.

## Populations
None. This nation is population-free by design. Leaking population details (HTTP, DOM, prompts, CLI flags) into a library is an anti-pattern.

## Domain translation
Domain contracts (`../domains/{domain}/contracts`) become readonly types and literal unions. Because types are erased, each contract also needs a runtime guard that checks incoming data, and the guards are where the domain rules actually live.

## Border crossings
- Consumed by `react`, `vue`, and later Node application nations. Their own culture owns population handling.
- Cross-compare with `csharp-lib`: the same language designer, but a different soul. One is checked at runtime, and one is checked only by hand.
- Cross-compare with `golang-lib`: both are pragmatic, but Go's types survive to runtime and TypeScript's do not.

## Pride
"We made JavaScript bearable, and then we made it the language of everything. Our type system can describe things other languages can't spell, and our ecosystem has a package for whatever you're about to write. Run us in a browser, a server, or a toaster."

## Rivals
- `rust-lib`: "You spent three weeks satisfying the borrow checker. We shipped on Tuesday."
- `golang-lib`: "Small and simple, sure. Where are your union types?"
- `csharp-lib`: "Same architect, and we got the better sense of humor."

## Domains
`./{domain}/` folders are added as specs are written.

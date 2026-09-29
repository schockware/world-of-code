*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Nation: golang-lib

Go packages and isolated business layers.

## Culture
- Simplicity and explicitness. Few abstractions, little magic.
- Errors are values, returned and handled explicitly. No exceptions.
- Small interfaces defined by the consumer, and composition through structs. No inheritance.
- Zero values should be useful. Exported surface area is kept small.
- Concurrency is available (goroutines, channels), but a library should not impose it. Where cancellation matters, `context.Context` is passed in.

## Ceremonies and tooling
- `go.mod` modules, semantic import versioning (`/v2`), packages as the unit of organization.
- `gofmt` as non-negotiable formatting, `go vet` and `golangci-lint`.
- `internal/` for private packages, doc comments on every exported name.
- Table-driven tests with the `testing` package, placed beside the code in `_test.go` files.

## Populations
None. This nation is population-free by design. Leaking population details (HTTP, prompts, CLI flags) into a library is an anti-pattern.

## Domain translation
Domain contracts (`../domains/{domain}/contracts`) become structs and functions or small interfaces. Invalid inputs are returned as errors, and the domain's unit and quantity rules are enforced in code. Nothing here knows how it is called.

## Border crossings
- Consumed by `golang-webapi`, which owns all population handling.
- Cross-compare with `csharp-lib`: same domain and no population, but different culture. Error values instead of exceptions, small consumer-side interfaces instead of DI-shaped abstractions, and explicit constructors instead of a container.

## Pride
"We shipped the whole playbook on one page. No inheritance, no container, no magic. `gofmt` ended every style argument before it started, and our binary needs nothing installed to run."

## Rivals
- `csharp-lib`: "Three layers of interfaces to add two numbers. We wrote `if err != nil` and went home early."
- `react` and `vue`: they spend a whole match arguing about state. We just pass the value.

## Domains
`./{domain}/` folders are added as specs are written.

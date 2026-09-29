*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Nation: golang-webapi

HTTP APIs in Go (named `golang` in nation names, since "Go" is shorthand).

## Culture
- Simplicity and explicitness. Few abstractions, little magic, "a little copying is better than a little dependency".
- Errors are values, returned and handled explicitly. No exceptions.
- Composition through small interfaces (defined by the consumer) and structs. No inheritance.
- Concurrency is built in (goroutines, channels, `context.Context`), and cancellation is passed explicitly.
- The standard library is the default. Frameworks (chi, gin, echo) are optional and often avoided.

## Ceremonies and tooling
- `go.mod` modules, `gofmt` as non-negotiable formatting, `go vet` and `golangci-lint`.
- `net/http` handlers and `http.ServeMux`, middleware as function wrappers.
- Struct tags for JSON, table-driven tests with the `testing` package.
- OpenAPI is usually generated from the spec (oapi-codegen), not the other way round.

## Populations
Typically `populations/automations` (other services), and `populations/humans` or `populations/ai` through a consuming nation. State the supported populations in each domain README.

## Domain translation
Domain schemas become structs with JSON tags, and operations become handlers. Missing or invalid values are returned as errors, and the domain's unit and quantity rules are enforced explicitly, not by convention.

## Border crossings
- Serves the same OpenAPI contracts as `aspnet-webapi`. This is the direct comparison, since the language and culture differ while the contract is the same.
- Consumed by `react`, `vue`, `dotnet-cli` and others.
- Calls into `golang-lib` for business logic, the counterpart of `csharp-lib` for `aspnet-webapi`.

## Pride
"Cold start? What cold start. One static binary, a tiny container, and a standard library that already knows HTTP. We handle ten thousand connections while other nations are still loading their frameworks."

## Rivals
- `aspnet-webapi`: "Your startup needs a composition root and a configuration file. Ours needs `main`."
- `vue` and `react`: "We're happy to serve you JSON. Please stop asking us to render anything."

## Domains
`./{domain}/` folders are added as specs are written.

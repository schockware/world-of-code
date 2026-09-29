*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Nation: dotnet-cli

.NET command-line applications. C# language, different culture from `csharp-lib`.

## Culture
- Programs are commands: arguments in, exit code and text out.
- Composable with other tools through stdin/stdout, so output format is part of the contract.
- Short-lived process. Startup cost and deterministic behavior matter more than long-lived state.

## Ceremonies and tooling
- `Program.Main` / top-level statements, generic host for DI and configuration when needed.
- Argument parsing (`System.CommandLine`, Spectre.Console.Cli), `dotnet tool` packaging.
- Exit codes, `--help` conventions, and stderr vs. stdout discipline.

## Populations
Whoever runs the command. Both are possible, so state which are supported in each domain README:
- `populations/humans` at a terminal (help text, readable output, color, prompts).
- `populations/automations` in scripts and pipelines (machine-readable output such as `--json`, stable exit codes).

## Domain translation
Domain operations become commands and subcommands. Domain schemas become output shapes (human table, or JSON matching the contract). Business logic is delegated to `csharp-lib`.

## Border crossings
- Calls into `csharp-lib` for business logic.
- May call HTTP APIs (`aspnet-webapi`, `golang-webapi`) via the OpenAPI contract.
- Consumed by scripts and pipelines in any nation.

## Pride
"No browser, no server, no ceremony beyond the exit code. We pipe, we script, we get out of the way. Every automation on the pitch depends on someone like us."

## Rivals
- `react` and `vue`: "You need a build step, a bundler and a browser to show a number. We print it."
- `aspnet-webapi`: "Long-lived services get all the attention. We do the job and exit with a zero."

## Domains
`./{domain}/` folders are added as specs are written.

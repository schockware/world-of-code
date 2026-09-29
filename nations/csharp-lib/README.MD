*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Nation: csharp-lib

C# class libraries and isolated business layers.

## Culture
- Object-oriented first, with modern C# leaning on records, immutability and pattern matching where it helps.
- Strong static typing, nullable reference types, explicit interfaces.
- Values correctness and testability over convenience. No framework opinions.

## Ceremonies and tooling
- `.csproj` (SDK-style), NuGet packaging, semantic versioning.
- Roslyn analyzers, `.editorconfig`, nullable enabled.
- xUnit/NUnit test projects alongside the library.

## Populations
None. This nation is population-free by design. Leaking population details (HTML, HTTP, prompts, CLI args) into a library is an anti-pattern.

## Domain translation
Domain contracts (`../domains/{domain}/contracts`) become plain types and services, such as records for schemas and interfaces for operations. Nothing here knows how it is called.

## Border crossings
- Consumed by `dotnet-mvc`, `aspnet-webapi` and `dotnet-cli`. The consuming nation owns all population handling.
- Other nations reach this one only through a consumer, never directly.
- Cross-compare with `golang-lib`, `rust-lib` and `typescript-lib`: same role and no population, but different culture.

## Pride
"Our types are so strong they finish your sentences. Nullable reference types, records, pattern matching: we catch the bug before you've had coffee. Twenty years of refinement, and the compiler is our teammate."

## Rivals
- `golang-lib`: "Error values? You return an error and hope someone checks it. We throw, and the stack trace tells the whole story."
- `rust-lib`: "Memory safety without a garbage collector, sure. Ours collects the garbage so you can think about weather."
- `typescript-lib`: "Same architect. We kept the type checker at runtime."
- `dotnet-mvc` and `aspnet-webapi`: family. Same language, but they got famous by wearing our work as a jersey.

## Domains
`./{domain}/` folders are added as specs are written.

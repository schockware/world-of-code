*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Nation: rust-lib

Rust crates and isolated business layers.

## Culture
- Ownership and borrowing. Who owns a value, and who may touch it, is a fact the compiler checks. Memory safety comes without a garbage collector.
- There is no null. Absence is `Option`, failure is `Result`, and both must be dealt with before the code compiles.
- Enums carry data and `match` must be exhaustive. Illegal states are meant to be unrepresentable.
- Traits define behavior, and abstraction is meant to be zero-cost.
- "If it compiles, it works" is the aspiration, and the compiler is strict on purpose.

## Ceremonies and tooling
- `Cargo.toml`, editions, crates.io, and `Cargo.lock` for reproducible builds.
- `cargo fmt` and `cargo clippy` as the shared standard, with warnings often promoted to errors.
- Lifetimes and the borrow checker, `#[derive]` for common traits, and `unsafe` fenced into small audited blocks.
- Unit tests beside the code, integration tests in `tests/`, and documentation tests in doc comments.

## Populations
None. This nation is population-free by design. Leaking population details (HTTP, prompts, CLI flags) into a library is an anti-pattern.

## Domain translation
Domain contracts (`../domains/{domain}/contracts`) become structs and enums. The three answer states become `Option` and `Option`-of-`Option`-shaped types, so the compiler forces every caller to handle each one. Invalid input is a `Result`, never a panic.

## Border crossings
- Consumed by whichever Rust application nations arrive later, which own all population handling.
- Cross-compare with `golang-lib`: both return errors as values, but Rust's type system forces you to handle them and has no `nil`.
- Cross-compare with `csharp-lib`: nullable reference types make absence a warning, while `Option` makes it a type.

## Pride
"No garbage collector, no null, no data races, and no runtime surprises. The compiler argues with you so production doesn't have to. Our enums say exactly what a value can be, and our `match` refuses to compile if you forget a case."

## Rivals
- `golang-lib`: "You return an `error` and hope someone checks it. We won't compile until they do."
- `csharp-lib`: "Nullable reference types are a lint. `Option` is the truth."
- `typescript-lib`: "Your types vanish at runtime. Ours are the runtime."

## Domains
`./{domain}/` folders are added as specs are written.

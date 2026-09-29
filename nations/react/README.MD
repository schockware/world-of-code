*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Nation: react

React with a functional, immutable-state culture.

## Culture
- UI is a function of state. Components are functions.
- State is immutable. Updates produce new values, never mutations.
- Composition over inheritance. Side effects are fenced inside hooks.

## Ceremonies and tooling
- Hooks and their rules (`eslint-plugin-react-hooks`), JSX transpilation (Babel/TypeScript/Vite).
- Dependency arrays, memoization, and keys are the daily discipline.
- TypeScript is expected. It is its own nation (`typescript-lib`), and this nation follows its rules for domain types.

## Populations
`populations/humans`, assumed by the framework. Accessibility and i18n obligations (WCAG, BCP 47) still apply, and are met through the component layer.

## Domain translation
Domain schemas become readonly TypeScript types. Operations become hooks or data-fetching functions. Nothing is mutated in place.

## Border crossings
- Consumes HTTP APIs from `aspnet-webapi`, `golang-webapi` (and others) via the OpenAPI contract.
- Contrast with `vue`: same population and domains, opposite state philosophy.

## Pride
"UI is a pure function of state. Nothing mutates behind your back, so nothing surprises you at 2 a.m. The biggest ecosystem on the pitch, and half the industry already speaks our dialect."

## Rivals
- `vue`: "You mutate state and hope the reactivity system notices. We return a new value and know exactly what changed."
- `dotnet-mvc`: "Refresh the whole page? We only touched the one button."

## Domains
`./{domain}/` folders are added as specs are written.

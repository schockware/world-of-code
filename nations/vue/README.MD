*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Nation: vue

Vue with a declarative, mutable-reactive-state culture.

## Culture
- Templates are declarative. The framework tracks dependencies and re-renders.
- State is mutable and reactive (`ref`, `reactive`). Mutating it is the intended path.
- Progressive adoption. Options API and Composition API coexist.

## Ceremonies and tooling
- Single-file components (`.vue`), the Vue compiler, Vite.
- Composition API, `<script setup>`, `eslint-plugin-vue`, Pinia for shared state.
- Reactivity caveats (unwrapping refs, losing reactivity on destructuring) are the daily discipline.

## Populations
`populations/humans`, assumed by the framework. Accessibility and i18n obligations (WCAG, BCP 47) still apply, and are met through templates and components.

## Domain translation
Domain schemas become reactive models or typed interfaces. Operations become composables. Mutation is expected and idiomatic.

## Border crossings
- Consumes HTTP APIs from `aspnet-webapi`, `golang-webapi` (and others) via the OpenAPI contract.
- Contrast with `react`: same population and domains, opposite state philosophy.

## Pride
"Write a template that looks like HTML, change a value, and the screen updates. No dependency arrays, no ceremony about hooks. Approachable on day one and still scaling on day one thousand."

## Rivals
- `react`: "Immutable everything, and you still need a lint rule to remember how hooks work. We just assign the value."
- `dotnet-mvc`: "We admire the server-rendered classics, and we've kept the parts that still work."

## Domains
`./{domain}/` folders are added as specs are written.

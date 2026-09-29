*Authored by Steven Chock, with Claude Sonnet 5.5 (Anthropic) as co-author.*

A long time ago I did a project with heavy interconnectivity across various systems and languages.

Let's have some fun and start interconnecting domains using various tools.

I'm using a satirical description "nation of" to evoke the differences in culture, language, and frameworks.

The satirical description "population of" is used to describe who or what interacts with any software we build.

"Domain of" is as straight forward as it comes.

## Why nations?

The satire is deliberate, and so is the substance. Every language and framework really does have its own culture, its own opinions and its own ceremonies, and pretending otherwise is how specs end up quietly favoring one of them.

The long-term goal is to let every nation play digital soccer/football against the others. Each nation implements the same domain contracts, so the rules are identical and the match is fair. Around the matches we'll build satirical rivalries, news broadcasts, and perspectives from each nation, jabbing at the others while praising its own strengths.

- The technical sections of each nation README are the truth. The **Pride** and **Rivals** sections are the banter.
- The jabs should be affectionate and grounded in real differences, and never a substitute for them.

## Culture assumptions

A nation is defined by its culture: the opinions and ceremonies a language or framework imposes on the people (and tools) working in it. It is not defined by runtime or deployment shape.

- **The test:** if a language or framework carries its own opinions and ceremonies, it is a separate nation, even when it shares a language with another. `csharp-lib` and `dotnet-mvc` both use C#, but are different nations.
- **Paradigm matters.** Functional vs. object-oriented, immutable vs. mutable, and imperative vs. declarative all shape how a domain gets expressed. React (functional, immutable state, hooks) and Vue (declarative, mutable reactive state) are different nations for that reason.
- **Ceremonies matter.** Required lint rules, transpilers, composition roots, project files and framework conventions are part of the culture.
- **Naming.** Nations are named by their culture at a glance, such as `csharp-lib`, `dotnet-cli`, `aspnet-webapi`, `react`. The name should tell you the language or framework and its shape.
- **Population follows the nation.** See `nations/README.MD`. A nation's framework implies its population (or none), and we don't repeat that in code.
- **Borders are contracts.** Nations interact through the contracts in `domains/{domain}/contracts`, not through each other's internals.
- **These lines will move.** Ambiguous cases (TypeScript vs. JavaScript, Blazor, etc.) get decided as we hit them, and the decision is recorded here.
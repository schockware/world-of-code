*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Nation: dotnet-mvc

ASP.NET Core MVC with Razor views (server-rendered pages). C# language, different culture from `csharp-lib` and from `aspnet-webapi`.

## Culture
- The server renders the page. Controllers pick a view and pass it a model.
- Convention over configuration: controllers, routes, model binding, filters, view discovery.
- Dependency injection and the middleware pipeline shape everything.
- View models at the edge and domain types inside.

## Ceremonies and tooling
- `Program.cs` composition root, `appsettings.json`, `Views/` and `Controllers/` layout.
- Razor syntax, tag helpers, partials, layouts, anti-forgery tokens.
- Model validation with server and client-side rendering of validation messages.

## Populations
`populations/humans`, handled by the framework, so no explicit reference is needed in code. Accessibility and i18n obligations (WCAG, BCP 47) still apply and are met in the views. State the supported populations in each domain README.

## Domain translation
Domain schemas become view models and operations become controller actions and views. Business logic is delegated to `csharp-lib`, not implemented in controllers.

## Border crossings
- Calls into `csharp-lib` for business logic.
- May call `aspnet-webapi` or `golang-webapi` over the OpenAPI contract instead of `csharp-lib` directly.
- Contrast with `react` and `vue`: same population, but the server owns rendering instead of the client.

## Pride
"We serve the finished page, hot, before the client's JavaScript has finished downloading. Convention does the wiring, so the team does the thinking. Old school, fully loaded, and still on the pitch."

## Rivals
- `react` and `vue`: "You ship a whole application to the browser to draw a table? We rendered it on the way out the door."
- `aspnet-webapi`: the sibling who got to skip the views and go professional as pure API.

## Domains
`./{domain}/` folders are added as specs are written.

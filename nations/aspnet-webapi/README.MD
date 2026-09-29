*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Nation: aspnet-webapi

ASP.NET Core Web API. C# language, different culture from `csharp-lib` and from `dotnet-mvc`.

## Culture
- The API is the product. Contracts (OpenAPI) come first and responses are data, not pages.
- Convention over configuration: controllers or minimal APIs, routing, model binding, filters.
- Dependency injection and the middleware pipeline shape everything.
- Objects are DTOs at the edge and domain types inside.

## Ceremonies and tooling
- `Program.cs` composition root, `appsettings.json`, `[ApiController]`, `[Route]` attributes.
- OpenAPI generation (Swashbuckle/NSwag), model validation, ProblemDetails for errors.
- Authentication and authorization middleware, CORS, versioning.

## Populations
Typically `populations/automations` (other services). It is also consumed by `populations/humans` through a UI nation, and by `populations/ai` when wrapped by an MCP nation. State the supported populations in each domain README.

## Domain translation
Domain OpenAPI contracts map to endpoints and DTOs. Business logic is delegated to `csharp-lib`, not implemented in controllers.

## Border crossings
- Calls into `csharp-lib` for business logic.
- Serves `react`, `vue`, `dotnet-cli` and others over the OpenAPI contract.
- Sibling of `golang-webapi`. Both should honor the same contract, and differences in how each does so are the interesting part.

## Pride
"Enterprise-grade before it was cool. Auth, versioning, validation and OpenAPI are all built in, and our contracts come with a documentation page and a test harness. We arrive fully equipped."

## Rivals
- `golang-webapi`: "Twelve lines to serve a route? Adorable. Ours comes with middleware, DI, and a pipeline you can draw on a whiteboard."
- `dotnet-mvc`: "Still rendering HTML? The world moved to JSON."

## Domains
`./{domain}/` folders are added as specs are written.

# Humans

How software describes the humans who use it.

This project models a human's **context** as a set of independent layers, and describes each layer in an **OpenAPI 3.x** document so that any client (web, desktop, mobile, CLI, agent) can tell a service who it is serving, and the service can adapt.

The layers are independent axes, not user "types". A Japanese-speaking admin on a watch, using a screen reader, at night, with reduced motion, is a valid combination.

## Scope

| Phase | Layer | Status |
|-------|-------|--------|
| 1 | Language | Core |
| 1 | Formatting | Core |
| 1 | Geolocation | Core |
| 1 | Browser / client | Core |
| 2 | UX: accessibility and demographics | Future |
| Later | Identity, situational context, preferences, relationships | Backlog |

## Phase 1: Application basics

### Language
- A BCP 47 tag such as `en-US` or `sr-Latn-RS`.
- Ordered list of preferences with fallbacks (`Accept-Language` semantics).
- Script and text direction (LTR, RTL, vertical).
- Plural and grammar rules come from CLDR, not from us.

### Formatting
Often derived from the locale but overridable, because people frequently want `en-US` language with metric units, or 24-hour time.
- Time zone (IANA name, e.g. `Europe/Paris`). This is separate from locale.
- Calendar system (Gregorian, Hijri, Buddhist, Japanese) and first day of the week.
- Numbering system and number, currency, and date/time formats.
- Currency (ISO 4217) and measurement system (metric, imperial, US customary).
- Collation (sort order), name order (given/family), address and phone formats.

### Geolocation
- Country/region (ISO 3166) is often enough; precise coordinates are opt-in.
- Legal jurisdiction (GDPR, CCPA, data residency) derives from this layer.
- Region is not language. Do not infer one from the other.

### Browser / client
- Platform: web, desktop, mobile, CLI, API, agent.
- User agent, OS, and version (prefer Client Hints over UA sniffing).
- Form factor, viewport, pixel density, input modality (touch, mouse, keyboard, voice).
- Display preferences: dark mode, contrast.
- Network quality and offline capability.

## Phase 2: UX (future)

You would not give a child a 30-page form, and you would not give a blind person a picture-only experience. This layer lets a service adapt the *experience*, not just the formatting.

- **Accessibility:** screen reader, magnification, reduced motion, contrast, text scaling, captions, color-vision deficiency, cognitive load and plain-language needs.
- **Demographics:** age band, guardianship, literacy and language proficiency, domain expertise.

Ground rules, since this layer is sensitive:
- **Declared, not inferred.** The human states it; we do not guess.
- **Opt-in and minimal.** Collect the least needed to adapt the experience.
- **Coarse over precise.** An age band, not a birthdate.
- **Never a gate.** No feature should require disclosing it.

## Modeling in OpenAPI 3.x

Recommended target is OpenAPI 3.1 (full JSON Schema support).

Approach:
1. Each layer is a schema under `components/schemas` (`Language`, `Formatting`, `Geolocation`, `Client`, later `Ux`).
2. `HumanContext` composes them, all properties optional.
3. Where an HTTP standard exists, use it as a header parameter under `components/parameters`:
   - `Accept-Language` for language
   - `User-Agent` and `Sec-CH-UA*` Client Hints for browser/client
4. Where none exists (time zone, geolocation, currency, units, UX), use a custom header or a request-body `HumanContext`. Custom headers use the `X-` prefix in this document.
5. Every field has a sensible default so a client sending nothing still works.

### Contracts

The contracts live in [contracts/](contracts/) and are validated as OpenAPI 3.1.

| File | Contents |
|------|----------|
| [human-context.openapi.yaml](contracts/human-context.openapi.yaml) | `HumanContext` composition, geolocation, client, UX placeholder |
| [i18n.openapi.yaml](contracts/i18n.openapi.yaml) | Language and formatting, using BCP 47 and CLDR/`Intl` values straight up |
| [a11y.openapi.yaml](contracts/a11y.openapi.yaml) | Accessibility preferences and WCAG conformance targets |
| [wcag-catalog.openapi.yaml](contracts/wcag-catalog.openapi.yaml) | Generated list of every WCAG 2.x success criterion, from W3C data |

The catalog is generated: `python scripts/build_wcag_catalog.py`.

### Source standards

Local copies of the standards are in [standards/](standards/), see [standards/README.md](standards/README.md).

## Open questions
- Header vs body: which layers travel on every request and which are sent once at session start?
- How is the `Ux` schema versioned, given its sensitivity?
- Do we define our own vocabulary for accessibility needs or adopt an existing one (e.g. WCAG-aligned)?

## Standards referenced
BCP 47, Unicode CLDR / ICU, IANA time zone database, ISO 3166, ISO 4217, HTTP Client Hints, WCAG, ARIA, OpenAPI 3.1.

## Later
Identity and entitlements (roles, tenancy, account state), situational context (time, session, cohort), preferences and personalization, and relationships (human to human, human to agent).

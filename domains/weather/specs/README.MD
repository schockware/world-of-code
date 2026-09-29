*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Weather specs

Specs say **what must be true** about weather data and behavior, regardless of language or framework. How a nation achieves it is a design, and lives in `nations/{nation}/weather/designs`. See the root `CLAUDE.md` for the spec-vs-design rule.

## Principle: stay faithful to the source

The domain carries data as the source reported it. Unit conversion, rounding, decimal places, locale formatting and export rules are **population concerns**, decided at the point of interaction (for example a human's locale preference, or an automation's export pipeline). They are not domain behavior, so specs do not require them. A nation's population handling implements them, and any nation is free to design that layer its own way.

## Spec sets

Specs are organized into named sets. The first is **`core`**, the minimum every nation must meet.

```
specs/
  core/              requirements for the core set
    conformance/     test vectors for the core set
```

Whether later work such as the NWS quality-control flag becomes an **enhancement** (adds to core, versioned with it) or an **extension** (a separate set, for example one that normalizes US sources to the international standard) is deliberately undecided. Nothing goes into core that would force that decision. When it is made, the decision and its rules are recorded here.

## Relationship to the rest of the domain

| Path | Holds | Relationship to specs |
|---|---|---|
| `../contracts/domain` | JSON Schemas and OpenAPI for `Quantity`, `Station`, `Observation` | Specs reference these for shape. They do not restate them. |
| `../contracts/us`, `../contracts/international` | Source-derived contracts (NWS, OGC SensorThings) | Specs about mapping cite the `*.notes.json` files. |
| `../docs` | Standards research | Specs cite it as rationale, never as a requirement. |
| `core/conformance` | Language-neutral test vectors | Every testable requirement has at least one vector. |
| `nations/{nation}/weather/designs` | Per-nation designs | Each design cites the requirement IDs it answers. |

Shape questions ("is `unit` required?") belong to the contract. Behavior questions ("what happens when a unit is not UCUM?") belong here.

## Writing requirements

- Use RFC 2119 keywords (**MUST**, **SHOULD**, **MAY**) in capitals, with their standard meanings.
- One testable statement per requirement. If it needs "and", split it.
- State outcomes, not mechanisms. Write "the input is rejected with reason `unit.not-ucum`", not "throws an exception". Presence, absence and failure are described in domain terms, since each nation models them its own way.
- Do not name language types, frameworks, or libraries. Do name domain concepts, standards (UCUM, RFC 3339) and contract schemas.
- Give every requirement a rationale line. If you cannot say why it exists, it probably should not.

### Requirement IDs

Format: `WX-{AREA}-{NNN}`. IDs are stable. Once published, an ID is never reused or renumbered. A removed requirement is marked `Withdrawn`, and stays in the file.

| Area | Covers |
|---|---|
| `QTY` | `Quantity`: the three answer states, value fidelity |
| `UNIT` | Unit form (UCUM, no namespace) and preservation of the source unit |
| `TIME` | `observedAt` handling: RFC 3339 parsing, ordering, time zones |
| `STN` | `Station`: identity and equality |
| `OBS` | `Observation`: required elements, element-level rules |
| `MAP` | Mapping to and from the US (NWS) and international (SensorThings) forms. **Reserved, unsupported for now.** |

New areas are added here before they are used.

### Requirement entry format

Specs are Markdown files, one per area (`qty.md`, `unit.md`, and so on), with each requirement written as:

```markdown
### WX-UNIT-002
**Status:** Draft
**Level:** MUST
A quantity's unit MUST be carried exactly as the source reported it, without
conversion.
**Rationale:** Conversion is a population concern, and altering the source unit loses fidelity.
**Contract:** `quantity.json`
**Vectors:** `unit.json#preserve-*`
```

Status values are `Draft`, `Accepted`, `Withdrawn`.

## The three answer states

The domain distinguishes three states for any element, in the terms the source can actually express:

| State | Meaning | In the contract |
|---|---|---|
| **Not reported** | The element was never answered. | The element is omitted from the `Observation`. |
| **Reported without a value** | The element was answered, and the answer was empty. | A `Quantity` whose `value` is `null`, with its `unit`. |
| **Reported** | The element was answered with a value. | A `Quantity` with a numeric `value` and `unit`. |

The `Quantity` object is the wrapper. The contract already supports this: an `Observation` element is an object, not a bare number, so "no answer" (the element is absent) and "an empty answer" (the object with a null `value`) can never be confused. Specs require that the three states stay distinct through every operation, and never collapse one into another.

How a nation represents "not reported" (an optional, a nullable reference, a pointer, an absent property) is a design decision. The states are the spec.

## Conformance vectors

Vectors live in `core/conformance/` as JSON so every nation can read them with its own tooling. Each file is one area. A vector looks like:

```json
{
  "id": "value-null-is-reported-without-value",
  "requirements": ["WX-QTY-003"],
  "operation": "classify",
  "input": { "observation": { "station": { "id": "KSEA" }, "observedAt": "2026-01-01T00:00:00Z", "temperature": { "value": null, "unit": "Cel" } }, "element": "temperature" },
  "expect": { "outcome": "ok", "state": "reported-without-value" }
}
```

Rules:
- `operation` names a domain operation defined in a spec (`classify`, `validate`), never a function in a language.
- `outcome` is `ok` or `rejected`. A rejection carries a `reason` code defined in a spec, such as `unit.not-ucum`. Reason codes are the language-neutral form of what each nation implements as exceptions, error values, or status codes.
- Vectors describe behavior only. How a nation loads and runs them is part of its design.

## Status of this folder

Drafted for `core`:

1. `core/qty.md` (WX-QTY-001 to 010) with `core/conformance/qty.json`: the three answer states, value fidelity, and rejection of malformed quantities.
2. `core/unit.md` (WX-UNIT-001 to 004) with `core/conformance/unit.json`: unit form (UCUM, no namespace prefix) and preservation of the source unit. No conversion. Its unit codes are not yet verified against a UCUM reference implementation.

Still to draft: `time.md`, `stn.md`, `obs.md`, which build on the quantity specs.

`MAP` is unsupported for now and gets no spec.

## Decisions

| Question | Decision |
|---|---|
| Default unit per element, and conversion vs. validation | No defaults and no conversion in the domain. Carry the source unit. Locale-based conversion belongs to population handling. |
| Conversion tolerance | Not applicable. There is no domain conversion. Rounding and precision are population preferences. |
| SensorThings ObservedProperty URIs | Unsupported for now. `MAP` is reserved. Needs more research (WMO Codes Registry, CF standard names). |
| NWS quality-control flag | Left out of `core`. Enhancement vs. extension is undecided, and `core` avoids forcing the choice. |
| Absence beyond `null` | Three distinct states (not reported, reported without a value, reported), carried by the `Quantity` wrapper. |
| Population handling of conversion, rounding, formatting | Populations get their own specs, at `populations/{population}/specs`. |
| JSON `null` for an element | Not allowed. "Not reported" has exactly one form: the element is omitted. |

## Population assumptions

Because the domain leaves conversion, rounding and formatting to populations, every place where a spec quietly assumes something about a population must be visible. Each domain spec file ends with a **Population assumptions** list naming what it expects the population to handle, for example "the presentation layer converts `Cel` to `[degF]` for a US locale". These lists are the to-do list for `populations/{population}/specs`, and they keep the domain from making choices on a population's behalf without saying so.

For now the domain owns these assumptions on behalf of its populations. That is expected to break down when a subgroup of a population needs something different (a scientist who wants raw precision, say). When that happens, the assumption gets revisited in the domain spec and the population spec together, and the change is recorded here.

## Open questions

- Enhancement or extension: how a second spec set relates to `core`, and how versions are named. This is decided when the first thing outside `core` arrives.
- The name and meaning of "not reported". It may later split into "not reported" and "no value provided" if knowing why data is missing matters. That change would add resolution to the data source's quality and would not change the meaning of what is already there, so it can wait.
- The layout of `populations/{population}/specs`, including whether `core` sets exist there too, and how the "Population assumptions" lists in domain specs map to population requirements.

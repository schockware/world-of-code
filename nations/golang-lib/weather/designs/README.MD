*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# golang-lib weather designs

Designs for the weather domain specs, in Go culture. Specs live in `domains/weather/specs`, and each design cites the requirement IDs it answers.

| Spec set | Area | Design |
|---|---|---|
| core | QTY | `core/qty.md` |
| core | UNIT | `core/unit.md` |

## Module shape

The module root is `nations/golang-lib/weather`, one level up from `designs/`.

```
go.mod                module world-of-code.example/golang-lib/weather
*.go                  package weather: Quantity, Observation, AnswerState, ParseQuantity, ParseObservation
ucum/                 package ucum: the UCUM validator, with a generator for its unit data
internal/vectors/     loads the conformance JSON for tests
```

- `gofmt` and `go vet` clean. `golangci-lint` is not installed here yet.
- Table-driven tests with one `t.Run` subtest per conformance vector, named by file and vector `id`, loading `domains/weather/specs/core/conformance/*.json` from the repository. Passing vectors is the scoreboard.
- The library stays population-free and uses the standard library only.
- Run with `go test ./...` from the module root. Regenerate the unit data with `go generate ./...`.

## Vector operations

| Operation | Go call |
|---|---|
| `validate` | `ParseQuantity`, checking `errors.As` for a `*RejectionError` |
| `classify` | `ParseObservation`, then `Observation.State(element)` |
| `roundtrip` | `json.Marshal` of the parsed value, then compare with `reflect.DeepEqual` after decoding both sides |

## Status

All 33 core vectors pass, and every one of the 312 atoms in the UCUM unit list is accepted by `ucum.Valid`.

## Open items

- **Provisional reasons.** `ReasonQtyNotObject` and the `ReasonObs*` reasons exist so the parsers have something to return, but no spec requirement backs them yet. They are listed in `weather.go` under a "Provisional" comment. WX-OBS and WX-TIME should define or replace them.
- **Timestamps.** `ObservedAt` is a `time.Time` formatted as RFC 3339, so `+00:00` is written back as `Z`. That is TIME's territory.
- **Whether `float64` is enough,** or `json.Number` should carry source text. Decide if the spec ever needs decimal text beyond about 15 to 17 significant digits.
- **`golangci-lint`** is not configured yet.

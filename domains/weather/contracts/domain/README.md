# Domain contract: what is shared and what is not

`contracts/us` is derived only from the NWS API OpenAPI document (3.11.0).
`contracts/international` is derived only from OGC SensorThings v1.1 (18-088).
This contract keeps only what both can express. Field-level mappings are in the
`*.notes.json` files next to each schema (`maps.us`, `maps.international`).

## Shared

| Domain | US (NWS) | International (SensorThings) |
|---|---|---|
| `Quantity.value` | `QuantitativeValue.value` | `Observation.result` (OM_Measurement) |
| `Quantity.unit` (UCUM) | `unitCode`, un-namespaced or `uc:` | `unitOfMeasurement.symbol` (SHOULD be UCUM) |
| `observedAt` | `timestamp` | `phenomenonTime`, instant form |
| `station` | `stationId`, `stationName` | Thing (via Datastream) |
| Elements | temperature, dewpoint, relativeHumidity, wind*, pressure, visibility | any ObservedProperty |

## Left out, and why

| Left out | Why it could not be shared |
|---|---|
| Quality flag | NWS uses a fixed MADIS enum (`Z C S V X Q G B T`). SensorThings uses `resultQuality`, a list of ISO 19157 DQ_Elements with no JSON shape in the standard. No common representation. |
| Unit namespaces (`wmoUnit:`, `nwsUnit:`) | NWS calls them deprecated, and `nwsUnit` is custom. `Quantity.unit` forbids them by pattern. NWS values using them cannot be converted without a mapping table we do not have yet. |
| Unit name and definition URI | SensorThings requires `name`, `symbol` and `definition`. NWS carries only one code string. The domain keeps the code only. |
| Non-UCUM SensorThings units | The standard says units SHOULD, not SHALL, follow UCUM. A service using something else has no `Quantity` form here. |
| Observation time as an interval | SensorThings `phenomenonTime` can be an interval. NWS `timestamp` is an instant. Accumulations and extremes need an interval. |
| Aggregation windows | NWS bakes the window into the field name (`precipitationLastHour`, `precipitationLast3Hours`, `maxTemperatureLast24Hours`). SensorThings expresses it as time on the Observation. Different modelling, so all of these are excluded. |
| `resultTime` | Mandatory in SensorThings, absent in NWS. |
| Other-typed results | SensorThings `result` may be a count, category, truth value, or anything else. NWS elements are all numeric quantities. |
| `presentWeather`, `cloudLayers`, `rawMessage`, `textDescription` | NWS-only, tied to METAR coding. SensorThings defines no equivalent structures. |
| `heatIndex`, `windChill` | NWS-only derived values. SensorThings could carry them as Datastreams, but the standard defines nothing for them. |
| `elevation`, station geometry | NWS uses a WKT string. SensorThings uses Location and FeatureOfInterest with an encodingType. Not reconciled yet. |
| Required fields | The NWS schema declares none. SensorThings has mandatory ones. The domain requires only `station` and `observedAt`. |
| Wire shape | NWS is a single record. SensorThings is normalised across Thing, Datastream, ObservedProperty and Observation. The domain uses the flat NWS-style record, so an international service needs an adapter. |

## Known gaps

- Which SensorThings ObservedProperty URI corresponds to each domain element is not decided. The obvious source is the WMO Codes Registry or the CF standard names table.
- Which unit each element should use by default is not decided. The sources do not fix one.
- The NWS `qualityControl` flag definitions are not opened. See `docs/us-standards.md`.

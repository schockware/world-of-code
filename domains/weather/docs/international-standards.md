# International Standards for Weather Measurement and Communication

Sourcing date: 2026-09-29. Method: each URL was fetched (or, where marked "search only", surfaced by web search but not successfully opened). Editions and versions are as stated on the fetched pages. Anything not confirmed is listed in the last section.

Verification key: **Opened** = page fetched and content read. **Search only** = appeared in search results, page not opened or was JS-rendered/blocked.

## 1. WMO codes and observation practice

### 1.1 WMO-No. 306 Manual on Codes (Vol. I.1 alphanumeric, Vol. I.2 binary + common, Vol. I.3 IWXXM)
- Publisher: WMO. Annex II to the WMO Technical Regulations.
- Identifier/version: WMO-No. 306. Vol. I.2 = Part B (FM 92 GRIB ed. 2, FM 94 BUFR ed. 4) + Part C (common features, common code tables, TDCF regulations). The community page states a **2025 edition** of Vol. I.2 is in the e-Library. Vol. I.1 (alphanumeric, includes METAR/SPECI/TAF, Beaufort table 1100) is at https://library.wmo.int/records/item/35713-manual-on-codes-international-codes-volume-i-1 (search only).
- Verified URLs: https://community.wmo.int/about-manual-codes-volume-i2 (Opened); https://codes.wmo.int/ (Opened); machine-readable tables: https://github.com/wmo-im/BUFR4, https://github.com/wmo-im/GRIB2, https://github.com/wmo-im/CCT (listed on the community page; repos not opened).
- Openness: PDFs downloadable from WMO e-Library (licence text not confirmed, see follow-up). Code tables are machine-readable via the WMO Codes Registry (content negotiation; registry says Open Government Licence v3 unless otherwise stated) and GitHub.
- Contract use: authoritative element/parameter identities (BUFR descriptors, GRIB2 parameter tables, common code tables such as present weather, cloud genus, Beaufort). Contracts can carry code-table URIs (`https://codes.wmo.int/...`) as stable identifiers for enumerations.
- Relevance: **HIGH**

### 1.2 WMO Codes Registry
- Publisher: WMO. URL: https://codes.wmo.int/ (Opened).
- Registers listed: BUFR4, GRIB2, Common, WMDR, WIS, IWXXM, significant weather, technical regulations. Content negotiation (RDF etc.), Open Government Licence v3.
- Contract use: URI-addressable enumerations and vocabularies; use as `$id`/`x-` references for code lists instead of copying values.
- Relevance: **HIGH**

### 1.3 WMO-No. 8 Guide to Instruments and Methods of Observation (CIMO Guide)
- Publisher: WMO. Five volumes (I Meteorological variables, II Cryospheric, III Observing systems, IV Space-based, V QA/QM).
- Edition: Community page (Opened) says "2021/2018 edition"; a search of community.wmo.int pages says a **2024 edition** is now available, with 2023 provisional volumes also seen. Treat the current edition as unconfirmed (see follow-up).
- URLs: https://community.wmo.int/site/knowledge-hub/programmes-and-initiatives/instruments-and-methods-of-observation-programme-imop/guide-instruments-and-methods-of-observation-wmo-no-8 (Opened); library record https://library.wmo.int/records/item/68695-guide-to-instruments-and-methods-of-observation (search only).
- Openness: free PDF download; not machine-readable; multilingual (ar, zh, fr, es, ru).
- Contract use: normative definitions of measured quantities, averaging periods, reference heights, accuracy/uncertainty classes; informs field descriptions and valid ranges (e.g. 10-min mean wind at 10 m).
- Relevance: **HIGH** (semantic source, not a data format)

### 1.4 WMO-No. 49 Technical Regulations Vol. II / ICAO Annex 3 (aviation meteorology)
- Publisher: WMO (WMO-No. 49 Vol. II) and ICAO (Annex 3, "Meteorological Service for International Air Navigation").
- Status (Opened): https://community.wmo.int/site/knowledge-hub/programmes-and-initiatives/aviation/two-stage-discontinuation-of-technical-regulations-wmo-no-49-volume-ii-meteorological-service says Parts I and II of WMO-No. 49 Vol. II (the ICAO Annex 3 reproduction) were discontinued on 31 Dec 2023; use ICAO Annex 3 directly. Parts III and IV remain WMO-unique. ICAO store search result lists Annex 3 **21st Edition, August 2025** (search only).
- URLs: https://store.icao.int/en/annexes/annex-3 (search only); WMO codes register https://codes.wmo.int/49-2 (search only).
- Openness: ICAO Annex 3 is sold (paid). WMO-No. 49 is free.
- Contract use: the governing rules for METAR/SPECI/TAF content, units and reporting conventions (visibility, RVR, cloud amount/height, QNH), plus SIGMET etc.
- Relevance: **MED** (HIGH if aviation products are in scope)

### 1.5 METAR / SPECI / TAF formats
- Defined in WMO-No. 306 Vol. I.1 (FM 15 METAR, FM 16 SPECI, FM 51 TAF; per search result, search only) with content requirements from ICAO Annex 3. Not separate publications.
- Contract use: text ("TAC") forms are legacy; contracts should model the decoded semantic content and reference IWXXM (below) for XML.
- Relevance: **MED**

### 1.6 IWXXM (ICAO Meteorological Information Exchange Model)
- Publisher: WMO/ICAO; documented in WMO-No. 306 Vol. I.3. XML/GML schemas + Schematron; per-package versions after 3.0. Covers METAR/SPECI, TAF, SIGMET, AIRMET, TCA, VAA, space-weather advisories.
- URLs: https://github.com/wmo-im/iwxxm (search only); https://community.wmo.int/iwxxm (search only).
- Openness: schemas on GitHub; codelists in WMO Codes Registry.
- Contract use: field-level reference model for aviation reports; a JSON contract would map concepts, not reuse GML.
- Relevance: **MED**

### 1.7 Beaufort scale
- WMO-No. 306 Vol. I.1 code table 1100 (search result; not opened). WMO also has a library report "The Beaufort scale of wind force (technical and operational aspects)" (https://library.wmo.int/records/item/60281-..., page JS-rendered, not readable).
- Contract use: integer enumeration 0-12 with wind speed bands (10-min mean at 10 m). Use code table 1100 URI.
- Relevance: **LOW-MED**

### 1.8 International Cloud Atlas (WMO-No. 407) and present-weather code tables
- Publisher: WMO. Site: https://cloudatlas.wmo.int/ (search only; fetch failed with connection reset). Classifies clouds (genera, species, varieties) and hydrometeors, lithometeors, photometeors, electrometeors. Legal status under WMO Technical Regulations.
- Openness: free online and PDF (https://cloudatlas.wmo.int/docs/wmo_407_en-v2.pdf, search only). Not machine-readable; the corresponding coded values are in WMO-No. 306 code tables (present weather ww/W1W2, cloud genus).
- Contract use: cloud genus/species enumerations and present-weather definitions.
- Relevance: **MED**

## 2. Metadata and information system

### 2.1 WIGOS Metadata Standard (WMO-No. 1192) and WMDR
- Publisher: WMO. WMDR = WIGOS Metadata Representation (v1.0 per GitHub page, Opened). Standard published as WMO-No. 1192 (2019 edition; search result).
- URLs: https://github.com/wmo-im/wmdr (Opened; UML model, XSD, examples); model browser https://schemas.wmo.int/wmdr and code tables https://codes.wmo.int/wmdr (search only); https://library.wmo.int/records/item/55626-wigos-metadata-standard (JS page, not readable).
- Openness: PDF free; XSD/UML/code tables machine-readable (GitHub licence not confirmed).
- Contract use: station/observing-facility metadata model (WIGOS Station Identifier, station type, instrument, observed variable, reference height, sampling/averaging, uncertainty). Basis for a `Station` and `ObservationMetadata` schema.
- Relevance: **HIGH**

### 2.2 WCMP2 (WMO Core Metadata Profile, version 2)
- Publisher: WMO. Version **2.3.0, STABLE, 2026-06-15**; copyright WMO 2024-2026 (Opened: https://wmo-im.github.io/wcmp2/standard/wcmp2-STABLE.html).
- Defines discovery metadata as GeoJSON extending OGC API - Records; data policy core/recommended. Normative JSON Schema: https://schemas.wmo.int/wcmp/2.0.0/schemas/wcmp2-bundled.json (URL as stated on page; schema file itself not fetched).
- Openness: freely viewable; JSON Schema machine-readable.
- Contract use: JSON Schema is directly reusable for dataset/collection descriptions.
- Relevance: **HIGH**

### 2.3 WIS2 (WMO Information System 2.0) and Notification Message
- Publisher: WMO. Operational since 1 Jan 2025; MQTT pub/sub plus HTTP; Global Broker, Discovery Catalogue, Cache, Monitor (Opened: https://community.wmo.int/site/knowledge-hub/programmes-and-initiatives/wmo-information-system-wis/wis2-overview). Governing manual: WMO-No. 1060 (Vol. II, Appendix E, per search result).
- **WIS2 Notification Message Encoding** v**1.3.0 STABLE**, 2026-06-15, copyright WMO (Opened: https://wmo-im.github.io/wis2-notification-message/standard/wis2-notification-message-STABLE.html). GeoJSON Feature; required `id` (UUID), `type`=Feature, `conformsTo` or `version`, `geometry` (Point/Polygon/null), `properties.pubtime` (RFC 3339 UTC), `properties.data_id`, `links[]` (href, rel); max 8192 bytes.
- Related (search only): WIS2 Topic Hierarchy https://wmo-im.github.io/wis2-topic-hierarchy/standard/wis2-topic-hierarchy-STABLE.html; Cookbook https://wmo-im.github.io/wis2-cookbook/cookbook/latest/wis2-cookbook-STABLE.html.
- Contract use: message envelope, metadata, and topic naming for any event-style publication.
- Relevance: **HIGH**

## 3. OGC and ISO geospatial/observation standards

### 3.1 OGC API - Environmental Data Retrieval (EDR)
- Publisher: OGC. OGC 19-086r6, **v1.1**, published 2023-07-27 (Opened: https://docs.ogc.org/is/19-086r6/19-086r6.html). Query types: position, radius, area, cube, trajectory, corridor, items, locations, instances. Conformance class is to **OpenAPI 3.0** (not 3.1; the standard allows other API definition languages).
- Openness: royalty-free; OpenAPI fragments on GitHub https://github.com/opengeospatial/ogcapi-environmental-data-retrieval (search only).
- Contract use: closest fit for a weather query API; CoverageJSON is its typical payload. Adapt to 3.1 in our contracts and note the deviation.
- Relevance: **HIGH**

### 3.2 OGC SensorThings API
- OGC 18-088 (Part 1 Sensing **v1.1**, 2021-08-04; Opened: https://docs.ogc.org/is/18-088/18-088.html). Part 2 Tasking Core v1.0 (17-079r1), STAplus 1.0 (22-022r1), WebSub extension 1.0 (24-032r1) (Opened: https://www.ogc.org/standards/sensorthings/).
- Entities: Thing, Location, HistoricalLocation, Datastream, Sensor, ObservedProperty, Observation, FeatureOfInterest, aligned with O&M. Royalty-free licence.
- Contract use: entity model and JSON encoding for observation streams; ObservedProperty links to vocabulary URIs; `unitOfMeasurement` (name, symbol, definition).
- Relevance: **HIGH**

### 3.3 Observations, Measurements and Samples (OGC 20-082r4 = ISO 19156:2023)
- Publisher: OGC + ISO/TC 211. Abstract Specification Topic 20, **v3.0**, ISO 19156:2023 edition 2 (Opened: https://www.ogc.org/standards/om/). Supersedes ISO 19156:2011 / O&M 2.0 (XML schemas at http://schemas.opengis.net/om/2.0/ remain valid for 2.0).
- Openness: OGC document free; ISO text paid (CHF 227 per search result; iso.org page returned 403 to fetch; https://www.iso.org/standard/82463.html search only).
- Contract use: conceptual model (Observation: featureOfInterest, observedProperty, procedure, phenomenonTime, resultTime, result).
- Relevance: **HIGH** (conceptual)

### 3.4 WaterML 2.0
- OGC 10-126r4 (Part 1 Timeseries), v2.0.1; Parts 2-4 v1.0 (Opened: https://www.ogc.org/standards/waterml/). Based on O&M; hydrology timeseries, ratings, groundwater.
- Contract use: timeseries encoding patterns (interpolation type, quality codes, aggregation) useful for precipitation/streamflow; XML-centric.
- Relevance: **LOW-MED**

## 4. Semantics, units and time

### 4.1 CF Conventions and Standard Name Table
- Publisher: CF community (cfconventions.org). Conventions **v1.13**; Standard Name Table **v95, 2026-09-16** (Opened: https://cfconventions.org/, https://cfconventions.org/Data/cf-standard-names/current/build/cf-standard-name-table.html). Site under CC0. Example: `air_temperature`, canonical unit K. CF units follow UDUNITS, not UCUM (canonical units are given in UDUNITS syntax).
- Openness: free; standard name table published as XML (format inferred, not opened).
- Contract use: `standard_name` field per variable; canonical unit for validation.
- Relevance: **HIGH**

### 4.2 UCUM
- Publisher: Regenstrief Institute (ucum.org). **v2.2, 2024-06-17** (Opened: https://ucum.org/ucum). Case-sensitive ASCII unit codes (`Cel`, `K`, `hPa`, `m/s`, `%`, `mm`).
- Openness: free; machine-readable `ucum-essence.xml` exists per project docs but my fetch returned an HTML rendering, so its format is unconfirmed.
- Contract use: `unit` field values. Note UCUM has no dedicated code for "octa"/"okta" (needs annotation `{okta}`), and `hPa` fine, `dBZ` needs `dB{Z}`-style annotation. Verify each unit with a UCUM validator.
- Relevance: **HIGH**

### 4.3 QUDT
- Publisher: QUDT.org (501(c)(3)). Catalog release 2.1 (page updated 2026-09-15). Ontologies in RDF/Turtle/SHACL for units, quantity kinds, dimension vectors; CC BY 4.0 (Opened: https://www.qudt.org/).
- Contract use: optional `quantityKind`/`unit` IRIs for semantic linking; carries UCUM codes as properties.
- Relevance: **MED**

### 4.4 ISO 8601 (Date and time)
- Publisher: ISO. ISO 8601-1:2019 (basic rules) and 8601-2:2019 (extensions), search-confirmed edition 1, Feb 2019 (https://www.iso.org/standard/70907.html; fetch returned 403, so treat as search only).
- Openness: paid. Free practical profile: RFC 3339 (used by WIS2 `pubtime`) and JSON Schema `format: date-time`.
- Contract use: `phenomenonTime`, intervals (`start/end`), durations (`PT10M`), UTC `Z` suffix. Recommend RFC 3339 profile of ISO 8601.
- Relevance: **HIGH**

### 4.5 Other ISO/IEC standards (not individually fetched)
- ISO 19115 (metadata) and ISO 19107/19111 (geometry/CRS), ISO 80000-1 / SI (units), ISO 3166 (country codes), ISO 639 (languages), ISO 19157 (data quality) are relevant background. **Not verified in this pass**; see follow-up.
- Relevance: **LOW-MED**

## 5. Warnings

### 5.1 OASIS Common Alerting Protocol (CAP) v1.2 and WMO CAP practice
- Publisher: OASIS. **CAP v1.2, OASIS Standard, 2010-07-01** (Opened: https://docs.oasis-open.org/emergency/cap/v1.2/CAP-v1.2-os.html). XML; elements `alert`, `info`, `area`, `resource`; namespace `urn:oasis:names:tc:emergency:cap:1.2`; urgency/severity/certainty enumerations. OASIS IPR policy; free to implement.
- WMO layer: WMO Register of Alerting Authorities https://alertingauthority.wmo.int/ (Opened; RSS/Atom feeds for updates). WMO CAP pages: https://community.wmo.int/site/knowledge-hub/programmes-and-initiatives/public-weather-services-programme-pws/common-alerting-protocol (search only). "WMO Global Weather Alerts / Alert Hub" implementation details were **not opened**.
- Contract use: JSON mapping of CAP alert (status, msgType, scope, category, event, urgency, severity, certainty, effective/expires, areaDesc, polygon, geocode). CAP has no official JSON binding; define ours explicitly and keep CAP field names.
- Relevance: **HIGH**

## 6. Not verified / needs follow-up
1. WMO e-Library record pages (`library.wmo.int/records/...`) are JS-rendered and returned no content; edition years, licences and download links for WMO-No. 306, 8, 1192, 49 are from search snippets and community pages, not the records. Licence terms (likely CC BY-NC-SA / free download) not confirmed.
2. WMO-No. 8: current edition conflicting (community page "2021/2018"; search results "2024 edition" and "2023 provisional"). Confirm on the library.
3. WMO-No. 306 Vol. I.1 edition/year, exact FM 15/16/51 clause numbers, and Beaufort table 1100 content were not opened directly.
4. ICAO Annex 3: edition claim (21st ed., Aug 2025) is search-only. Paid; licensing for reuse of text not checked.
5. ISO 8601 and ISO 19156 pages returned 403 to fetch; edition data is from search results only.
6. WMDR licence, current WMDR version (1.0 only per GitHub page) and any WMDR 2.0 status.
7. UCUM `ucum-essence.xml` format/version; CF standard-name XML file location; UDUNITS-vs-UCUM mapping gaps.
8. WMO Alert Hub / Global Weather Alerts (as an actual service) and any WIS2 CAP topic conventions; WMO-endorsed CAP profile.
9. WIS2 Topic Hierarchy, Cookbook, GDC and WMO-No. 1060 were seen only in search results.
10. IWXXM current package versions and GitHub licence; OGC EDR GitHub OpenAPI files; OGC API - Records and CoverageJSON (not investigated).
11. Other ISO/IEC standards in 4.5; also not covered: WMO-No. 305/GRIB2 templates for radar/satellite, WMO Unified Data Policy (Res. 1 Cg-Ext 2021), OSCAR/Surface.

## 7. Summary table

| # | Standard | Publisher | Version/ID (as verified) | Open? / machine-readable | Verified | Priority |
|---|----------|-----------|--------------------------|--------------------------|----------|----------|
| 1 | Manual on Codes (BUFR/CREX/GRIB, code tables) | WMO | WMO-No. 306; Vol. I.2 2025 ed. | PDF free; tables via registry/GitHub | Opened (community page) | High |
| 2 | WMO Codes Registry | WMO | codes.wmo.int | OGL v3; RDF etc. | Opened | High |
| 3 | CIMO Guide | WMO | WMO-No. 8; 2021 or 2024 ed. (unconfirmed) | PDF free | Opened (partial) | High |
| 4 | Aviation regs / Annex 3 | WMO / ICAO | WMO-No. 49 Vol. II; Annex 3 21st ed. | ICAO paid | Opened (WMO), search (ICAO) | Med |
| 5 | METAR/SPECI/TAF | WMO / ICAO | FM 15/16/51 in 306 I.1 | in 306 | Search only | Med |
| 6 | IWXXM | WMO / ICAO | WMO-No. 306 Vol. I.3 | XSD on GitHub | Search only | Med |
| 7 | Beaufort scale | WMO | Code table 1100 | in 306 | Search only | Low-Med |
| 8 | International Cloud Atlas | WMO | WMO-No. 407 | Free; not machine-readable | Search only | Med |
| 9 | WIGOS Metadata Standard / WMDR | WMO | WMO-No. 1192; WMDR 1.0 | PDF free; XSD/UML | Opened (GitHub) | High |
| 10 | WCMP2 | WMO | 2.3.0 STABLE (2026-06-15) | Free; JSON Schema | Opened | High |
| 11 | WIS2 + Notification Message | WMO | Msg 1.3.0 STABLE (2026-06-15) | Free; GeoJSON | Opened | High |
| 12 | OGC API - EDR | OGC | 19-086r6 v1.1 (2023) | Royalty-free; OpenAPI 3.0 | Opened | High |
| 13 | SensorThings API | OGC | 18-088 v1.1 (2021) | Royalty-free | Opened | High |
| 14 | O&M / ISO 19156 | OGC / ISO | 20-082r4 v3.0 = ISO 19156:2023 | OGC free; ISO paid | Opened (OGC) | High |
| 15 | WaterML 2.0 | OGC | 10-126r4 v2.0.1 | Free; XML | Opened | Low-Med |
| 16 | CF Conventions + Std Names | CF community | 1.13; table v95 (2026-09-16) | CC0 | Opened | High |
| 17 | UCUM | Regenstrief | 2.2 (2024-06-17) | Free | Opened | High |
| 18 | QUDT | QUDT.org | Catalog 2.1 | CC BY 4.0; RDF/SHACL | Opened | Med |
| 19 | ISO 8601 | ISO | 8601-1:2019 | Paid (RFC 3339 free) | Search only | High |
| 20 | OASIS CAP | OASIS | v1.2 (2010-07-01) | Free; XML | Opened | High |
| 21 | WMO Register of Alerting Authorities | WMO | n/a | Feeds RSS/Atom | Opened | Med |

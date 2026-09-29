# US Standards and Specifications for Weather Measurement and Communication

Research date: 2026-09-29. "Verified" means the URL was opened (fetched) in this session and the stated facts were read from it. Fetch summaries were produced by a small model, so numeric details that matter to a contract should be re-checked against the primary document before being frozen. Priority = relevance to our OpenAPI 3.1 / JSON Schema / UCUM contracts.

General note on openness: works of US Federal Government employees are public domain in the US (17 USC 105); NOAA/NWS pages state their data are open. Exceptions are third-party standards (OASIS, ICAO, WMO).

---

## 1. NWS API and NWS products

### 1.1 weather.gov API (api.weather.gov) OpenAPI spec
- Publisher: NWS / NOAA. Identifier: OpenAPI 3.1.2 document, `info.title` "weather.gov API", `info.version` 3.11.0 (as served on 2026-09-29). Single server `https://api.weather.gov`.
- URL (verified): https://api.weather.gov/openapi.json
- Openness: machine-readable, open data, no fee, User-Agent header required, rate limited (retry after ~5 s).
- Defines: paths for `/alerts*`, `/points/{lat},{lon}`, `/gridpoints/{wfo}/{x},{y}[/forecast|/forecast/hourly|/stations]`, `/stations/{id}/observations[/latest]`, `/stations/{id}/tafs`, `/aviation/sigmets`, `/aviation/cwsus/...`, `/glossary`, `/icons`, `/products`, etc. Also `GridpointForecastUnits` enum ("us" | "si") for forecast unit selection. Direct template for our own OpenAPI paths and schema names.
- Contract relevance: HIGH.

### 1.2 NWS API formats: GeoJSON and JSON-LD; units convention
- Publisher: NWS. URL (verified): https://www.weather.gov/documentation/services-web-api
- Formats via Accept header: GeoJSON (default, because of geometry), JSON-LD ("based upon JSON-LD to promote machine data discovery"), DWML, OXML, CAP (alerts), Atom. `@context` appears throughout the schema (array of URIs/objects, or single object).
- Units (verified in openapi.json, schema `QuantitativeValue`): properties `value`, `unitCode`, `qualityControl`; unitCode is a WMO or UCUM code, either `wmoUnit:degC`-style prefixed WMO codes or plain UCUM expressions. The docs page itself does not specify unit conventions; the OpenAPI spec is the authority. Forecast endpoints accept units us/si. (Which endpoints use SI vs US units per field was not checked.)
- Contract relevance: HIGH. Our `Quantity {value, unit(UCUM), qualityControl}` should be shape-compatible; map `wmoUnit:` to UCUM.

### 1.3 NWS Alerts web service (CAP-based)
- Publisher: NWS. URL (verified): https://www.weather.gov/documentation/services-web-alerts
- Alerts delivered as JSON-LD (primary) or CAP v1.2 XML, Atom for indexes; pull (no faster than every 30 s), or push via NOAAPORT/NWWS, and FEMA IPAWS. Filters by state, county, zone, marine area, point. Statement: NWS CAP must not be used as input to EAS encoders (use NOAA Weather Radio).
- Contract relevance: HIGH (Alert schema: severity/urgency/certainty, event, geocode UGC/SAME, VTEC).

### 1.4 NWS Directives (NWSI/NWSPD), Operations and Services 10-series
- Publisher: NWS. Index (verified): https://www.weather.gov/directives/010 ; category page https://www.weather.gov/directives/ . Public domain PDFs, not machine-readable.
- NWSI 10-813 "Terminal Aerodrome Forecasts", 30 Oct 2024 (PDF opened and read; supersedes 18 Nov 2020 version): https://www.weather.gov/media/directives/010_pdfs/pd01008013curr.pdf . TAF follows ICAO Annex 3 / WMO No. 49 with US-specific practice; TAFs issued in TAC and ICAO IWXXM. Note: 10-813 is the TAF directive; it is not the METAR directive.
- NWSI 10-1701 "Text Product Formats and Codes", 29 Oct 2019 (PDF opened): https://www.weather.gov/media/directives/010_pdfs/pd01017001curr.pdf . WMO headers / AWIPS IDs / format rules for text products.
- Listed on the 10-series index page (titles from the index fetch, PDFs not individually opened): 10-1702 Universal Geographic Code (UGC); 10-503 WFO Public Weather Forecast Products Specification; 10-511 WFO Severe Weather Products Specification; 10-601 WFO Tropical Cyclone Products; 10-1302 Requirements and Standards for NWS Climate Observations; 10-1004 Climate Records; 10-922 WFO Water Resources Products Specification. Storm Data event types are governed by NWSI 10-1605 (cited inside the Storm Events documentation, section 3.2).
- Contract relevance: 10-1701 HIGH (product identifiers, headers), 10-1702 HIGH (zone/county codes), 10-813 MED, 10-503/511/601 MED, others LOW.

---

## 2. Federal Meteorological Handbooks (OFCM / ICAMS)

- Publisher: Office of the Federal Coordinator for Meteorology (OFCM), now hosted by ICAMS. Index (verified via fetch): https://www.icams-portal.gov/resources/ofcm/fmh/allfmh2.htm . PDFs public domain; not machine-readable.
- Volumes listed there: FMH-1 Surface Weather Observations and Reports (FCM-H1-2019, July 2019); FMH-2 Surface Synoptic Codes (FCM-H2-1988, last change 2005); FMH-3 Rawinsonde and Pibal Observations (FCM-H3-1997, chg 2006); FMH-11 Doppler Radar Meteorological Observations (WSR-88D, parts A-D, dates 2005-2021); FMH-12 US Meteorological Codes and Coding Practices (FMH-12-2019, chg Oct 2019); FMH-13 (FCM-H13-2021) US Federal Meteorological Data Management Handbook (Dec 2021).
- FMH-1: PDF for the 2019 edition https://www.icams-portal.gov/resources/ofcm/fmh/FMH1/fmh1_2019.pdf returned HTTP 403 for automated fetch (existence confirmed by the index listing and search). I read an older mirror (FCM-H1-1995, https://marrella.aos.wisc.edu/aos452/fmh1.pdf) which confirms US conventions such as visibility in statute miles and a remarks section; treat 2019 details as needing follow-up. Defines observing elements, reporting resolution, METAR/SPECI criteria. Contract relevance: HIGH for observation element definitions and resolution/units.
- FMH-12: US METAR/TAF/code practices including where US differs from WMO. Search-confirmed URL https://www.icams-portal.gov/resources/ofcm/fmh/FMH12/fmh12.pdf (not opened). Relevance: HIGH/MED.
- FMH-11 (radar) MED; FMH-2, FMH-3 LOW; FMH-13 (data management) MED.

---

## 3. Aviation: FAA AIM, US METAR/TAF vs ICAO

- Aviation Weather Center (NWS) operates aviationweather.gov; verified reachable. Data API https://aviationweather.gov/data/api/ (verified): endpoints e.g. `/api/data/metar`, TAF, PIREP, SIGMET, G-AIRMET; formats raw, JSON, GeoJSON, XML, CSV, IWXXM; documented by an OpenAPI spec; 100 requests/minute; ~30 days history; open. Contract relevance: HIGH (JSON shape for METAR/TAF).
- FAA AIM Chapter 7 Section 1 (https://www.faa.gov/air_traffic/publications/atpubs/aim_html/chap7_section_1.html): HTTP 403, NOT verified. See follow-up.
- US vs ICAO differences: NWSI 10-813 (read) confirms US TAF follows ICAO Annex 3 with domestic practice (verified only in general terms). The specific differences (visibility in statute miles, altimeter in inHg with A prefix vs QNH hPa, wind in knots, temp in whole C, RMK section with AO1/AO2, SLP, T-group) are widely documented but were not confirmed from an opened source apart from FMH-1 (1995) mentioning statute miles and remarks; treat as needing follow-up.
- Contract relevance: HIGH for METAR/TAF schema; decode fields with unit tags (`[mi_i]` vs `m`, `[in_i'Hg]` vs `hPa`).

---

## 4. NOAA NCEI data formats

- GHCN-Daily. Publisher NCEI. Version 3.34 (as read). URL (verified): https://www.ncei.noaa.gov/pub/data/ghcn/daily/readme.txt . Fixed-width `.dly` per station: ID (cols 1-11), year, month, element, then 31 x (value, measurement flag, quality flag, source flag). PRCP in tenths of mm, TMAX/TMIN in tenths of degC, SNOW/SNWD in mm. Public domain, machine readable, citation requested (Menne et al. 2012, JAOT 29:897-910). Relevance: MED (daily climate observation record + flag model).
- ISD (Integrated Surface Database). URL (verified): https://www.ncei.noaa.gov/products/land-based-station/integrated-surface-database . Full ISD in common ASCII format; ISD-Lite (8 parameters, fixed-width); Global Summary of the Day CSV. Version 2 QC documented (ish-qc.pdf). Units not stated on the page; the format document (isd-format-document.pdf) was not opened. Open, no explicit license text. Relevance: MED.
- Storm Events Database. URL (verified): https://www.ncei.noaa.gov/stormevents/ (only a thin summary was retrieved). Bulk CSV format document (opened and text-extracted): https://www.ncei.noaa.gov/pub/data/swdi/stormevents/csvfiles/Storm-Data-Bulk-csv-Format.pdf . Three linked files (details, locations, fatalities) via `event_id`; `episode_id`; date fields as YYYYMM / DD / hhmm; `event_type` restricted to NWSI 10-1605 list; `damage_property`/`damage_crops` encoded as strings like 10.00K, 10.00M. Public domain. Relevance: MED (event taxonomy, damage encoding; the 48 event type count is from the page summary, not checked against 10-1605).

---

## 5. NOAA NDBC buoy and C-MAN

- Publisher: NDBC/NWS. URLs (verified): https://www.ndbc.noaa.gov/faq/rt_data_access.shtml and https://www.ndbc.noaa.gov/faq/measdes.shtml .
- Realtime files (`/data/realtime2/{station}.{ext}`): `.txt` standard met, `.spec`, `.drift`, `.cwind`, `.data_spec`, `.ocean`, `.tide`, `.swdir`, `.swr1`, `.adcp`, etc.; last 45 days; whitespace-delimited text. Metric units (WSPD m/s, WVHT m, temps degC, pressure hPa - metric confirmed by page, specific unit per column should be checked); missing value is `MM` in realtime and 99/999 style in historical; times UTC. Public, open, not JSON.
- Contract relevance: MED (marine observation elements, sentinel handling).

---

## 6. Radar: NEXRAD (WSR-88D)

- Publisher: NOAA NWS Radar Operations Center (ROC) / NCEI. URLs (verified): https://www.ncei.noaa.gov/products/radar/next-generation-weather-radar ; https://www.roc.noaa.gov/interface-control-documents.php .
- Level II base data: reflectivity, radial velocity, spectrum width plus dual-pol (ZDR, CC, PhiDP). Level III: 75+ derived products. Formats are binary, defined by ICDs: Archive II/User ICD 2620010 (Rev J, Build 23.0, 25 Jun 2024; Rev K draft), Product Specification ICD 2620003 (Rev AE, Build 24.0, 19 Aug 2025; Rev AF draft). Also 2620001, 2620002, 2620007. Free; cloud-hosted via NOAA Open Data Dissemination.
- Contract relevance: LOW for base schemas (binary, out of JSON contract scope); MED for metadata (radar site, product code, scan time) only.

---

## 7. Tropical cyclone products (NHC)

- Publisher: NHC/NWS. GIS products URL (verified): https://www.nhc.noaa.gov/gis/ : forecast track/cone/watches-warnings (shapefile, KMZ), wind field and radii, best track, wind speed probabilities, storm surge (GRIB2, KML), Graphical Outlook (shapefile), RSS/XML feeds by basin. Open.
- Text advisories (Public Advisory, Forecast/Advisory, Discussion) are presumably governed by NWSI 10-601 (WFO tropical products; NHC-specific directive not identified); the NHC product description page (https://www.nhc.noaa.gov/aboutnhcprod.shtml) failed to load (connection reset), so advisory text format is NOT verified.
- Relevance: MED (storm identifier ATCF style, position, intensity in kt, pressure, radii in nm).

### 7.1 Saffir-Simpson Hurricane Wind Scale
- Publisher NHC, "Updated May 2021" (PDF opened, text extracted): https://www.nhc.noaa.gov/pdf/sshws.pdf . Category is determined by peak 1-minute sustained surface wind at 10 m. Cat 1: 74-95 mph (64-82 kt; 119-153 km/h). Cat 2: 96-110 (83-95 kt; 154-177). Cat 3: 111-129 (96-112 kt; 178-208). Cat 4: 130-156 (113-136 kt; 209-251). Cat 5: the extracted text reads "156 mph or higher, 136 kt or higher, 251 km/h or higher", which contradicts the usual 157 / 137 / 252 (ranges above are contiguous, so 157+ is expected); likely a PDF extraction/typo issue. Follow up before freezing the Cat 4/5 boundary. Summary page https://www.nhc.noaa.gov/aboutsshws.php confirmed Cat 1-3 thresholds.
- Contract relevance: HIGH (enum + integer wind thresholds; note wind averaging period = 1 min, distinct from ICAO 10-min).

---

## 8. Severe weather categories

- SPC Convective Outlook categories. URL (verified): https://www.spc.noaa.gov/misc/about.html . Categories: TSTM (general thunder, >=10% thunderstorm probability), MRGL (1), SLGT (2), ENH (3), MDT (4), HIGH (5). Formats (GeoJSON/KML/shapefile) not confirmed from this page. Relevance: HIGH (small enum, risk level 0-5 mapping).
- Enhanced Fujita Scale. URL (verified): https://www.weather.gov/oun/efscale . Operational 1 Feb 2007. 3-second gust estimates (mph): EF0 65-85, EF1 86-110, EF2 111-135, EF3 136-165, EF4 166-200, EF5 over 200. NWS has exclusive authority for official tornado ratings; values are estimates from damage, not measurements. Relevance: HIGH.

---

## 9. Derived indices and health scales

- Heat Index. Publisher NWS. URL (verified): https://www.weather.gov/safety/heat-index . Combines air temperature and relative humidity; applies at 80 F and above; example 96 F at 65% RH gives 121 F; assumes shade and light wind, direct sun can add up to 15 F. The page mentions related WBGT and HeatRisk tools. The Rothfusz regression and the four risk categories were not on the fetched page (NOT verified). Relevance: HIGH.
- Wind Chill. URL (verified): https://www.weather.gov/safety/cold-wind-chill-chart . 2001 formula: WC(F) = 35.74 + 0.6215 T - 35.75 V^0.16 + 0.4275 T V^0.16 (T in F, V in mph); valid for T at or below 50 F and V above 3 mph; applies to living organisms only. Relevance: HIGH.
- UV Index. EPA page https://www.epa.gov/sunsafety/uv-index-scale-0 (verified): US scale follows WHO international guidelines; page bands are grouped Low 1-2, Moderate to High 3-7, Very High to Extreme 8+ (page-level grouping; the finer 0-2 Low, 3-5 Moderate, 6-7 High, 8-10 Very High, 11+ Extreme breakdown was not on the page, NOT verified). NWS/EPA publisher of the forecast. Relevance: MED.
- Air Quality Index. Publisher EPA / AirNow. URL (verified): https://www.airnow.gov/aqi/aqi-basics/ . Categories: Good 0-50, Moderate 51-100, USG 101-150, Unhealthy 151-200, Very Unhealthy 201-300, Hazardous 301+ (page shows 301+; 0-500 scale and technical assistance document not confirmed). Pollutants: O3, PM, CO, SO2, NO2. AirNow API https://docs.airnowapi.org/ (verified): free account/API key needed; data flagged preliminary and not for regulatory/decision use. Relevance: MED (dedicated AQI schema; category enum + colors).

---

## 10. Alerts: CAP v1.2 and IPAWS

- OASIS Common Alerting Protocol v1.2, OASIS Standard, 01 July 2010. Publisher OASIS Emergency Management TC. URL (verified): https://docs.oasis-open.org/emergency/cap/v1.2/CAP-v1.2-os.html . Namespace `urn:oasis:names:tc:emergency:cap:1.2`. Elements verified: alert (identifier, sender, sent, status, msgType, source, scope, restriction, addresses, code, note, references, incidents); info (language, category, event, responseType, urgency, severity, certainty, audience, eventCode, effective, onset, expires, senderName, headline, description, instruction, web, contact, parameter); area (areaDesc, polygon, circle, geocode, altitude, ceiling); resource (resourceDesc, mimeType, size, uri, derefUri, digest). Copyright OASIS; free to copy with attribution, XML schema provided. Relevance: HIGH (enums for status/msgType/urgency/severity/certainty; NWS JSON alerts mirror these).
- IPAWS: FEMA adopts "OASIS CAP v1.2 IPAWS Profile Version 1.0". URL (verified): https://www.fema.gov/emergency-managers/practitioners/integrated-public-alert-warning-system/technology-developers/common-alerting-protocol . Profile document itself not opened. Relevance: MED.

---

## 11. US customary units in weather (conventions)

No single authoritative source was found and opened for this; the following is drawn from documents that were read: NWS forecast API `units=us|si` (openapi.json); heat index/wind chill formulas in F and mph (weather.gov); SSHWS in mph/kt/km/h; EF scale in mph; SPC/NWS text products; FMH-1 (1995) statute miles for visibility. Common convention (from general knowledge, NOT verified from an opened source): temperature F, wind kt in aviation/marine and mph in public products, pressure inHg (altimeter) and mb/hPa (SLP), precipitation and snow in inches, visibility in statute miles, distance in nautical miles for marine/tropical. UCUM mapping: `[degF]`, `[mi_i]`, `[nmi_i]`, `[in_i]`, `[in_i'Hg]`, `[kn_i]`, `[mph]` (`[mi_i]/h`). UCUM itself is publisher Regenstrief Institute (https://ucum.org), not fetched here. Relevance: HIGH.

---

## Not verified / needs follow-up

1. FMH-1 2019 edition (HTTP 403 on icams-portal.gov PDF); FMH-12, FMH-11, FMH-13 PDFs not opened. Confirm METAR/SPECI criteria and reporting resolution in the 2019 text.
2. FAA AIM Chapter 7 (HTTP 403) and FAA's own METAR/TAF guidance; US-vs-ICAO METAR/TAF difference list (statute miles, inHg, remarks) needs a primary source (FMH-1/12, ICAO Annex 3 is paid).
3. NWSI for METAR/ASOS observations: could not identify the exact NWSI number; 10-813 is TAF, not METAR. Check ASOS User's Guide and NWSI 10-1302 / FMH-1.
4. NHC advisory text formats (aboutnhcprod.shtml unreachable) and their governing directive (10-601 title as returned by the index page only).
5. NWS directives 10-503, 10-511, 10-601, 10-1702, 10-1302, 10-1004, 10-922, 10-1605 titles came only from the index-page summary; PDFs not opened.
6. Saffir-Simpson Cat 5 lower bound (PDF text says 156 mph / 136 kt / 251 km/h; expected 157/137/252).
7. Heat Index categories and Rothfusz regression; UV index five-band breakdown; AQI 0-500 range and pollutant breakpoint tables (EPA AQI Technical Assistance Document).
8. ISD format document (isd-format-document.pdf) fields and units; NDBC per-column units and missing-value sentinels; Storm Events 48 event type list.
9. SPC GeoJSON/KML/shapefile output formats and URLs; the NWS API JSON-LD context URL value; whether `qualityControl` enum values (e.g. Z, C, S, V, X, Q, G, B, T) are defined in openapi.json.
10. IPAWS CAP profile document itself; UCUM specification; US customary unit conventions (section 11) beyond what is cited.

---

## Summary table

| # | Name | Publisher | Identifier / version | Verified URL | Open / machine-readable | Priority |
|---|------|-----------|----------------------|--------------|-------------------------|----------|
| 1.1 | weather.gov API OpenAPI | NWS | OpenAPI 3.1.2, API 3.11.0 | api.weather.gov/openapi.json | Open, JSON | High |
| 1.2 | NWS API formats/units | NWS | GeoJSON, JSON-LD, QuantitativeValue (WMO/UCUM) | weather.gov/documentation/services-web-api | Open | High |
| 1.3 | NWS Alerts service | NWS | CAP 1.2 / JSON-LD | weather.gov/documentation/services-web-alerts | Open | High |
| 1.4 | NWSI 10-1701 Text Product Formats | NWS | 29 Oct 2019 | weather.gov/media/directives/010_pdfs/pd01017001curr.pdf | PD, PDF | High |
| 1.4 | NWSI 10-813 TAF | NWS | 30 Oct 2024 | weather.gov/media/directives/010_pdfs/pd01008013curr.pdf | PD, PDF | Med |
| 1.4 | Other 10-series (1702, 503, 511, 601, 1302, 1004, 922) | NWS | index only | weather.gov/directives/010 | PD, PDF | Med/Low |
| 2 | FMH-1 Surface Observations | OFCM/ICAMS | FCM-H1-2019 (2019 PDF not opened) | icams-portal.gov/resources/ofcm/fmh/allfmh2.htm | PD, PDF | High |
| 2 | FMH-2/3/11/12/13 | OFCM/ICAMS | various | icams-portal.gov/resources/ofcm/fmh/allfmh2.htm | PD, PDF | Med/Low |
| 3 | AWC Data API (METAR/TAF) | NWS AWC | OpenAPI-described | aviationweather.gov/data/api/ | Open, JSON/GeoJSON/XML | High |
| 3 | FAA AIM Ch.7 | FAA | not verified | (403) | - | Follow-up |
| 4 | GHCN-Daily | NCEI | v3.34 | ncei.noaa.gov/pub/data/ghcn/daily/readme.txt | Open, fixed-width | Med |
| 4 | ISD / ISD-Lite | NCEI | v2 | ncei.noaa.gov/products/land-based-station/integrated-surface-database | Open, ASCII | Med |
| 4 | Storm Events | NCEI/NWS | bulk CSV v1.0 | ncei.noaa.gov/pub/data/swdi/stormevents/csvfiles/Storm-Data-Bulk-csv-Format.pdf | PD, CSV | Med |
| 5 | NDBC realtime/C-MAN | NDBC | realtime2 formats | ndbc.noaa.gov/faq/rt_data_access.shtml | Open, text | Med |
| 6 | NEXRAD Level II/III | ROC/NCEI | ICD 2620010J, 2620003AE | roc.noaa.gov/interface-control-documents.php | Open, binary | Low/Med |
| 7 | NHC GIS products | NHC | n/a | nhc.noaa.gov/gis/ | Open, shp/KMZ/GRIB2 | Med |
| 7.1 | Saffir-Simpson scale | NHC | updated May 2021 | nhc.noaa.gov/pdf/sshws.pdf | PD | High |
| 8 | SPC convective outlook categories | SPC | n/a | spc.noaa.gov/misc/about.html | PD | High |
| 8 | Enhanced Fujita scale | NWS | operational 1 Feb 2007 | weather.gov/oun/efscale | PD | High |
| 9 | Heat Index | NWS | n/a | weather.gov/safety/heat-index | PD | High |
| 9 | Wind Chill (2001) | NWS | n/a | weather.gov/safety/cold-wind-chill-chart | PD | High |
| 9 | UV Index | EPA / WHO | n/a | epa.gov/sunsafety/uv-index-scale-0 | PD | Med |
| 9 | AQI / AirNow API | EPA | n/a | airnow.gov/aqi/aqi-basics/ ; docs.airnowapi.org | PD; API key | Med |
| 10 | OASIS CAP v1.2 | OASIS | OS, 1 Jul 2010 | docs.oasis-open.org/emergency/cap/v1.2/CAP-v1.2-os.html | Free w/ attribution, XSD | High |
| 10 | IPAWS CAP v1.2 Profile v1.0 | FEMA | v1.0 | fema.gov/.../common-alerting-protocol | Public | Med |
| 11 | US customary unit conventions | (various) | n/a | none opened | - | High (unverified) |

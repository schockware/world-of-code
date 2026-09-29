//! Runs the conformance vectors shared by every nation. Passing them is the scoreboard.
//!
//! Every vector is run and every failure is collected with its file and vector id, so one
//! bad vector does not hide the others. The test fails with the whole list.

use std::fs;
use std::path::PathBuf;

use serde_json::Value;
use weather::ucum::Validator;
use weather::{Element, Observation, ParseError, Quantity, UnitValidator};

fn vector_dir() -> PathBuf {
    PathBuf::from(env!("CARGO_MANIFEST_DIR"))
        .join("../../../domains/weather/specs/core/conformance")
}

struct Vector {
    file: String,
    id: String,
    operation: String,
    input: Value,
    expect: Value,
}

fn load() -> Vec<Vector> {
    let dir = vector_dir();
    let mut paths: Vec<PathBuf> = fs::read_dir(&dir)
        .unwrap_or_else(|e| panic!("reading {}: {e}", dir.display()))
        .map(|entry| entry.expect("directory entry").path())
        .filter(|p| p.extension().is_some_and(|x| x == "json"))
        .collect();
    paths.sort();

    let mut vectors = Vec::new();
    for path in paths {
        let text =
            fs::read_to_string(&path).unwrap_or_else(|e| panic!("reading {}: {e}", path.display()));
        let doc: Value =
            serde_json::from_str(&text).unwrap_or_else(|e| panic!("{}: {e}", path.display()));
        let file = path
            .file_stem()
            .expect("file stem")
            .to_string_lossy()
            .into_owned();
        for v in doc["vectors"].as_array().expect("vectors array") {
            let text = |key: &str| {
                v[key]
                    .as_str()
                    .unwrap_or_else(|| panic!("{file}: vector without {key}"))
                    .to_string()
            };
            vectors.push(Vector {
                file: file.clone(),
                id: text("id"),
                operation: text("operation"),
                input: v["input"].clone(),
                expect: v["expect"].clone(),
            });
        }
    }
    vectors
}

/// Compares JSON values, objects in any order and numbers by value.
fn json_eq(a: &Value, b: &Value) -> bool {
    match (a, b) {
        (Value::Number(x), Value::Number(y)) => x.as_f64() == y.as_f64(),
        (Value::Array(x), Value::Array(y)) => {
            x.len() == y.len() && x.iter().zip(y).all(|(p, q)| json_eq(p, q))
        }
        (Value::Object(x), Value::Object(y)) => {
            x.len() == y.len()
                && x.iter()
                    .all(|(k, v)| y.get(k).is_some_and(|w| json_eq(v, w)))
        }
        _ => a == b,
    }
}

fn run(v: &Vector, units: &dyn UnitValidator) -> Result<(), String> {
    match v.operation.as_str() {
        "validate" => validate(v, units),
        "classify" => classify(v, units),
        "roundtrip" => roundtrip(v, units),
        other => Err(format!("unknown operation {other:?}")),
    }
}

fn validate(v: &Vector, units: &dyn UnitValidator) -> Result<(), String> {
    let result = Quantity::from_value(&v.input["quantity"], units);
    match v.expect["outcome"].as_str() {
        Some("ok") => result
            .map(|_| ())
            .map_err(|e| format!("expected ok, got {e}")),
        Some("rejected") => {
            let want = v.expect["reason"].as_str().unwrap_or_default();
            match result {
                Ok(q) => Err(format!("expected {want}, got ok: {q:?}")),
                Err(e) if e.reason().as_str() == want => Ok(()),
                Err(e) => Err(format!("reason = {}, want {want}", e.reason())),
            }
        }
        other => Err(format!("unknown outcome {other:?}")),
    }
}

fn classify(v: &Vector, units: &dyn UnitValidator) -> Result<(), String> {
    let observation =
        Observation::from_value(&v.input["observation"], units).map_err(|e| e.to_string())?;
    let name = v.input["element"].as_str().unwrap_or_default();
    let element = Element::from_name(name).ok_or_else(|| format!("unknown element {name:?}"))?;
    let want = v.expect["state"].as_str().unwrap_or_default();
    let got = observation.state(element).as_str();
    if got == want {
        Ok(())
    } else {
        Err(format!("state = {got}, want {want}"))
    }
}

fn roundtrip(v: &Vector, units: &dyn UnitValidator) -> Result<(), String> {
    // Go through JSON text, so reading and writing are exercised as a caller would.
    let written = if v.input.get("quantity").is_some() {
        let text = v.input["quantity"].to_string();
        let q = weather::parse_quantity(&text, units).map_err(|e: ParseError| e.to_string())?;
        q.to_json().map_err(|e| e.to_string())?
    } else {
        let text = v.input["observation"].to_string();
        let o = weather::parse_observation(&text, units).map_err(|e: ParseError| e.to_string())?;
        o.to_json().map_err(|e| e.to_string())?
    };
    let got: Value = serde_json::from_str(&written).map_err(|e| e.to_string())?;
    let want = &v.expect["result"];
    if json_eq(&got, want) {
        Ok(())
    } else {
        Err(format!("wrote {written}, want {want}"))
    }
}

#[test]
fn conformance_vectors() {
    let vectors = load();
    assert!(
        !vectors.is_empty(),
        "no conformance vectors found in {}",
        vector_dir().display()
    );

    let units = Validator;
    let failures: Vec<String> = vectors
        .iter()
        .filter_map(|v| {
            run(v, &units)
                .err()
                .map(|why| format!("{}/{}: {why}", v.file, v.id))
        })
        .collect();

    println!(
        "{} of {} vectors passed",
        vectors.len() - failures.len(),
        vectors.len()
    );
    assert!(
        failures.is_empty(),
        "{} vector(s) failed:\n{}",
        failures.len(),
        failures.join("\n")
    );
}

#[test]
fn vector_count_is_what_the_specs_promise() {
    assert_eq!(load().len(), 33, "the core conformance set has 33 vectors");
}

#[test]
fn a_fake_validator_can_stand_in_at_the_seam() {
    let accept_all = |_: &str| true;
    let q = Quantity::parse(r#"{"value": 1, "unit": "degC"}"#, &accept_all)
        .expect("accepted by the fake");
    assert_eq!(q, Quantity::new(Some(1.0), "degC"));
    // Namespaces are rejected before any validator is asked.
    let err = Quantity::parse(r#"{"value": 1, "unit": "wmoUnit:degC"}"#, &accept_all).unwrap_err();
    assert_eq!(
        err.rejection().map(|r| r.reason().as_str()),
        Some("unit.namespaced")
    );
}

#[test]
fn malformed_and_wrong_shaped_input_is_reported_not_panicked() {
    let units = Validator;
    assert!(matches!(
        weather::parse_quantity("{", &units),
        Err(ParseError::Malformed(_))
    ));
    for (text, code) in [
        ("null", "qty.not-object"),
        ("[]", "qty.not-object"),
        (r#"{"value": 1, "unit": 5}"#, "unit.not-ucum"),
        (r#"{"value": 1e999, "unit": "m"}"#, "qty.value-not-numeric"),
    ] {
        match weather::parse_quantity(text, &units) {
            Err(ParseError::Rejected(r)) => assert_eq!(r.reason().as_str(), code, "{text}"),
            Err(ParseError::Malformed(_)) if text.contains("1e999") => {} // the decoder refuses it first
            other => panic!("{text}: {other:?}"),
        }
    }

    let obs = |text: &str| {
        weather::parse_observation(text, &units)
            .unwrap_err()
            .rejection()
            .map(|r| r.reason().as_str())
    };
    let base = r#""station": {"id": "KSEA"}, "observedAt": "2026-01-01T00:00:00Z""#;
    assert_eq!(obs("[]"), Some("obs.not-object"));
    assert_eq!(
        obs(r#"{"observedAt": "2026-01-01T00:00:00Z"}"#),
        Some("obs.station-missing")
    );
    assert_eq!(
        obs(r#"{"station": {"id": "K", "x": "y"}, "observedAt": "2026-01-01T00:00:00Z"}"#),
        Some("obs.station-missing")
    );
    assert_eq!(
        obs(r#"{"station": {"id": "K"}}"#),
        Some("obs.observed-at-missing")
    );
    assert_eq!(
        obs(r#"{"station": {"id": "K"}, "observedAt": "yesterday"}"#),
        Some("obs.observed-at-invalid")
    );
    assert_eq!(
        obs(&format!(r#"{{{base}, "temperature": null}}"#)),
        Some("obs.element-null")
    );
    assert_eq!(
        obs(&format!(r#"{{{base}, "humidity": {{}}}}"#)),
        Some("obs.unknown-member")
    );
    assert_eq!(
        obs(&format!(r#"{{{base}, "windSpeed": {{"unit": "m/s"}}}}"#)),
        Some("qty.value-missing")
    );
}

#[test]
fn observed_at_and_station_survive_unchanged() {
    let units = Validator;
    let text = r#"{"station": {"id": "KSEA", "name": "Seattle"}, "observedAt": "2026-01-01T00:00:00.500+00:00"}"#;
    let o = weather::parse_observation(text, &units).expect("valid");
    assert_eq!(o.observed_at.as_str(), "2026-01-01T00:00:00.500+00:00");
    let written: Value = serde_json::from_str(&o.to_json().expect("writes")).expect("json");
    assert_eq!(written, serde_json::from_str::<Value>(text).expect("json"));
}

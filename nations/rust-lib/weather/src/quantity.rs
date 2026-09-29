use serde_json::{Map, Number, Value};

use crate::rejection::{ParseError, Reason, Rejection};
use crate::unit::{UnitValidator, check_unit};

/// A measured value and the unit it was reported in (WX-QTY).
///
/// `value: None` means "reported without a value". "Not reported" is an `Option<Quantity>`
/// that is `None` on an [`crate::Observation`].
#[derive(Debug, Clone, PartialEq)]
pub struct Quantity {
    /// The number, or `None` when the source answered with `null`.
    pub value: Option<f64>,
    /// The UCUM unit, exactly as the source spelled it.
    pub unit: String,
}

impl Quantity {
    /// Builds a `Quantity` without validating it. Use [`Quantity::from_value`] to validate.
    pub fn new(value: Option<f64>, unit: impl Into<String>) -> Self {
        Self {
            value,
            unit: unit.into(),
        }
    }

    /// Reads a `Quantity` from JSON text, applying WX-QTY and WX-UNIT.
    pub fn parse(json: &str, units: &dyn UnitValidator) -> Result<Self, ParseError> {
        let value: Value = serde_json::from_str(json)?;
        Ok(Self::from_value(&value, units)?)
    }

    /// Reads a `Quantity` from a parsed JSON value.
    ///
    /// It checks the members itself, because a deserializer cannot tell a missing `value`
    /// from `"value": null`, and WX-QTY-007 needs them apart.
    pub fn from_value(json: &Value, units: &dyn UnitValidator) -> Result<Self, Rejection> {
        let Value::Object(members) = json else {
            return Err(Reason::QtyNotObject.into());
        };
        Self::from_members(members, units)
    }

    fn from_members(
        members: &Map<String, Value>,
        units: &dyn UnitValidator,
    ) -> Result<Self, Rejection> {
        if members.keys().any(|k| k != "value" && k != "unit") {
            return Err(Reason::QtyUnknownMember.into());
        }
        let raw_value = members.get("value").ok_or(Reason::QtyValueMissing)?;
        let raw_unit = members.get("unit").ok_or(Reason::QtyUnitMissing)?;

        let value = match raw_value {
            Value::Null => None,
            Value::Number(n) => Some(n.as_f64().ok_or(Reason::QtyValueNotNumeric)?),
            _ => return Err(Reason::QtyValueNotNumeric.into()),
        };

        let unit = raw_unit.as_str().unwrap_or("");
        check_unit(unit, units)?;
        Ok(Self::new(value, unit))
    }

    /// Writes the `Quantity` as JSON. Both members are always written, so a missing value
    /// is written as `null` (WX-QTY-003).
    ///
    /// It fails only for a NaN or infinite value, which JSON cannot carry. Reading never
    /// produces one, but the fields are public.
    pub fn to_value(&self) -> Result<Value, Rejection> {
        let value = match self.value {
            None => Value::Null,
            Some(v) => Value::Number(number(v)?),
        };
        let mut members = Map::new();
        members.insert("value".into(), value);
        members.insert("unit".into(), Value::String(self.unit.clone()));
        Ok(Value::Object(members))
    }

    /// Writes the `Quantity` as JSON text.
    pub fn to_json(&self) -> Result<String, Rejection> {
        Ok(self.to_value()?.to_string())
    }
}

/// Makes a JSON number from `v` without changing it. A whole number that a `f64` holds
/// exactly is written without a fraction, so `12` stays `12`. The shortest decimal that
/// parses back to the same `f64` is written for everything else.
fn number(v: f64) -> Result<Number, Rejection> {
    const EXACT: f64 = 9_007_199_254_740_992.0; // 2^53
    if v.is_finite() && v.fract() == 0.0 && v.abs() < EXACT && !(v == 0.0 && v.is_sign_negative()) {
        return Ok(Number::from(v as i64));
    }
    Number::from_f64(v).ok_or_else(|| Reason::QtyValueNotNumeric.into())
}

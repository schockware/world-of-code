use std::fmt;

use serde_json::{Map, Value};

use crate::quantity::Quantity;
use crate::rejection::{ParseError, Reason, Rejection};
use crate::unit::UnitValidator;

/// What the source said about one element (WX-QTY-001).
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum AnswerState {
    /// Nothing was answered.
    NotReported,
    /// The element was answered, and the answer was empty.
    ReportedWithoutValue,
    /// The element was answered with a value.
    Reported,
}

impl AnswerState {
    /// The state's name as the conformance vectors spell it.
    pub const fn as_str(self) -> &'static str {
        match self {
            AnswerState::NotReported => "not-reported",
            AnswerState::ReportedWithoutValue => "reported-without-value",
            AnswerState::Reported => "reported",
        }
    }
}

impl fmt::Display for AnswerState {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.write_str(self.as_str())
    }
}

/// An element of an [`Observation`], as it is named in the contract.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Element {
    /// `temperature`
    Temperature,
    /// `dewpoint`
    Dewpoint,
    /// `relativeHumidity`
    RelativeHumidity,
    /// `windDirection`
    WindDirection,
    /// `windSpeed`
    WindSpeed,
    /// `windGust`
    WindGust,
    /// `barometricPressure`
    BarometricPressure,
    /// `seaLevelPressure`
    SeaLevelPressure,
    /// `visibility`
    Visibility,
}

impl Element {
    /// Every element in contract order.
    pub const ALL: [Element; 9] = [
        Element::Temperature,
        Element::Dewpoint,
        Element::RelativeHumidity,
        Element::WindDirection,
        Element::WindSpeed,
        Element::WindGust,
        Element::BarometricPressure,
        Element::SeaLevelPressure,
        Element::Visibility,
    ];

    /// The member name in the contract.
    pub const fn name(self) -> &'static str {
        match self {
            Element::Temperature => "temperature",
            Element::Dewpoint => "dewpoint",
            Element::RelativeHumidity => "relativeHumidity",
            Element::WindDirection => "windDirection",
            Element::WindSpeed => "windSpeed",
            Element::WindGust => "windGust",
            Element::BarometricPressure => "barometricPressure",
            Element::SeaLevelPressure => "seaLevelPressure",
            Element::Visibility => "visibility",
        }
    }

    /// Finds the element for a member name, or `None` if the contract has no such element.
    pub fn from_name(name: &str) -> Option<Element> {
        Element::ALL.into_iter().find(|e| e.name() == name)
    }
}

/// The observing station.
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Station {
    /// The station identifier.
    pub id: String,
    /// The station's name, if the source gave one.
    pub name: Option<String>,
}

/// The instant an observation applies to, kept as the RFC 3339 text the source used.
///
/// Holding the text keeps the offset and the fractional digits exactly as reported, so
/// `+00:00` is not rewritten as `Z`. The cost is that it is not an instant you can compare
/// or subtract. Convert at the edge if you need that. It can only be built from valid
/// RFC 3339 text, so an `ObservedAt` is always well-formed.
#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub struct ObservedAt(String);

impl ObservedAt {
    /// Validates `text` as an RFC 3339 date-time and keeps it unchanged.
    pub fn new(text: impl Into<String>) -> Result<Self, Rejection> {
        let text = text.into();
        if is_rfc3339(&text) {
            Ok(Self(text))
        } else {
            Err(Reason::ObsObservedAtInvalid.into())
        }
    }

    /// The text as reported.
    pub fn as_str(&self) -> &str {
        &self.0
    }
}

impl fmt::Display for ObservedAt {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.write_str(&self.0)
    }
}

/// A set of surface weather elements observed at one station at one instant.
/// A `None` element was not reported.
#[derive(Debug, Clone, PartialEq)]
pub struct Observation {
    /// The observing station.
    pub station: Station,
    /// The instant the observation applies to.
    pub observed_at: ObservedAt,
    /// `temperature`
    pub temperature: Option<Quantity>,
    /// `dewpoint`
    pub dewpoint: Option<Quantity>,
    /// `relativeHumidity`
    pub relative_humidity: Option<Quantity>,
    /// `windDirection`
    pub wind_direction: Option<Quantity>,
    /// `windSpeed`
    pub wind_speed: Option<Quantity>,
    /// `windGust`
    pub wind_gust: Option<Quantity>,
    /// `barometricPressure`
    pub barometric_pressure: Option<Quantity>,
    /// `seaLevelPressure`
    pub sea_level_pressure: Option<Quantity>,
    /// `visibility`
    pub visibility: Option<Quantity>,
}

impl Observation {
    /// An observation with no elements reported.
    pub fn new(station: Station, observed_at: ObservedAt) -> Self {
        Self {
            station,
            observed_at,
            temperature: None,
            dewpoint: None,
            relative_humidity: None,
            wind_direction: None,
            wind_speed: None,
            wind_gust: None,
            barometric_pressure: None,
            sea_level_pressure: None,
            visibility: None,
        }
    }

    /// The quantity for `element`, or `None` if it was not reported.
    pub fn get(&self, element: Element) -> Option<&Quantity> {
        self.slot(element).as_ref()
    }

    /// Sets an element. `None` means not reported.
    pub fn set(&mut self, element: Element, quantity: Option<Quantity>) {
        *self.slot_mut(element) = quantity;
    }

    fn slot(&self, element: Element) -> &Option<Quantity> {
        match element {
            Element::Temperature => &self.temperature,
            Element::Dewpoint => &self.dewpoint,
            Element::RelativeHumidity => &self.relative_humidity,
            Element::WindDirection => &self.wind_direction,
            Element::WindSpeed => &self.wind_speed,
            Element::WindGust => &self.wind_gust,
            Element::BarometricPressure => &self.barometric_pressure,
            Element::SeaLevelPressure => &self.sea_level_pressure,
            Element::Visibility => &self.visibility,
        }
    }

    fn slot_mut(&mut self, element: Element) -> &mut Option<Quantity> {
        match element {
            Element::Temperature => &mut self.temperature,
            Element::Dewpoint => &mut self.dewpoint,
            Element::RelativeHumidity => &mut self.relative_humidity,
            Element::WindDirection => &mut self.wind_direction,
            Element::WindSpeed => &mut self.wind_speed,
            Element::WindGust => &mut self.wind_gust,
            Element::BarometricPressure => &mut self.barometric_pressure,
            Element::SeaLevelPressure => &mut self.sea_level_pressure,
            Element::Visibility => &mut self.visibility,
        }
    }

    /// What the source said about `element` (WX-QTY-001 to 005).
    pub fn state(&self, element: Element) -> AnswerState {
        match self.get(element) {
            None => AnswerState::NotReported,
            Some(Quantity { value: None, .. }) => AnswerState::ReportedWithoutValue,
            Some(_) => AnswerState::Reported,
        }
    }

    /// Reads an `Observation` from JSON text. Only the parts the core specs cover are strict.
    /// Reasons starting `obs.` are provisional until WX-OBS is written.
    pub fn parse(json: &str, units: &dyn UnitValidator) -> Result<Self, ParseError> {
        let value: Value = serde_json::from_str(json)?;
        Ok(Self::from_value(&value, units)?)
    }

    /// Reads an `Observation` from a parsed JSON value. Members are checked in name order,
    /// so with several faults the reason is stable.
    pub fn from_value(json: &Value, units: &dyn UnitValidator) -> Result<Self, Rejection> {
        let Value::Object(members) = json else {
            return Err(Reason::ObsNotObject.into());
        };

        let mut station = None;
        let mut observed_at = None;
        let mut elements: Vec<(Element, Quantity)> = Vec::new();
        for (name, member) in members {
            if let Some(element) = Element::from_name(name) {
                // "Not reported" has exactly one form: the element is omitted. A JSON null is not allowed.
                if member.is_null() {
                    return Err(Reason::ObsElementNull.into());
                }
                elements.push((element, Quantity::from_value(member, units)?));
                continue;
            }
            match name.as_str() {
                "station" => station = Some(parse_station(member)?),
                "observedAt" => {
                    let text = member.as_str().ok_or(Reason::ObsObservedAtInvalid)?;
                    observed_at = Some(ObservedAt::new(text)?);
                }
                _ => return Err(Reason::ObsUnknownMember.into()),
            }
        }

        let station = station.ok_or(Reason::ObsStationMissing)?;
        let observed_at = observed_at.ok_or(Reason::ObsObservedAtMissing)?;
        let mut observation = Self::new(station, observed_at);
        for (element, quantity) in elements {
            observation.set(element, Some(quantity));
        }
        Ok(observation)
    }

    /// Writes contract JSON. Elements that were not reported are omitted.
    pub fn to_value(&self) -> Result<Value, Rejection> {
        let mut members = Map::new();
        let mut station = Map::new();
        station.insert("id".into(), Value::String(self.station.id.clone()));
        if let Some(name) = &self.station.name {
            station.insert("name".into(), Value::String(name.clone()));
        }
        members.insert("station".into(), Value::Object(station));
        members.insert(
            "observedAt".into(),
            Value::String(self.observed_at.0.clone()),
        );
        for element in Element::ALL {
            if let Some(quantity) = self.get(element) {
                members.insert(element.name().into(), quantity.to_value()?);
            }
        }
        Ok(Value::Object(members))
    }

    /// Writes contract JSON text.
    pub fn to_json(&self) -> Result<String, Rejection> {
        Ok(self.to_value()?.to_string())
    }
}

/// A station needs a string `id`, may have a string `name`, and has no other members.
/// Anything else is reported as a missing station.
fn parse_station(json: &Value) -> Result<Station, Rejection> {
    let bad = || Rejection::from(Reason::ObsStationMissing);
    let Value::Object(members) = json else {
        return Err(bad());
    };
    let mut id = None;
    let mut name = None;
    for (key, member) in members {
        let text = member.as_str().ok_or_else(bad)?.to_string();
        match key.as_str() {
            "id" => id = Some(text),
            "name" => name = Some(text),
            _ => return Err(bad()),
        }
    }
    Ok(Station {
        id: id.ok_or_else(bad)?,
        name,
    })
}

/// Checks the RFC 3339 `date-time` production: `YYYY-MM-DDTHH:MM:SS[.f+](Z|+HH:MM|-HH:MM)`.
/// `T` and `Z` may be lower case, as the RFC allows. Calendar and clock ranges are checked,
/// with a leap second (`:60`) allowed.
fn is_rfc3339(s: &str) -> bool {
    let b = s.as_bytes();
    let digits = |from: usize, len: usize| -> Option<u32> {
        let part = b.get(from..from + len)?;
        part.iter()
            .all(u8::is_ascii_digit)
            .then(|| part.iter().fold(0, |n, d| n * 10 + u32::from(d - b'0')))
    };
    let at = |i: usize, c: u8| b.get(i).is_some_and(|x| x.eq_ignore_ascii_case(&c));

    let (Some(year), Some(month), Some(day)) = (digits(0, 4), digits(5, 2), digits(8, 2)) else {
        return false;
    };
    if !(at(4, b'-') && at(7, b'-') && at(10, b'T') && at(13, b':') && at(16, b':')) {
        return false;
    }
    let leap = year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
    let days = match month {
        1 | 3 | 5 | 7 | 8 | 10 | 12 => 31,
        4 | 6 | 9 | 11 => 30,
        2 if leap => 29,
        2 => 28,
        _ => return false,
    };
    if !(1..=days).contains(&day) {
        return false;
    }
    let (Some(hour), Some(minute), Some(second)) = (digits(11, 2), digits(14, 2), digits(17, 2))
    else {
        return false;
    };
    if hour > 23 || minute > 59 || second > 60 {
        return false;
    }

    let mut i = 19;
    if at(i, b'.') {
        let start = i + 1;
        i = start;
        while b.get(i).is_some_and(u8::is_ascii_digit) {
            i += 1;
        }
        if i == start {
            return false;
        }
    }
    if at(i, b'Z') {
        return i + 1 == b.len();
    }
    if !(at(i, b'+') || at(i, b'-')) || !at(i + 3, b':') || b.len() != i + 6 {
        return false;
    }
    matches!((digits(i + 1, 2), digits(i + 4, 2)), (Some(h), Some(m)) if h <= 23 && m <= 59)
}

#[cfg(test)]
mod tests {
    use super::is_rfc3339;

    #[test]
    fn rfc3339_timestamps() {
        for ok in [
            "2026-01-01T00:00:00Z",
            "2026-01-01T00:00:00+00:00",
            "2026-02-28T23:59:60.123456789-07:00",
            "2024-02-29T12:00:00z",
        ] {
            assert!(is_rfc3339(ok), "{ok}");
        }
        for bad in [
            "",
            "2026-01-01",
            "2026-01-01 00:00:00Z",
            "2026-01-01T00:00:00",
            "2026-13-01T00:00:00Z",
            "2025-02-29T00:00:00Z",
            "2026-01-01T24:00:00Z",
            "2026-01-01T00:00:00.Z",
            "2026-01-01T00:00:00+0000",
            "2026-01-01T00:00:00Zjunk",
            "２０２６-01-01T00:00:00Z",
        ] {
            assert!(!is_rfc3339(bad), "{bad}");
        }
    }
}

//! Weather observations as the weather domain specs define them (`domains/weather/specs`).
//! The crate has no population: no HTTP, UI or CLI concerns.
//!
//! Reading is strict and returns `Result<_, Rejection>`, or `Result<_, ParseError>` when
//! starting from JSON text. Writing does not round or convert anything.

mod observation;
mod quantity;
mod rejection;
mod unit;

pub mod ucum;

pub use observation::{AnswerState, Element, Observation, ObservedAt, Station};
pub use quantity::Quantity;
pub use rejection::{ParseError, Reason, Rejection};
pub use unit::UnitValidator;

/// Reads a [`Quantity`] from JSON text. See [`Quantity::parse`].
pub fn parse_quantity(json: &str, units: &dyn UnitValidator) -> Result<Quantity, ParseError> {
    Quantity::parse(json, units)
}

/// Reads an [`Observation`] from JSON text. See [`Observation::parse`].
pub fn parse_observation(json: &str, units: &dyn UnitValidator) -> Result<Observation, ParseError> {
    Observation::parse(json, units)
}

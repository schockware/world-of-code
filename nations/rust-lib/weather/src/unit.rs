use crate::rejection::{Reason, Rejection};

/// The seam for unit validation: whether a unit is a valid UCUM code (case-sensitive form).
///
/// [`crate::ucum::Validator`] is the real implementation. Any `Fn(&str) -> bool` also works,
/// which is how a test supplies a fake.
pub trait UnitValidator {
    /// Reports whether `unit` is a valid UCUM code.
    fn is_valid(&self, unit: &str) -> bool;
}

impl<F: Fn(&str) -> bool> UnitValidator for F {
    fn is_valid(&self, unit: &str) -> bool {
        self(unit)
    }
}

/// Namespace prefixes that WX-UNIT-003 rejects.
const NAMESPACE_PREFIXES: [&str; 4] = ["wmo:", "wmoUnit:", "nwsUnit:", "uc:"];

/// Applies WX-UNIT in the order the specs need: empty, namespaced, then UCUM.
pub(crate) fn check_unit(unit: &str, units: &dyn UnitValidator) -> Result<(), Rejection> {
    if unit.is_empty() {
        return Err(Reason::UnitNotUcum.into());
    }
    // WX-UNIT-003: a namespaced unit is reported as namespaced even though it is not valid UCUM either.
    if NAMESPACE_PREFIXES.iter().any(|p| unit.starts_with(p)) {
        return Err(Reason::UnitNamespaced.into());
    }
    if !units.is_valid(unit) {
        return Err(Reason::UnitNotUcum.into());
    }
    Ok(())
}

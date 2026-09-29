use std::error::Error;
use std::fmt;

/// A rejection code defined by the weather specs.
///
/// The variants marked provisional are not backed by a spec requirement yet: WX-OBS is not
/// written, and WX-QTY does not say what a non-object `Quantity` is.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Reason {
    /// `qty.value-missing` (WX-QTY-007).
    QtyValueMissing,
    /// `qty.unit-missing` (WX-QTY-008).
    QtyUnitMissing,
    /// `qty.value-not-numeric` (WX-QTY-009).
    QtyValueNotNumeric,
    /// `qty.unknown-member` (WX-QTY-010).
    QtyUnknownMember,
    /// `unit.not-ucum` (WX-UNIT-001, WX-UNIT-004).
    UnitNotUcum,
    /// `unit.namespaced` (WX-UNIT-003).
    UnitNamespaced,

    /// Provisional: a `Quantity` that is not a JSON object.
    QtyNotObject,
    /// Provisional: an `Observation` that is not a JSON object.
    ObsNotObject,
    /// Provisional: the `station` member is missing or malformed.
    ObsStationMissing,
    /// Provisional: the `observedAt` member is missing.
    ObsObservedAtMissing,
    /// Provisional: `observedAt` is not an RFC 3339 date-time string.
    ObsObservedAtInvalid,
    /// Provisional: an element is JSON `null`. Not reported is expressed by omission only.
    ObsElementNull,
    /// Provisional: an `Observation` member the contract does not define.
    ObsUnknownMember,
}

impl Reason {
    /// The code as the conformance vectors spell it.
    pub const fn as_str(self) -> &'static str {
        match self {
            Reason::QtyValueMissing => "qty.value-missing",
            Reason::QtyUnitMissing => "qty.unit-missing",
            Reason::QtyValueNotNumeric => "qty.value-not-numeric",
            Reason::QtyUnknownMember => "qty.unknown-member",
            Reason::UnitNotUcum => "unit.not-ucum",
            Reason::UnitNamespaced => "unit.namespaced",
            Reason::QtyNotObject => "qty.not-object",
            Reason::ObsNotObject => "obs.not-object",
            Reason::ObsStationMissing => "obs.station-missing",
            Reason::ObsObservedAtMissing => "obs.observed-at-missing",
            Reason::ObsObservedAtInvalid => "obs.observed-at-invalid",
            Reason::ObsElementNull => "obs.element-null",
            Reason::ObsUnknownMember => "obs.unknown-member",
        }
    }
}

impl fmt::Display for Reason {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        f.write_str(self.as_str())
    }
}

/// An input the specs reject. It is a value that carries its [`Reason`].
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct Rejection {
    reason: Reason,
}

impl Rejection {
    /// Makes a rejection for `reason`.
    pub const fn new(reason: Reason) -> Self {
        Self { reason }
    }

    /// Why the input was rejected.
    pub const fn reason(&self) -> Reason {
        self.reason
    }
}

impl From<Reason> for Rejection {
    fn from(reason: Reason) -> Self {
        Self::new(reason)
    }
}

impl fmt::Display for Rejection {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(f, "rejected: {}", self.reason)
    }
}

impl Error for Rejection {}

/// What can go wrong reading JSON text: it is not JSON at all, or it is JSON the specs reject.
#[derive(Debug)]
pub enum ParseError {
    /// The text is not well-formed JSON, so the specs were never consulted.
    Malformed(serde_json::Error),
    /// The JSON was read and the specs rejected it.
    Rejected(Rejection),
}

impl ParseError {
    /// The rejection, if the specs rejected the input.
    pub fn rejection(&self) -> Option<Rejection> {
        match self {
            ParseError::Rejected(r) => Some(*r),
            ParseError::Malformed(_) => None,
        }
    }
}

impl fmt::Display for ParseError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            ParseError::Malformed(e) => write!(f, "malformed JSON: {e}"),
            ParseError::Rejected(r) => r.fmt(f),
        }
    }
}

impl Error for ParseError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        match self {
            ParseError::Malformed(e) => Some(e),
            ParseError::Rejected(r) => Some(r),
        }
    }
}

impl From<Rejection> for ParseError {
    fn from(r: Rejection) -> Self {
        ParseError::Rejected(r)
    }
}

impl From<serde_json::Error> for ParseError {
    fn from(e: serde_json::Error) -> Self {
        ParseError::Malformed(e)
    }
}

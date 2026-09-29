// Package weather models weather observations as the weather domain specs define them
// (domains/weather/specs). It has no population: no HTTP, UI or CLI concerns.
package weather

// AnswerState is what the source said about one element (WX-QTY-001).
type AnswerState int

const (
	// NotReported is the zero value: nothing was answered.
	NotReported AnswerState = iota
	// ReportedWithoutValue means the element was answered, and the answer was empty.
	ReportedWithoutValue
	// Reported means the element was answered with a value.
	Reported
)

// String returns the state's name as the conformance vectors spell it.
func (s AnswerState) String() string {
	switch s {
	case NotReported:
		return "not-reported"
	case ReportedWithoutValue:
		return "reported-without-value"
	case Reported:
		return "reported"
	}
	return "unknown"
}

// Reason is a rejection code defined by the weather specs.
type Reason string

// Codes marked provisional are not yet backed by a spec requirement.
const (
	ReasonValueMissing    Reason = "qty.value-missing"
	ReasonUnitMissing     Reason = "qty.unit-missing"
	ReasonValueNotNumeric Reason = "qty.value-not-numeric"
	ReasonUnknownMember   Reason = "qty.unknown-member"
	ReasonNotUCUM         Reason = "unit.not-ucum"
	ReasonNamespaced      Reason = "unit.namespaced"

	// Provisional: WX-OBS is not written yet, and WX-QTY does not say what a non-object Quantity is.
	ReasonQtyNotObject       Reason = "qty.not-object"
	ReasonObsNotObject       Reason = "obs.not-object"
	ReasonObsStationMissing  Reason = "obs.station-missing"
	ReasonObsObservedAtMiss  Reason = "obs.observed-at-missing"
	ReasonObsObservedAtInval Reason = "obs.observed-at-invalid"
	ReasonObsElementNull     Reason = "obs.element-null"
	ReasonObsUnknownMember   Reason = "obs.unknown-member"
)

// RejectionError is returned when an input is rejected. Test for it with errors.As.
type RejectionError struct{ Reason Reason }

func (e *RejectionError) Error() string { return string(e.Reason) }

func reject(r Reason) error { return &RejectionError{Reason: r} }

// UnitValidator reports whether a unit is a valid UCUM code (case-sensitive form).
type UnitValidator interface {
	Valid(unit string) bool
}

// UnitValidatorFunc adapts a function to a UnitValidator.
type UnitValidatorFunc func(string) bool

// Valid calls f.
func (f UnitValidatorFunc) Valid(unit string) bool { return f(unit) }

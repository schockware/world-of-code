package weather

import (
	"bytes"
	"encoding/json"
	"strings"
)

// Quantity is a measured value and the unit it was reported in (WX-QTY).
// A nil Value means "reported without a value". "Not reported" is a nil *Quantity on an
// Observation, which is why the observation's element fields are pointers.
type Quantity struct {
	Value *float64
	Unit  string
}

// MarshalJSON always writes both members, so a nil Value is written as null (WX-QTY-003).
func (q Quantity) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Value *float64 `json:"value"`
		Unit  string   `json:"unit"`
	}{q.Value, q.Unit})
}

var namespacePrefixes = []string{"wmo:", "wmoUnit:", "nwsUnit:", "uc:"}

// ParseQuantity reads a Quantity from JSON, applying WX-QTY and WX-UNIT.
// A rejected input returns a *RejectionError. Malformed JSON returns the decoder's error.
//
// It checks the members itself, because encoding/json cannot tell a missing "value"
// from "value": null, and WX-QTY-007 needs them apart.
func ParseQuantity(data []byte, units UnitValidator) (Quantity, error) {
	var raw map[string]json.RawMessage
	if err := json.Unmarshal(data, &raw); err != nil {
		if _, isType := err.(*json.UnmarshalTypeError); !isType {
			return Quantity{}, err
		}
		return Quantity{}, reject(ReasonQtyNotObject)
	}
	if raw == nil { // the JSON literal null
		return Quantity{}, reject(ReasonQtyNotObject)
	}
	return parseQuantityMembers(raw, units)
}

func parseQuantityMembers(raw map[string]json.RawMessage, units UnitValidator) (Quantity, error) {
	for name := range raw {
		if name != "value" && name != "unit" {
			return Quantity{}, reject(ReasonUnknownMember)
		}
	}
	rawValue, hasValue := raw["value"]
	rawUnit, hasUnit := raw["unit"]
	if !hasValue {
		return Quantity{}, reject(ReasonValueMissing)
	}
	if !hasUnit {
		return Quantity{}, reject(ReasonUnitMissing)
	}

	value, err := parseValue(rawValue)
	if err != nil {
		return Quantity{}, err
	}

	var unit string
	if err := json.Unmarshal(rawUnit, &unit); err != nil || unit == "" {
		return Quantity{}, reject(ReasonNotUCUM)
	}
	// WX-UNIT-003: a namespaced unit is reported as namespaced even though it is not valid UCUM either.
	for _, prefix := range namespacePrefixes {
		if strings.HasPrefix(unit, prefix) {
			return Quantity{}, reject(ReasonNamespaced)
		}
	}
	if !units.Valid(unit) {
		return Quantity{}, reject(ReasonNotUCUM)
	}
	return Quantity{Value: value, Unit: unit}, nil
}

// parseValue accepts a JSON number or null. Anything else, or a number that does not fit a float64, is rejected.
func parseValue(raw json.RawMessage) (*float64, error) {
	raw = bytes.TrimSpace(raw)
	if string(raw) == "null" {
		return nil, nil
	}
	if len(raw) == 0 || (raw[0] != '-' && (raw[0] < '0' || raw[0] > '9')) {
		return nil, reject(ReasonValueNotNumeric)
	}
	var f float64
	if err := json.Unmarshal(raw, &f); err != nil {
		return nil, reject(ReasonValueNotNumeric)
	}
	return &f, nil
}

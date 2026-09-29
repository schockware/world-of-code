package weather

import (
	"encoding/json"
	"time"
)

// Station is the observing station.
type Station struct {
	ID   string  `json:"id"`
	Name *string `json:"name,omitempty"`
}

// Element names an element of an Observation, as it appears in the contract.
type Element string

const (
	Temperature        Element = "temperature"
	Dewpoint           Element = "dewpoint"
	RelativeHumidity   Element = "relativeHumidity"
	WindDirection      Element = "windDirection"
	WindSpeed          Element = "windSpeed"
	WindGust           Element = "windGust"
	BarometricPressure Element = "barometricPressure"
	SeaLevelPressure   Element = "seaLevelPressure"
	Visibility         Element = "visibility"
)

// Elements lists every element in contract order.
var Elements = []Element{
	Temperature, Dewpoint, RelativeHumidity, WindDirection, WindSpeed,
	WindGust, BarometricPressure, SeaLevelPressure, Visibility,
}

// Observation is a set of surface weather elements observed at one station at one instant.
// A nil element was not reported.
type Observation struct {
	Station    Station
	ObservedAt time.Time

	Temperature        *Quantity
	Dewpoint           *Quantity
	RelativeHumidity   *Quantity
	WindDirection      *Quantity
	WindSpeed          *Quantity
	WindGust           *Quantity
	BarometricPressure *Quantity
	SeaLevelPressure   *Quantity
	Visibility         *Quantity
}

func (o *Observation) slot(e Element) **Quantity {
	switch e {
	case Temperature:
		return &o.Temperature
	case Dewpoint:
		return &o.Dewpoint
	case RelativeHumidity:
		return &o.RelativeHumidity
	case WindDirection:
		return &o.WindDirection
	case WindSpeed:
		return &o.WindSpeed
	case WindGust:
		return &o.WindGust
	case BarometricPressure:
		return &o.BarometricPressure
	case SeaLevelPressure:
		return &o.SeaLevelPressure
	case Visibility:
		return &o.Visibility
	}
	return nil
}

// State answers "what did the source say about this element?" (WX-QTY-001 to 005).
// An unknown element is reported as NotReported.
func (o Observation) State(e Element) AnswerState {
	slot := o.slot(e)
	switch {
	case slot == nil || *slot == nil:
		return NotReported
	case (*slot).Value == nil:
		return ReportedWithoutValue
	}
	return Reported
}

// MarshalJSON writes contract JSON. Elements that were not reported are omitted.
func (o Observation) MarshalJSON() ([]byte, error) {
	wire := struct {
		Station    Station `json:"station"`
		ObservedAt string  `json:"observedAt"`

		Temperature        *Quantity `json:"temperature,omitempty"`
		Dewpoint           *Quantity `json:"dewpoint,omitempty"`
		RelativeHumidity   *Quantity `json:"relativeHumidity,omitempty"`
		WindDirection      *Quantity `json:"windDirection,omitempty"`
		WindSpeed          *Quantity `json:"windSpeed,omitempty"`
		WindGust           *Quantity `json:"windGust,omitempty"`
		BarometricPressure *Quantity `json:"barometricPressure,omitempty"`
		SeaLevelPressure   *Quantity `json:"seaLevelPressure,omitempty"`
		Visibility         *Quantity `json:"visibility,omitempty"`
	}{
		Station:            o.Station,
		ObservedAt:         o.ObservedAt.Format(time.RFC3339Nano),
		Temperature:        o.Temperature,
		Dewpoint:           o.Dewpoint,
		RelativeHumidity:   o.RelativeHumidity,
		WindDirection:      o.WindDirection,
		WindSpeed:          o.WindSpeed,
		WindGust:           o.WindGust,
		BarometricPressure: o.BarometricPressure,
		SeaLevelPressure:   o.SeaLevelPressure,
		Visibility:         o.Visibility,
	}
	return json.Marshal(wire)
}

// ParseObservation reads an Observation from JSON. Only the parts the core specs cover are strict.
// Reasons starting "obs." are provisional until WX-OBS is written.
func ParseObservation(data []byte, units UnitValidator) (Observation, error) {
	var raw map[string]json.RawMessage
	if err := json.Unmarshal(data, &raw); err != nil {
		if _, isType := err.(*json.UnmarshalTypeError); !isType {
			return Observation{}, err
		}
		return Observation{}, reject(ReasonObsNotObject)
	}
	if raw == nil {
		return Observation{}, reject(ReasonObsNotObject)
	}

	var (
		obs        Observation
		hasStation bool
		hasTime    bool
	)
	for name, member := range raw {
		switch Element(name) {
		case Temperature, Dewpoint, RelativeHumidity, WindDirection, WindSpeed,
			WindGust, BarometricPressure, SeaLevelPressure, Visibility:
			// "Not reported" has exactly one form: the element is omitted. A JSON null is not allowed.
			if string(member) == "null" {
				return Observation{}, reject(ReasonObsElementNull)
			}
			q, err := ParseQuantity(member, units)
			if err != nil {
				return Observation{}, err
			}
			*obs.slot(Element(name)) = &q
		default:
			switch name {
			case "station":
				station, ok := parseStation(member)
				if !ok {
					return Observation{}, reject(ReasonObsStationMissing)
				}
				obs.Station, hasStation = station, true
			case "observedAt":
				var s string
				if err := json.Unmarshal(member, &s); err != nil {
					return Observation{}, reject(ReasonObsObservedAtInval)
				}
				t, err := time.Parse(time.RFC3339, s)
				if err != nil {
					return Observation{}, reject(ReasonObsObservedAtInval)
				}
				obs.ObservedAt, hasTime = t, true
			default:
				return Observation{}, reject(ReasonObsUnknownMember)
			}
		}
	}
	if !hasStation {
		return Observation{}, reject(ReasonObsStationMissing)
	}
	if !hasTime {
		return Observation{}, reject(ReasonObsObservedAtMiss)
	}
	return obs, nil
}

func parseStation(raw json.RawMessage) (Station, bool) {
	var members map[string]json.RawMessage
	if err := json.Unmarshal(raw, &members); err != nil || members == nil {
		return Station{}, false
	}
	var station Station
	var hasID bool
	for name, member := range members {
		var s string
		if err := json.Unmarshal(member, &s); err != nil {
			return Station{}, false
		}
		switch name {
		case "id":
			station.ID, hasID = s, true
		case "name":
			station.Name = &s
		default:
			return Station{}, false
		}
	}
	return station, hasID
}

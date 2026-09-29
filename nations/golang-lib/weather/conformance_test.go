package weather_test

import (
	"encoding/json"
	"errors"
	"reflect"
	"testing"

	"world-of-code.example/golang-lib/weather"
	"world-of-code.example/golang-lib/weather/internal/vectors"
	"world-of-code.example/golang-lib/weather/ucum"
)

// The conformance vectors shared by every nation. Passing them is the scoreboard.
const vectorDir = "../../../domains/weather/specs/core/conformance"

func TestConformance(t *testing.T) {
	files, err := vectors.Load(vectorDir)
	if err != nil {
		t.Fatal(err)
	}
	if len(files) == 0 {
		t.Fatalf("no conformance files found in %s", vectorDir)
	}

	units := ucum.Validator{}
	for _, file := range files {
		for _, v := range file.Vectors {
			t.Run(file.Name+"/"+v.ID, func(t *testing.T) {
				switch v.Operation {
				case "validate":
					runValidate(t, v, units)
				case "classify":
					runClassify(t, v, units)
				case "roundtrip":
					runRoundtrip(t, v, units)
				default:
					t.Fatalf("unknown operation %q", v.Operation)
				}
			})
		}
	}
}

type expectation struct {
	Outcome string         `json:"outcome"`
	Reason  weather.Reason `json:"reason"`
	State   string         `json:"state"`
	Result  any            `json:"result"`
}

func decodeExpect(t *testing.T, v vectors.Vector) expectation {
	t.Helper()
	var e expectation
	if err := json.Unmarshal(v.Expect, &e); err != nil {
		t.Fatal(err)
	}
	return e
}

func runValidate(t *testing.T, v vectors.Vector, units weather.UnitValidator) {
	t.Helper()
	var in struct {
		Quantity json.RawMessage `json:"quantity"`
	}
	if err := json.Unmarshal(v.Input, &in); err != nil {
		t.Fatal(err)
	}
	want := decodeExpect(t, v)

	_, err := weather.ParseQuantity(in.Quantity, units)
	if want.Outcome == "ok" {
		if err != nil {
			t.Fatalf("expected ok, got %v", err)
		}
		return
	}
	var rej *weather.RejectionError
	if !errors.As(err, &rej) {
		t.Fatalf("expected a rejection with reason %q, got %v", want.Reason, err)
	}
	if rej.Reason != want.Reason {
		t.Errorf("reason = %q, want %q", rej.Reason, want.Reason)
	}
}

func runClassify(t *testing.T, v vectors.Vector, units weather.UnitValidator) {
	t.Helper()
	var in struct {
		Observation json.RawMessage `json:"observation"`
		Element     weather.Element `json:"element"`
	}
	if err := json.Unmarshal(v.Input, &in); err != nil {
		t.Fatal(err)
	}
	obs, err := weather.ParseObservation(in.Observation, units)
	if err != nil {
		t.Fatal(err)
	}
	if got, want := obs.State(in.Element).String(), decodeExpect(t, v).State; got != want {
		t.Errorf("state = %q, want %q", got, want)
	}
}

func runRoundtrip(t *testing.T, v vectors.Vector, units weather.UnitValidator) {
	t.Helper()
	var in struct {
		Quantity    json.RawMessage `json:"quantity"`
		Observation json.RawMessage `json:"observation"`
	}
	if err := json.Unmarshal(v.Input, &in); err != nil {
		t.Fatal(err)
	}

	var written []byte
	if in.Quantity != nil {
		q, err := weather.ParseQuantity(in.Quantity, units)
		if err != nil {
			t.Fatal(err)
		}
		written, err = json.Marshal(q)
		if err != nil {
			t.Fatal(err)
		}
	} else {
		o, err := weather.ParseObservation(in.Observation, units)
		if err != nil {
			t.Fatal(err)
		}
		written, err = json.Marshal(o)
		if err != nil {
			t.Fatal(err)
		}
	}

	var got any
	if err := json.Unmarshal(written, &got); err != nil {
		t.Fatal(err)
	}
	if want := decodeExpect(t, v).Result; !reflect.DeepEqual(got, want) {
		t.Errorf("wrote %s, want %v", written, want)
	}
}

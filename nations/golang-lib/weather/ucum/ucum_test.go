package ucum_test

import (
	"testing"

	"world-of-code.example/golang-lib/weather/ucum"
)

func TestEveryAtomInTheUnitListIsValid(t *testing.T) {
	var failures []string
	for _, code := range ucum.Atoms() {
		if !ucum.Valid(code) {
			failures = append(failures, code)
		}
	}
	if len(failures) > 0 {
		t.Errorf("rejected atoms: %v", failures)
	}
	if len(ucum.Atoms()) < 300 {
		t.Errorf("only %d atoms loaded, expected the full unit list", len(ucum.Atoms()))
	}
}

func TestValid(t *testing.T) {
	tests := []struct {
		code string
		want bool
	}{
		{"m/s", true},
		{"/min", true},
		{"km2", true},
		{"s-1", true},
		{"10*3/uL", true},
		{"mm[Hg]", true},
		{"kg.m/s2", true},
		{"(m/s)", true},
		{"{rbc}", true},
		{"mL{total}", true},
		{"hPa", true},
		{"%", true},
		{"[degF]", true},

		{"", false},
		{"degC", false},
		{"celsius", false},
		{"cel", false},
		{"m/", false},
		{"m..s", false},
		{"m s", false},
		{"(m", false},
		{"(m/s)2", false}, // an exponent applies to a unit, not to a parenthesized term
		{"m{unclosed", false},
		{"m-", false},
		{"[degF", false},
		{"kBtu_IT[", false},
		{"mé", false},
	}
	for _, tt := range tests {
		t.Run(tt.code, func(t *testing.T) {
			if got := ucum.Valid(tt.code); got != tt.want {
				t.Errorf("Valid(%q) = %v, want %v", tt.code, got, tt.want)
			}
		})
	}
}

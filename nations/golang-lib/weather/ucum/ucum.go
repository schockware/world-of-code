// Package ucum validates UCUM codes in the case-sensitive form. It checks validity only.
// There is no conversion. The unit list in atoms_gen.go comes from domains/weather/standards/ucum.
package ucum

//go:generate go run gen.go

import "strings"

// Validator reports whether a unit is a valid UCUM code. Its zero value is ready to use.
type Validator struct{}

// Valid reports whether unit is a valid UCUM code (case-sensitive form).
func (Validator) Valid(unit string) bool { return Valid(unit) }

// Valid reports whether unit is a valid UCUM code (case-sensitive form).
func Valid(unit string) bool {
	if unit == "" {
		return false
	}
	p := parser{s: unit}
	if p.s[0] == '/' {
		p.i++
	}
	return p.term() && p.i == len(p.s)
}

// Atoms returns every atom code in the unit list, for tests.
func Atoms() []string {
	codes := make([]string, 0, len(atoms))
	for c := range atoms {
		codes = append(codes, c)
	}
	return codes
}

// parser is a recursive descent parser over the UCUM grammar.
// See domains/weather/standards/ucum/README.MD.
type parser struct {
	s string
	i int
}

func (p *parser) term() bool {
	if !p.component() {
		return false
	}
	for p.i < len(p.s) && (p.s[p.i] == '.' || p.s[p.i] == '/') {
		p.i++
		if !p.component() {
			return false
		}
	}
	return true
}

func (p *parser) component() bool {
	if p.i >= len(p.s) {
		return false
	}
	switch c := p.s[p.i]; {
	case c == '(':
		p.i++
		if !p.term() || p.i >= len(p.s) || p.s[p.i] != ')' {
			return false
		}
		p.i++
		return true
	case c == '{':
		return p.annotation()
	}

	var symbol string
	if isDigit(p.s[p.i]) {
		// "10*" and "10^" are atoms, and any other digit run is a plain factor.
		if strings.HasPrefix(p.s[p.i:], "10*") || strings.HasPrefix(p.s[p.i:], "10^") {
			symbol = p.s[p.i : p.i+3]
			p.i += 3
		} else {
			for p.i < len(p.s) && isDigit(p.s[p.i]) {
				p.i++
			}
			return true
		}
	} else {
		var ok bool
		if symbol, ok = p.scanSymbol(); !ok {
			return false
		}
	}

	if !simpleUnit(symbol) || !p.exponent() {
		return false
	}
	return p.i >= len(p.s) || p.s[p.i] != '{' || p.annotation()
}

func (p *parser) scanSymbol() (string, bool) {
	start := p.i
	for p.i < len(p.s) {
		c := p.s[p.i]
		switch {
		case isLetter(c) || c == '_' || c == '%' || c == '\'' || c == '"' || c == '*' || c == '^':
			p.i++
		case c == '[':
			end := strings.IndexByte(p.s[p.i:], ']')
			if end < 0 {
				return "", false
			}
			p.i += end + 1
		default:
			return p.s[start:p.i], p.i > start
		}
	}
	return p.s[start:p.i], p.i > start
}

func (p *parser) exponent() bool {
	if p.i >= len(p.s) {
		return true
	}
	switch c := p.s[p.i]; {
	case c == '+' || c == '-':
		p.i++
		if p.i >= len(p.s) || !isDigit(p.s[p.i]) {
			return false
		}
	case !isDigit(c):
		return true
	}
	for p.i < len(p.s) && isDigit(p.s[p.i]) {
		p.i++
	}
	return true
}

func (p *parser) annotation() bool {
	// "{", printable ASCII other than braces, "}"
	p.i++
	for p.i < len(p.s) && p.s[p.i] != '}' {
		if c := p.s[p.i]; c < ' ' || c > '~' || c == '{' {
			return false
		}
		p.i++
	}
	if p.i >= len(p.s) {
		return false
	}
	p.i++
	return true
}

// simpleUnit reports whether symbol is an atom, or a prefix followed by a metric atom.
func simpleUnit(symbol string) bool {
	if _, ok := atoms[symbol]; ok {
		return true
	}
	for _, prefix := range prefixes {
		if rest, found := strings.CutPrefix(symbol, prefix); found && atoms[rest] {
			return true
		}
	}
	return false
}

func isDigit(c byte) bool  { return c >= '0' && c <= '9' }
func isLetter(c byte) bool { return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' }

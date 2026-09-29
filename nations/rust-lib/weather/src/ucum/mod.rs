//! Validates UCUM codes in the case-sensitive form. It checks validity only.
//! There is no conversion. The unit list in `atoms_gen.rs` comes from
//! `domains/weather/standards/ucum`, through `examples/gen_ucum.rs`.

mod atoms_gen;

use crate::unit::UnitValidator;
use atoms_gen::{ATOMS, PREFIXES};

/// A [`UnitValidator`] for UCUM codes (case-sensitive form). It holds no state.
#[derive(Debug, Clone, Copy, Default)]
pub struct Validator;

impl UnitValidator for Validator {
    fn is_valid(&self, unit: &str) -> bool {
        is_valid(unit)
    }
}

/// Reports whether `unit` is a valid UCUM code (case-sensitive form).
pub fn is_valid(unit: &str) -> bool {
    if unit.is_empty() {
        return false;
    }
    let mut parser = Parser { s: unit, i: 0 };
    if parser.byte() == Some(b'/') {
        parser.i += 1;
    }
    parser.term() && parser.i == unit.len()
}

/// Every atom code in the unit list, for tests.
pub fn atoms() -> impl Iterator<Item = &'static str> {
    ATOMS.iter().map(|&(code, _)| code)
}

/// A recursive descent parser over the UCUM grammar.
/// See `domains/weather/standards/ucum/README.MD`.
///
/// It walks bytes. Every delimiter it looks for is ASCII, so each slice it takes starts and
/// ends on a character boundary.
struct Parser<'a> {
    s: &'a str,
    i: usize,
}

impl Parser<'_> {
    fn byte(&self) -> Option<u8> {
        self.s.as_bytes().get(self.i).copied()
    }

    fn term(&mut self) -> bool {
        if !self.component() {
            return false;
        }
        while matches!(self.byte(), Some(b'.' | b'/')) {
            self.i += 1;
            if !self.component() {
                return false;
            }
        }
        true
    }

    fn component(&mut self) -> bool {
        let Some(c) = self.byte() else {
            return false;
        };
        match c {
            b'(' => {
                self.i += 1;
                if !self.term() || self.byte() != Some(b')') {
                    return false;
                }
                self.i += 1;
                return true;
            }
            b'{' => return self.annotation(),
            _ => {}
        }

        let symbol = if c.is_ascii_digit() {
            // "10*" and "10^" are atoms, and any other digit run is a plain factor.
            let rest = &self.s[self.i..];
            if rest.starts_with("10*") || rest.starts_with("10^") {
                self.i += 3;
                &rest[..3]
            } else {
                while self.byte().is_some_and(|b| b.is_ascii_digit()) {
                    self.i += 1;
                }
                return true;
            }
        } else {
            match self.scan_symbol() {
                Some(symbol) => symbol,
                None => return false,
            }
        };

        if !simple_unit(symbol) || !self.exponent() {
            return false;
        }
        self.byte() != Some(b'{') || self.annotation()
    }

    fn scan_symbol(&mut self) -> Option<&str> {
        let start = self.i;
        while let Some(c) = self.byte() {
            match c {
                c if c.is_ascii_alphabetic() => self.i += 1,
                b'_' | b'%' | b'\'' | b'"' | b'*' | b'^' => self.i += 1,
                b'[' => {
                    let end = self.s[self.i..].find(']')?;
                    self.i += end + 1;
                }
                _ => break,
            }
        }
        (self.i > start).then(|| &self.s[start..self.i])
    }

    fn exponent(&mut self) -> bool {
        match self.byte() {
            None => return true,
            Some(b'+' | b'-') => {
                self.i += 1;
                if !self.byte().is_some_and(|b| b.is_ascii_digit()) {
                    return false;
                }
            }
            Some(c) if !c.is_ascii_digit() => return true,
            Some(_) => {}
        }
        while self.byte().is_some_and(|b| b.is_ascii_digit()) {
            self.i += 1;
        }
        true
    }

    /// An annotation is `{`, printable ASCII other than braces, then `}`.
    fn annotation(&mut self) -> bool {
        self.i += 1;
        while let Some(c) = self.byte() {
            if c == b'}' {
                self.i += 1;
                return true;
            }
            if !(b' '..=b'~').contains(&c) || c == b'{' {
                return false;
            }
            self.i += 1;
        }
        false
    }
}

/// Whether `symbol` is an atom, or a prefix followed by a metric atom.
fn simple_unit(symbol: &str) -> bool {
    if atom(symbol).is_some() {
        return true;
    }
    PREFIXES.iter().any(|prefix| {
        symbol
            .strip_prefix(prefix)
            .is_some_and(|rest| atom(rest) == Some(true))
    })
}

/// Looks up an atom, returning whether it takes a metric prefix.
fn atom(code: &str) -> Option<bool> {
    ATOMS
        .binary_search_by_key(&code, |&(c, _)| c)
        .ok()
        .map(|i| ATOMS[i].1)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn every_atom_in_the_unit_list_is_valid() {
        let rejected: Vec<&str> = atoms().filter(|code| !is_valid(code)).collect();
        assert!(rejected.is_empty(), "rejected atoms: {rejected:?}");
        assert!(
            atoms().count() >= 300,
            "only {} atoms loaded, expected the full unit list",
            atoms().count()
        );
    }

    #[test]
    fn valid_and_invalid_codes() {
        let cases: &[(&str, bool)] = &[
            ("m/s", true),
            ("/min", true),
            ("km2", true),
            ("s-1", true),
            ("10*3/uL", true),
            ("mm[Hg]", true),
            ("kg.m/s2", true),
            ("(m/s)", true),
            ("{rbc}", true),
            ("mL{total}", true),
            ("hPa", true),
            ("%", true),
            ("[degF]", true),
            ("", false),
            ("degC", false),
            ("celsius", false),
            ("cel", false),
            ("m/", false),
            ("m..s", false),
            ("m s", false),
            ("(m", false),
            ("(m/s)2", false), // an exponent applies to a unit, not to a parenthesized term
            ("m{unclosed", false),
            ("m-", false),
            ("[degF", false),
            ("kBtu_IT[", false),
            ("mé", false),
        ];
        let wrong: Vec<String> = cases
            .iter()
            .filter(|&&(code, want)| is_valid(code) != want)
            .map(|&(code, want)| format!("is_valid({code:?}) should be {want}"))
            .collect();
        assert!(wrong.is_empty(), "{wrong:#?}");
    }

    #[test]
    fn prefixes_apply_only_to_metric_atoms() {
        assert!(is_valid("kg"));
        assert!(is_valid("mm[Hg]") && !is_valid("m[degF]"));
        assert!(!is_valid("k[degF]"));
        assert!(!is_valid("kmin")); // min is not metric
    }

    #[test]
    fn never_panics_on_odd_input() {
        for code in [
            "é", "[é]", "m{é}", "\u{0}", "10", "10*", "10^", "/", "()", "{}", "m[", "🌡",
        ] {
            let _ = is_valid(code);
        }
        assert!(is_valid("10") && is_valid("10*") && is_valid("{}"));
    }

    #[test]
    fn works_through_the_seam() {
        let validator: &dyn UnitValidator = &Validator;
        assert!(validator.is_valid("Cel") && !validator.is_valid("degC"));
    }
}

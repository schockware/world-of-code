// Validates UCUM codes in the case-sensitive form. It checks validity only: there is no conversion.
// The unit list in atoms.gen.ts comes from domains/weather/standards/ucum (npm run gen:ucum).
import { atoms, prefixes } from './atoms.gen.ts';

/** Reports whether `unit` is a valid UCUM code (case-sensitive form). It is a `UnitValidator`. */
export function isValid(unit: string): boolean {
  if (unit === '') return false;
  const p = new Parser(unit);
  if (unit.startsWith('/')) p.i++;
  return p.term() && p.i === unit.length;
}

/** Every atom code in the unit list, for tests. */
export function atomCodes(): string[] {
  return [...atoms.keys()];
}

/**
 * A recursive descent parser over the UCUM grammar. See domains/weather/standards/ucum/README.MD.
 * Each method returns false for "this is not valid" and leaves `i` wherever it stopped.
 * The input is JS UTF-16 text, and every character the grammar allows is ASCII, so any other
 * code unit ends the parse and fails.
 */
class Parser {
  readonly s: string;
  i = 0;

  constructor(s: string) {
    this.s = s;
  }

  private peek(): string {
    return this.s.charAt(this.i); // '' past the end
  }

  term(): boolean {
    if (!this.component()) return false;
    while (this.peek() === '.' || this.peek() === '/') {
      this.i++;
      if (!this.component()) return false;
    }
    return true;
  }

  private component(): boolean {
    if (this.i >= this.s.length) return false;
    const c = this.peek();
    if (c === '(') {
      this.i++;
      if (!this.term() || this.peek() !== ')') return false;
      this.i++;
      return true;
    }
    if (c === '{') return this.annotation();

    let symbol: string;
    if (isDigit(c)) {
      // "10*" and "10^" are atoms, and any other digit run is a plain factor.
      const three = this.s.slice(this.i, this.i + 3);
      if (three === '10*' || three === '10^') {
        symbol = three;
        this.i += 3;
      } else {
        while (isDigit(this.peek())) this.i++;
        return true;
      }
    } else {
      const scanned = this.scanSymbol();
      if (scanned === undefined) return false;
      symbol = scanned;
    }

    if (!simpleUnit(symbol) || !this.exponent()) return false;
    return this.peek() !== '{' || this.annotation();
  }

  private scanSymbol(): string | undefined {
    const start = this.i;
    for (;;) {
      const c = this.peek();
      if (isLetter(c) || c === '_' || c === '%' || c === "'" || c === '"' || c === '*' || c === '^') {
        this.i++;
      } else if (c === '[') {
        const end = this.s.indexOf(']', this.i);
        if (end < 0) return undefined;
        this.i = end + 1;
      } else {
        return this.i > start ? this.s.slice(start, this.i) : undefined;
      }
    }
  }

  private exponent(): boolean {
    const c = this.peek();
    if (c === '+' || c === '-') {
      this.i++;
      if (!isDigit(this.peek())) return false;
    } else if (!isDigit(c)) {
      return true;
    }
    while (isDigit(this.peek())) this.i++;
    return true;
  }

  private annotation(): boolean {
    // "{", printable ASCII other than braces, "}"
    this.i++;
    while (this.i < this.s.length && this.peek() !== '}') {
      const code = this.s.charCodeAt(this.i);
      if (code < 0x20 || code > 0x7e || code === 0x7b) return false;
      this.i++;
    }
    if (this.i >= this.s.length) return false;
    this.i++;
    return true;
  }
}

/** Whether `symbol` is an atom, or a prefix followed by a metric atom. */
function simpleUnit(symbol: string): boolean {
  if (atoms.has(symbol)) return true;
  return prefixes.some((prefix) => symbol.startsWith(prefix) && atoms.get(symbol.slice(prefix.length)) === true);
}

function isDigit(c: string): boolean {
  return c >= '0' && c <= '9' && c.length === 1;
}

function isLetter(c: string): boolean {
  return c.length === 1 && ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'));
}

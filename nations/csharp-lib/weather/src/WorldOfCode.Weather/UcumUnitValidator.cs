using System.Xml.Linq;

namespace WorldOfCode.Weather;

/// <summary>
/// Validates UCUM codes (case-sensitive form) against the shared unit list in
/// <c>domains/weather/standards/ucum</c>. Only validity is checked. There is no conversion.
/// </summary>
public sealed class UcumUnitValidator : IUnitValidator
{
    private sealed record UnitData(HashSet<string> Atoms, HashSet<string> MetricAtoms, List<string> Prefixes);

    private static readonly Lazy<UnitData> Data = new(Load);

    internal static IReadOnlyCollection<string> KnownAtomCodes => Data.Value.Atoms;

    public bool IsValid(string unit)
    {
        var parser = new Parser(unit, Data.Value);
        return parser.ParseMainTerm();
    }

    private static UnitData Load()
    {
        using var stream = typeof(UcumUnitValidator).Assembly.GetManifestResourceStream("ucum-essence.xml")
            ?? throw new InvalidOperationException("The embedded UCUM unit list is missing.");
        var doc = XDocument.Load(stream);
        var atoms = new HashSet<string>(StringComparer.Ordinal);
        var metric = new HashSet<string>(StringComparer.Ordinal);
        var prefixes = new List<string>();

        foreach (var e in doc.Root!.Elements())
        {
            var code = (string?)e.Attribute("Code");
            if (code is null)
            {
                continue;
            }

            switch (e.Name.LocalName)
            {
                case "prefix":
                    prefixes.Add(code);
                    break;
                case "base-unit":
                    atoms.Add(code);
                    metric.Add(code); // SI base units all take prefixes
                    break;
                case "unit":
                    atoms.Add(code);
                    if ((string?)e.Attribute("isMetric") == "yes")
                    {
                        metric.Add(code);
                    }

                    break;
            }
        }

        return new UnitData(atoms, metric, prefixes);
    }

    /// <summary>Recursive descent over the UCUM grammar (see domains/weather/standards/ucum/README.MD).</summary>
    private sealed class Parser(string s, UnitData data)
    {
        private int _i;

        public bool ParseMainTerm()
        {
            if (s.Length == 0)
            {
                return false;
            }

            if (s[0] == '/')
            {
                _i++;
            }

            return Term() && _i == s.Length;
        }

        private bool Term()
        {
            if (!Component())
            {
                return false;
            }

            while (_i < s.Length && (s[_i] == '.' || s[_i] == '/'))
            {
                _i++;
                if (!Component())
                {
                    return false;
                }
            }

            return true;
        }

        private bool Component()
        {
            if (_i >= s.Length)
            {
                return false;
            }

            var c = s[_i];
            if (c == '(')
            {
                _i++;
                if (!Term() || _i >= s.Length || s[_i] != ')')
                {
                    return false;
                }

                _i++;
                return true;
            }

            if (c == '{')
            {
                return Annotation();
            }

            string symbol;
            if (char.IsAsciiDigit(c))
            {
                // "10*" and "10^" are atoms, and any other digit run is a plain factor.
                if (s.AsSpan(_i).StartsWith("10*") || s.AsSpan(_i).StartsWith("10^"))
                {
                    symbol = s.Substring(_i, 3);
                    _i += 3;
                }
                else
                {
                    while (_i < s.Length && char.IsAsciiDigit(s[_i]))
                    {
                        _i++;
                    }

                    return true;
                }
            }
            else if (!ScanSymbol(out symbol))
            {
                return false;
            }

            if (!IsSimpleUnit(symbol))
            {
                return false;
            }

            if (!Exponent())
            {
                return false;
            }

            return _i >= s.Length || s[_i] != '{' || Annotation();
        }

        private bool ScanSymbol(out string symbol)
        {
            var start = _i;
            while (_i < s.Length)
            {
                var c = s[_i];
                if (char.IsAsciiLetter(c) || c is '_' or '%' or '\'' or '"' or '*' or '^')
                {
                    _i++;
                }
                else if (c == '[')
                {
                    var close = s.IndexOf(']', _i);
                    if (close < 0)
                    {
                        symbol = string.Empty;
                        return false;
                    }

                    _i = close + 1;
                }
                else
                {
                    break;
                }
            }

            symbol = s[start.._i];
            return symbol.Length > 0;
        }

        private bool Exponent()
        {
            if (_i >= s.Length)
            {
                return true;
            }

            var c = s[_i];
            if (c is '+' or '-')
            {
                _i++;
                if (_i >= s.Length || !char.IsAsciiDigit(s[_i]))
                {
                    return false;
                }
            }
            else if (!char.IsAsciiDigit(c))
            {
                return true;
            }

            while (_i < s.Length && char.IsAsciiDigit(s[_i]))
            {
                _i++;
            }

            return true;
        }

        private bool Annotation()
        {
            // "{", printable ASCII other than braces, "}"
            _i++;
            while (_i < s.Length && s[_i] != '}')
            {
                if (s[_i] is < ' ' or > '~' or '{')
                {
                    return false;
                }

                _i++;
            }

            if (_i >= s.Length)
            {
                return false;
            }

            _i++;
            return true;
        }

        private bool IsSimpleUnit(string symbol)
        {
            if (data.Atoms.Contains(symbol))
            {
                return true;
            }

            foreach (var prefix in data.Prefixes)
            {
                if (symbol.StartsWith(prefix, StringComparison.Ordinal)
                    && data.MetricAtoms.Contains(symbol[prefix.Length..]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

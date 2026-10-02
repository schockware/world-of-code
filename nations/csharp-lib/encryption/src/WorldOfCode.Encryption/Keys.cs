using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace WorldOfCode.Encryption;

/// <summary>Base of every key. One sealed record per key contract in <c>contracts/domain/schemas/keys</c>.</summary>
public abstract record CipherKey;

/// <summary>A <c>caesar</c> key (<c>keys/caesar.json</c>). Constructs for any shift; <c>ValidateKey</c> enforces 0 to 25.</summary>
public sealed record CaesarKey(int Shift) : CipherKey
{
    internal static bool TryRead(JsonElement json,
        [NotNullWhen(true)] out CipherKey? key,
        [NotNullWhen(false)] out Rejection? rejection)
    {
        key = null;
        rejection = null;

        if (json.ValueKind != JsonValueKind.Object
            || !KeyJson.HasExactlyMembers(json, "shift")
            || !KeyJson.TryReadInteger(json.GetProperty("shift"), out var shift, out rejection))
        {
            rejection ??= new Rejection(Reasons.KeyMalformed);
            return false;
        }

        key = new CaesarKey(shift);
        return true;
    }
}

/// <summary>The empty key for ciphers with no secret: <c>rot13</c> and <c>atbash</c> (ENC-CAES-004).</summary>
public sealed record EmptyKey : CipherKey
{
    private EmptyKey()
    {
    }

    public static EmptyKey Instance { get; } = new();

    internal static bool TryRead(JsonElement json,
        [NotNullWhen(true)] out CipherKey? key,
        [NotNullWhen(false)] out Rejection? rejection)
    {
        if (json.ValueKind == JsonValueKind.Object && KeyJson.HasExactlyMembers(json))
        {
            key = Instance;
            rejection = null;
            return true;
        }

        key = null;
        rejection = new Rejection(Reasons.KeyMalformed);
        return false;
    }
}

/// <summary>Shape checks shared by the key readers (ENC-CIPH-007).</summary>
internal static class KeyJson
{
    /// <summary>True when the object has exactly the named members, each once, and nothing else.</summary>
    public static bool HasExactlyMembers(JsonElement obj, params string[] names)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in obj.EnumerateObject())
        {
            if (!names.Contains(property.Name, StringComparer.Ordinal) || !seen.Add(property.Name))
            {
                return false;
            }
        }

        return seen.Count == names.Length;
    }

    /// <summary>
    /// Reads a JSON integer: a number with no fractional part, so <c>3</c>, <c>3.0</c> and <c>3e0</c> all read as 3.
    /// A non-number or a fraction is <c>key.malformed</c>. An integer too large for <see cref="int"/> is
    /// <c>key.out-of-range</c>, because the spec calls every integer outside a key's range out of range.
    /// </summary>
    public static bool TryReadInteger(JsonElement value, out int integer, [NotNullWhen(false)] out Rejection? rejection)
    {
        integer = 0;
        rejection = null;

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDecimal(out var number))
        {
            rejection = new Rejection(Reasons.KeyMalformed);
            return false;
        }

        if (decimal.Truncate(number) != number)
        {
            rejection = new Rejection(Reasons.KeyMalformed);
            return false;
        }

        if (number < int.MinValue || number > int.MaxValue)
        {
            rejection = new Rejection(Reasons.KeyOutOfRange);
            return false;
        }

        integer = (int)number;
        return true;
    }
}

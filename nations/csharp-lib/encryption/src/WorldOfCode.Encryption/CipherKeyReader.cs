using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace WorldOfCode.Encryption;

/// <summary>
/// Turns a key in its JSON contract shape into a typed <see cref="CipherKey"/>. Shape faults are <c>key.malformed</c>
/// (ENC-CIPH-007); value faults are left to the cipher's <c>ValidateKey</c>, so one key has one answer (ENC-CIPH-005).
/// </summary>
public static class CipherKeyReader
{
    /// <summary>
    /// Reads the key for the named cipher. A name that is unknown, or known but not built here, is reported as a
    /// rejection so the facade can keep one code path; callers normally resolve the name first.
    /// </summary>
    public static bool TryRead(string cipher, JsonElement json,
        [NotNullWhen(true)] out CipherKey? key,
        [NotNullWhen(false)] out Rejection? rejection)
    {
        ArgumentNullException.ThrowIfNull(cipher);

        switch (cipher)
        {
            case CipherIds.Caesar:
                return CaesarKey.TryRead(json, out key, out rejection);
            case CipherIds.Rot13:
            case CipherIds.Atbash:
                return EmptyKey.TryRead(json, out key, out rejection);
            default:
                key = null;
                rejection = new Rejection(CipherIds.IsKnown(cipher) ? Reasons.CipherNotImplemented : Reasons.CipherUnknown);
                return false;
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace WorldOfCode.Encryption;

/// <summary>
/// The untyped door: the three operations by cipher name with the key as JSON, as a conformance vector or a wire
/// format carries them. Resolves the name (ENC-CIPH-006), reads the key (ENC-CIPH-007), then delegates to the
/// typed cipher, so the precedence is name, key, text (ENC-CIPH-008).
/// </summary>
public static class Ciphers
{
    public static ICipher Caesar { get; } = new CaesarCipher();

    public static ICipher Rot13 { get; } = new Rot13Cipher();

    public static ICipher Atbash { get; } = new AtbashCipher();

    private static readonly Dictionary<string, ICipher> Implemented =
        new[] { Caesar, Rot13, Atbash }.ToDictionary(c => c.Name, StringComparer.Ordinal);

    /// <summary>Finds an implemented cipher by its exact name. False for unknown and for not-yet-built names alike.</summary>
    public static bool TryGet(string name, [NotNullWhen(true)] out ICipher? cipher)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (Implemented.TryGetValue(name, out var found))
        {
            cipher = found;
            return true;
        }

        cipher = null;
        return false;
    }

    public static Rejection? ValidateKey(string cipher, JsonElement key) =>
        TryResolve(cipher, key, out var c, out var k, out var rejection) ? c.ValidateKey(k) : rejection;

    public static CipherResult Encrypt(string cipher, JsonElement key, string text) =>
        TryResolve(cipher, key, out var c, out var k, out var rejection)
            ? c.Encrypt(k, text)
            : new CipherResult.Rejected(rejection);

    public static CipherResult Decrypt(string cipher, JsonElement key, string text) =>
        TryResolve(cipher, key, out var c, out var k, out var rejection)
            ? c.Decrypt(k, text)
            : new CipherResult.Rejected(rejection);

    /// <summary>Steps 1 and 2 of the order of checks: the name, then the key's shape.</summary>
    private static bool TryResolve(string name, JsonElement json,
        [NotNullWhen(true)] out ICipher? cipher,
        [NotNullWhen(true)] out CipherKey? key,
        [NotNullWhen(false)] out Rejection? rejection)
    {
        key = null;
        if (!TryGet(name, out cipher))
        {
            rejection = new Rejection(CipherIds.IsKnown(name) ? Reasons.CipherNotImplemented : Reasons.CipherUnknown);
            return false;
        }

        return CipherKeyReader.TryRead(name, json, out key, out rejection);
    }
}

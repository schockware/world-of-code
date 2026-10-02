namespace WorldOfCode.Encryption;

/// <summary>The cipher identifiers in <c>contracts/domain/schemas/cipher-id.json</c>. Case-sensitive (ENC-CIPH-006).</summary>
public static class CipherIds
{
    public const string Caesar = "caesar";
    public const string Rot13 = "rot13";
    public const string Atbash = "atbash";
    public const string Affine = "affine";
    public const string AutokeyCiphertext = "autokey-ciphertext";
    public const string AutokeyPlaintext = "autokey-plaintext";
    public const string Skytale = "skytale";
    public const string EnigmaM3 = "enigma-m3";
    public const string M209 = "m209";

    /// <summary>Every name the core set defines, whether or not this library implements it yet.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        Caesar, Rot13, Atbash, Affine, AutokeyCiphertext, AutokeyPlaintext, Skytale, EnigmaM3, M209,
    ];

    public static bool IsKnown(string name) => All.Contains(name, StringComparer.Ordinal);
}

namespace WorldOfCode.Encryption;

/// <summary>
/// One named cipher offering exactly the three operations the spec allows (ENC-CIPH-001).
/// Implementations are stateless and their operations are pure (ENC-CIPH-009).
/// </summary>
public interface ICipher
{
    /// <summary>The cipher's identifier from <c>cipher-id.json</c>, for example <c>caesar</c>.</summary>
    string Name { get; }

    /// <summary>Checks the key's shape and values. Returns <see langword="null"/> when the key is valid.</summary>
    Rejection? ValidateKey(CipherKey key);

    /// <summary>Encrypts plaintext (A-Z only) with the key.</summary>
    CipherResult Encrypt(CipherKey key, string text);

    /// <summary>Decrypts ciphertext (A-Z only) with the key.</summary>
    CipherResult Decrypt(CipherKey key, string text);
}

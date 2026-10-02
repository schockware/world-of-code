namespace WorldOfCode.Encryption;

/// <summary>
/// Shared engine for ciphers over A-Z. Enforces the order of checks (key, then text; ENC-CIPH-008) and that
/// <c>Encrypt</c> and <c>Decrypt</c> judge a key exactly as <c>ValidateKey</c> does (ENC-CIPH-005).
/// </summary>
internal abstract class LetterCipher<TKey> : ICipher
    where TKey : CipherKey
{
    public abstract string Name { get; }

    /// <summary>Checks the key's values. Shape is already right, because the key is a <typeparamref name="TKey"/>.</summary>
    protected abstract Rejection? ValidateKeyValues(TKey key);

    protected abstract string EncryptLetters(TKey key, string text);

    protected abstract string DecryptLetters(TKey key, string text);

    public Rejection? ValidateKey(CipherKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return key is TKey typed ? ValidateKeyValues(typed) : new Rejection(Reasons.KeyMalformed);
    }

    public CipherResult Encrypt(CipherKey key, string text) => Run(key, text, EncryptLetters);

    public CipherResult Decrypt(CipherKey key, string text) => Run(key, text, DecryptLetters);

    private CipherResult Run(CipherKey key, string text, Func<TKey, string, string> operation)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (ValidateKey(key) is { } keyRejection)
        {
            return new CipherResult.Rejected(keyRejection);
        }

        if (Letters.Check(text) is { } textRejection)
        {
            return new CipherResult.Rejected(textRejection);
        }

        return new CipherResult.Ok(operation((TKey)key, text));
    }
}

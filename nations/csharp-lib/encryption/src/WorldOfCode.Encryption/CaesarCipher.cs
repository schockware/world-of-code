namespace WorldOfCode.Encryption;

/// <summary><c>caesar</c>: each letter moves <c>Shift</c> places forward (ENC-CAES-001 to 003).</summary>
internal sealed class CaesarCipher : LetterCipher<CaesarKey>
{
    public override string Name => CipherIds.Caesar;

    protected override Rejection? ValidateKeyValues(CaesarKey key) =>
        key.Shift is < 0 or >= Letters.Count ? new Rejection(Reasons.KeyOutOfRange) : null;

    protected override string EncryptLetters(CaesarKey key, string text) =>
        Letters.Map(text, x => (x + key.Shift) % Letters.Count);

    // C#'s % keeps the dividend's sign, so add 26 first to stay in 0..25.
    protected override string DecryptLetters(CaesarKey key, string text) =>
        Letters.Map(text, y => (y - key.Shift + Letters.Count) % Letters.Count);
}

/// <summary><c>rot13</c>: the fixed shift of 13, its own inverse (ENC-CAES-004, 005).</summary>
internal sealed class Rot13Cipher : LetterCipher<EmptyKey>
{
    public override string Name => CipherIds.Rot13;

    protected override Rejection? ValidateKeyValues(EmptyKey key) => null;

    protected override string EncryptLetters(EmptyKey key, string text) => Rotate(text);

    protected override string DecryptLetters(EmptyKey key, string text) => Rotate(text);

    private static string Rotate(string text) => Letters.Map(text, x => (x + 13) % Letters.Count);
}

/// <summary><c>atbash</c>: the reversed alphabet, its own inverse (ENC-CAES-004, 006).</summary>
internal sealed class AtbashCipher : LetterCipher<EmptyKey>
{
    public override string Name => CipherIds.Atbash;

    protected override Rejection? ValidateKeyValues(EmptyKey key) => null;

    protected override string EncryptLetters(EmptyKey key, string text) => Reverse(text);

    protected override string DecryptLetters(EmptyKey key, string text) => Reverse(text);

    private static string Reverse(string text) => Letters.Map(text, x => Letters.Count - 1 - x);
}

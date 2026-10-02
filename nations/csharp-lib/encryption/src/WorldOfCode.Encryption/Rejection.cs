namespace WorldOfCode.Encryption;

/// <summary>Why an input was rejected. <see cref="Reason"/> is one of the codes defined by the encryption specs.</summary>
public sealed record Rejection(string Reason);

/// <summary>Reason codes. Codes marked provisional are not yet backed by a spec requirement.</summary>
public static class Reasons
{
    public const string CipherUnknown = "cipher.unknown";
    public const string KeyMalformed = "key.malformed";
    public const string KeyOutOfRange = "key.out-of-range";
    public const string TextEmpty = "text.empty";
    public const string TextNotLetters = "text.not-letters";

    // Provisional: the name is in cipher-id.json, but this nation has not built that cipher yet.
    public const string CipherNotImplemented = "cipher.not-implemented";
}

/// <summary>Thrown by <see cref="CipherResult.TextOrThrow"/> for callers who prefer to fail fast.</summary>
public sealed class CipherRejectedException(Rejection rejection) : Exception(rejection.Reason)
{
    public Rejection Rejection { get; } = rejection;
}

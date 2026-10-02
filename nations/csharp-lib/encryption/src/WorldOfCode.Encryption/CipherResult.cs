namespace WorldOfCode.Encryption;

/// <summary>
/// The result of <c>encrypt</c> or <c>decrypt</c>: either <see cref="Ok"/> with text, or <see cref="Rejected"/> with a reason.
/// The hierarchy is closed, so a switch over it is exhaustive and nothing else can be carried (ENC-CIPH-002).
/// </summary>
public abstract record CipherResult
{
    private CipherResult()
    {
    }

    /// <summary>The operation succeeded and <see cref="Text"/> is its output, A-Z only.</summary>
    public sealed record Ok(string Text) : CipherResult;

    /// <summary>The input was rejected. No text is produced.</summary>
    public sealed record Rejected(Rejection Rejection) : CipherResult;

    public bool IsOk => this is Ok;

    /// <summary>The output text, or a <see cref="CipherRejectedException"/> when the input was rejected.</summary>
    public string TextOrThrow() => this switch
    {
        Ok ok => ok.Text,
        Rejected rejected => throw new CipherRejectedException(rejected.Rejection),
        _ => throw new InvalidOperationException("CipherResult is a closed hierarchy."),
    };
}

namespace WorldOfCode.Encryption;

/// <summary>The text a cipher works on: one or more of A-Z, numbered A=0 to Z=25 (ENC-TEXT).</summary>
internal static class Letters
{
    public const int Count = 26;

    /// <summary>Returns <c>text.empty</c>, <c>text.not-letters</c>, or <see langword="null"/> when the text is letters.</summary>
    public static Rejection? Check(string text)
    {
        if (text.Length == 0)
        {
            return new Rejection(Reasons.TextEmpty);
        }

        foreach (var c in text)
        {
            if (c is < 'A' or > 'Z')
            {
                return new Rejection(Reasons.TextNotLetters);
            }
        }

        return null;
    }

    /// <summary>Applies a letter-number function to every letter. Output has exactly the input's length (ENC-TEXT-004).</summary>
    public static string Map(string text, Func<int, int> letter) =>
        string.Create(text.Length, (text, letter), static (span, state) =>
        {
            var (source, f) = state;
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = ToLetter(f(ToNumber(source[i])));
            }
        });

    public static int ToNumber(char letter) => letter - 'A';

    public static char ToLetter(int number) => (char)('A' + number);
}

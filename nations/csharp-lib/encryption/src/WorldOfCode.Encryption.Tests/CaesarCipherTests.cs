using System.Text.Json;

namespace WorldOfCode.Encryption.Tests;

/// <summary>Behavior the vectors cannot express as data: the typed door, round trips over every key, and the public surface.</summary>
public class CaesarCipherTests
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly string[] ExpectedMethods = ["Decrypt", "Encrypt", "ValidateKey"];
    private static readonly string[] ExpectedProperties = ["Name"];

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(13)]
    [InlineData(25)]
    public void TypedDoorRoundTripsEveryShift(int shift)
    {
        var key = new CaesarKey(shift);
        var ciphertext = Ciphers.Caesar.Encrypt(key, Alphabet).TextOrThrow();

        Assert.Equal(Alphabet.Length, ciphertext.Length);
        Assert.Equal(Alphabet, Ciphers.Caesar.Decrypt(key, ciphertext).TextOrThrow());
    }

    [Fact]
    public void AllTwentySixShiftsRoundTrip()
    {
        for (var shift = 0; shift < 26; shift++)
        {
            var key = new CaesarKey(shift);
            Assert.Null(Ciphers.Caesar.ValidateKey(key));
            Assert.Equal("ATTACKATDAWN", Ciphers.Caesar.Decrypt(key, Ciphers.Caesar.Encrypt(key, "ATTACKATDAWN").TextOrThrow()).TextOrThrow());
        }
    }

    [Fact]
    public void Rot13AndAtbashAreTheirOwnInverse()
    {
        foreach (var cipher in new[] { Ciphers.Rot13, Ciphers.Atbash })
        {
            var once = cipher.Encrypt(EmptyKey.Instance, Alphabet).TextOrThrow();
            Assert.Equal(Alphabet, cipher.Encrypt(EmptyKey.Instance, once).TextOrThrow());
            Assert.Equal(once, cipher.Decrypt(EmptyKey.Instance, Alphabet).TextOrThrow());
        }
    }

    [Fact]
    public void TypedKeyOfAnotherCipherIsMalformed()
    {
        Assert.Equal(Reasons.KeyMalformed, Ciphers.Caesar.ValidateKey(EmptyKey.Instance)?.Reason);
        Assert.Equal(Reasons.KeyMalformed, Ciphers.Rot13.ValidateKey(new CaesarKey(13))?.Reason);
        Assert.Equal(Reasons.KeyMalformed, Ciphers.Atbash.ValidateKey(new CaesarKey(0))?.Reason);
    }

    [Fact]
    public void OutOfRangeTypedKeyIsRejectedTheSameWayEverywhere()
    {
        var key = new CaesarKey(26);

        Assert.Equal(Reasons.KeyOutOfRange, Ciphers.Caesar.ValidateKey(key)?.Reason);
        Assert.Equal(Reasons.KeyOutOfRange, Rejected(Ciphers.Caesar.Encrypt(key, "HELLO")));
        Assert.Equal(Reasons.KeyOutOfRange, Rejected(Ciphers.Caesar.Decrypt(key, "HELLO")));
    }

    [Fact]
    public void TextOrThrowThrowsWithTheReason()
    {
        var ex = Assert.Throws<CipherRejectedException>(() => Ciphers.Caesar.Encrypt(new CaesarKey(3), "hello").TextOrThrow());
        Assert.Equal(Reasons.TextNotLetters, ex.Rejection.Reason);
        Assert.Equal(Reasons.TextNotLetters, ex.Message);
    }

    [Fact]
    public void NullArgumentsAreProgrammingErrors()
    {
        Assert.Throws<ArgumentNullException>(() => Ciphers.Caesar.Encrypt(null!, "HELLO"));
        Assert.Throws<ArgumentNullException>(() => Ciphers.Caesar.Encrypt(new CaesarKey(3), null!));
        Assert.Throws<ArgumentNullException>(() => Ciphers.TryGet(null!, out _));
    }

    [Fact]
    public void KnownButUnimplementedCipherIsNotUnknown()
    {
        using var key = JsonDocument.Parse("""{"a": 5, "b": 8}""");

        Assert.False(Ciphers.TryGet(CipherIds.Affine, out _));
        Assert.Equal(Reasons.CipherNotImplemented, Rejected(Ciphers.Encrypt(CipherIds.Affine, key.RootElement, "HELLO")));
        Assert.Equal(Reasons.CipherUnknown, Rejected(Ciphers.Encrypt("Affine", key.RootElement, "HELLO")));
    }

    [Fact]
    public void JsonIntegersAreReadByValueNotSpelling()
    {
        using var whole = JsonDocument.Parse("""{"shift": 3.0}""");
        using var exponent = JsonDocument.Parse("""{"shift": 1e2}""");
        using var huge = JsonDocument.Parse("""{"shift": 99999999999}""");

        Assert.Equal("D", Ciphers.Encrypt(CipherIds.Caesar, whole.RootElement, "A").TextOrThrow());
        Assert.Equal(Reasons.KeyOutOfRange, Ciphers.ValidateKey(CipherIds.Caesar, exponent.RootElement)?.Reason);
        Assert.Equal(Reasons.KeyOutOfRange, Ciphers.ValidateKey(CipherIds.Caesar, huge.RootElement)?.Reason);
    }

    [Fact]
    public void CipherInterfaceOffersExactlyTheThreeOperations()
    {
        // ENC-CIPH-001: validateKey, encrypt, decrypt, plus the name. Keyspace and enumeration must not appear.
        var methods = typeof(ICipher).GetMethods().Where(m => !m.IsSpecialName).Select(m => m.Name).Order().ToList();
        var properties = typeof(ICipher).GetProperties().Select(p => p.Name).ToList();

        Assert.Equal(ExpectedMethods, methods);
        Assert.Equal(ExpectedProperties, properties);
    }

    [Fact]
    public void EveryImplementedCipherReportsItsContractName()
    {
        Assert.Equal(CipherIds.Caesar, Ciphers.Caesar.Name);
        Assert.Equal(CipherIds.Rot13, Ciphers.Rot13.Name);
        Assert.Equal(CipherIds.Atbash, Ciphers.Atbash.Name);
        Assert.True(Ciphers.TryGet(CipherIds.Caesar, out var found));
        Assert.Same(Ciphers.Caesar, found);
    }

    private static string Rejected(CipherResult result) => Assert.IsType<CipherResult.Rejected>(result).Rejection.Reason;
}

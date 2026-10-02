using System.Text.Json;
using Xunit.Abstractions;

namespace WorldOfCode.Encryption.Tests;

/// <summary>Runs every vector in the linked files from domains/encryption/specs/core/conformance. Passing them is the scoreboard.</summary>
public class ConformanceTests(ITestOutputHelper output)
{
    private static readonly string VectorDir = Path.Combine(AppContext.BaseDirectory, "conformance");

    public static TheoryData<string, string> Vectors()
    {
        var data = new TheoryData<string, string>();
        foreach (var (file, vector) in AllVectors())
        {
            if (IsRunnable(vector))
            {
                data.Add(file, vector.GetProperty("id").GetString()!);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Vectors))]
    public void Vector(string file, string id)
    {
        var vector = AllVectors().Single(v => v.File == file && v.Vector.GetProperty("id").GetString() == id).Vector;
        var input = vector.GetProperty("input");
        var expect = vector.GetProperty("expect");
        var cipher = input.GetProperty("cipher").GetString()!;
        var key = input.GetProperty("key");

        switch (vector.GetProperty("operation").GetString())
        {
            case "validateKey":
                AssertOutcome(expect, Ciphers.ValidateKey(cipher, key));
                break;

            case "encrypt":
                var plaintext = input.GetProperty("text").GetString()!;
                var encrypted = Ciphers.Encrypt(cipher, key, plaintext);
                AssertOutcome(expect, encrypted);
                if (encrypted is CipherResult.Ok ok)
                {
                    // ENC-CIPH-010: every encrypt vector decrypts back to its input.
                    Assert.Equal(plaintext, Ciphers.Decrypt(cipher, key, ok.Text).TextOrThrow());
                }

                break;

            case "decrypt":
                AssertOutcome(expect, Ciphers.Decrypt(cipher, key, input.GetProperty("text").GetString()!));
                break;

            case var other:
                Assert.Fail($"Unknown operation '{other}'.");
                break;
        }
    }

    /// <summary>A skip is never silent: every vector left out of the theory data names a known cipher this nation has not built.</summary>
    [Fact]
    public void SkippedVectorsAreOnlyForUnimplementedCiphers()
    {
        var skipped = AllVectors().Where(v => !IsRunnable(v.Vector)).ToList();
        foreach (var (file, vector) in skipped)
        {
            var id = vector.GetProperty("id").GetString();
            var cipher = vector.GetProperty("input").GetProperty("cipher").GetString()!;
            output.WriteLine($"skipped {file}#{id} ({cipher} not implemented)");
            Assert.True(CipherIds.IsKnown(cipher) && !Ciphers.TryGet(cipher, out _),
                $"{file}#{id} was skipped for a reason other than an unimplemented cipher");
        }

        output.WriteLine($"{skipped.Count} skipped");
    }

    private static bool IsRunnable(JsonElement vector)
    {
        var cipher = vector.GetProperty("input").GetProperty("cipher").GetString()!;
        return Ciphers.TryGet(cipher, out _) || !CipherIds.IsKnown(cipher);
    }

    private static IEnumerable<(string File, JsonElement Vector)> AllVectors()
    {
        foreach (var path in Directory.GetFiles(VectorDir, "*.json").Order())
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            foreach (var vector in doc.RootElement.GetProperty("vectors").EnumerateArray())
            {
                yield return (Path.GetFileName(path), vector.Clone());
            }
        }
    }

    private static void AssertOutcome(JsonElement expect, Rejection? rejection)
    {
        if (expect.GetProperty("outcome").GetString() == "ok")
        {
            Assert.True(rejection is null, $"expected ok but rejected with {rejection?.Reason}");
        }
        else
        {
            Assert.NotNull(rejection);
            Assert.Equal(expect.GetProperty("reason").GetString(), rejection.Reason);
        }
    }

    private static void AssertOutcome(JsonElement expect, CipherResult result)
    {
        if (expect.GetProperty("outcome").GetString() == "ok")
        {
            var ok = Assert.IsType<CipherResult.Ok>(result);
            Assert.Equal(expect.GetProperty("text").GetString(), ok.Text);
        }
        else
        {
            var rejected = Assert.IsType<CipherResult.Rejected>(result);
            Assert.Equal(expect.GetProperty("reason").GetString(), rejected.Rejection.Reason);
        }
    }
}

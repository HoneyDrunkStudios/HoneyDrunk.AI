using Xunit;

namespace HoneyDrunk.AI.Providers.Anthropic.Tests;

/// <summary>Secret redaction for errors that leave the provider.</summary>
public sealed class SecretRedactionTests
{
    private const string Secret = "sk-secret-value";

    /// <summary>Errors that do not mention the secret pass through unchanged.</summary>
    [Fact]
    public void Leaves_unrelated_errors_unchanged()
    {
        var error = new InvalidOperationException("rate limited");

        Assert.Same(error, SecretRedaction.Redact(error, Secret));
    }

    /// <summary>An error whose message contains the secret is replaced.</summary>
    [Fact]
    public void Replaces_error_whose_message_contains_secret()
    {
        var redacted = SecretRedaction.Redact(new InvalidOperationException($"bad key {Secret}"), Secret);

        AssertClean(redacted);
    }

    /// <summary>An error whose inner exception contains the secret is replaced too.</summary>
    [Fact]
    public void Replaces_error_whose_inner_message_contains_secret()
    {
        var redacted = SecretRedaction.Redact(new HttpRequestException("failed", new InvalidOperationException($"header {Secret}")), Secret);

        AssertClean(redacted);
    }

    /// <summary>A blank secret never redacts.</summary>
    [Fact]
    public void Blank_secret_leaves_error_unchanged()
    {
        var error = new InvalidOperationException("anything");

        Assert.Same(error, SecretRedaction.Redact(error, string.Empty));
    }

    private static void AssertClean(Exception redacted)
    {
        for (var current = redacted; current is not null; current = current.InnerException)
        {
            Assert.DoesNotContain(Secret, current.Message, StringComparison.Ordinal);
        }

        Assert.Equal("Anthropic request failed.", redacted.Message);
        Assert.Null(redacted.InnerException);
    }
}

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

    /// <summary>A secret in any inner exception of an aggregate is caught, not only the first.</summary>
    [Fact]
    public void Replaces_aggregate_whose_second_inner_contains_secret()
    {
        var aggregate = new AggregateException("several", new InvalidOperationException("first"), new InvalidOperationException($"second {Secret}"));

        AssertClean(SecretRedaction.Redact(aggregate, Secret));
    }

    /// <summary>A secret nested under a non-first inner exception of an aggregate is caught.</summary>
    [Fact]
    public void Replaces_aggregate_with_secret_nested_under_second_inner()
    {
        var nested = new HttpRequestException("send failed", new InvalidOperationException($"header {Secret}"));
        var aggregate = new AggregateException("several", new InvalidOperationException("first"), nested);

        AssertClean(SecretRedaction.Redact(aggregate, Secret));
    }

    /// <summary>Errors thrown while starting, reading or closing a stream are all redacted.</summary>
    /// <param name="failAt">Where the fake stream fails.</param>
    [Theory]
    [InlineData("create")]
    [InlineData("move")]
    [InlineData("dispose")]
    public async Task Redacting_stream_covers_create_move_and_dispose(string failAt)
    {
        var error = await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await foreach (var item in SecretRedaction.RedactStream(() => new FailingStream(failAt, Secret), Secret, CancellationToken.None))
            {
                Assert.Equal(1, item);
            }
        });

        AssertClean(error);
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

    /// <summary>An async stream that fails, with the secret in the message, at a chosen point.</summary>
    private sealed class FailingStream(string failAt, string secret) : IAsyncEnumerable<int>, IAsyncEnumerator<int>
    {
        private bool yielded;

        public int Current => 1;

        public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            failAt == "create" ? throw new InvalidOperationException($"create {secret}") : this;

        public ValueTask<bool> MoveNextAsync()
        {
            if (failAt == "move")
            {
                throw new InvalidOperationException($"move {secret}");
            }

            var moved = !this.yielded;
            this.yielded = true;
            return ValueTask.FromResult(moved);
        }

        public ValueTask DisposeAsync() =>
            failAt == "dispose" ? throw new InvalidOperationException($"dispose {secret}") : ValueTask.CompletedTask;
    }
}

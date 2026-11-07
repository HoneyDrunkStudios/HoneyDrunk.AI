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

    /// <summary>Cancellation the caller asked for passes through unchanged.</summary>
    [Fact]
    public void Translate_passes_caller_cancellation_through()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.Null(SecretRedaction.Translate(new OperationCanceledException(cts.Token), Secret, cts.Token));
    }

    /// <summary>An SDK timeout surfaces as a timeout, not as cancellation, when the caller did not cancel.</summary>
    [Fact]
    public void Translate_turns_sdk_timeout_into_timeout_exception()
    {
        var translated = SecretRedaction.Translate(new TaskCanceledException("timed out", new TimeoutException("after 600s")), Secret, CancellationToken.None);

        Assert.IsType<TimeoutException>(translated);
    }

    /// <summary>Unrelated errors pass through unchanged; errors carrying the secret are replaced.</summary>
    [Fact]
    public void Translate_redacts_only_errors_that_carry_the_secret()
    {
        Assert.Null(SecretRedaction.Translate(new InvalidOperationException("rate limited"), Secret, CancellationToken.None));
        AssertClean(SecretRedaction.Translate(new InvalidOperationException($"key {Secret}"), Secret, CancellationToken.None)!);
    }

    /// <summary>A timeout while reading a stream surfaces as a timeout.</summary>
    [Fact]
    public async Task Redacting_stream_reports_timeout_not_cancellation()
    {
        await Assert.ThrowsAsync<TimeoutException>(async () =>
        {
            await foreach (var item in SecretRedaction.RedactStream(() => new FailingStream("timeout", Secret), Secret, CancellationToken.None))
            {
                Assert.Equal(1, item);
            }
        });
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

            if (failAt == "timeout")
            {
                throw new TaskCanceledException("timed out", new TimeoutException("after 600s"));
            }

            var moved = !this.yielded;
            this.yielded = true;
            return ValueTask.FromResult(moved);
        }

        public ValueTask DisposeAsync() =>
            failAt == "dispose" ? throw new InvalidOperationException($"dispose {secret}") : ValueTask.CompletedTask;
    }
}

using System.Runtime.CompilerServices;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>Keeps the API key out of errors that leave the provider.</summary>
internal static class SecretRedaction
{
    /// <summary>The message used when an error had to be replaced.</summary>
    public const string RedactedMessage = "Anthropic request failed.";

    /// <summary>The message used when an SDK timeout is reported.</summary>
    public const string TimedOutMessage = "Anthropic request timed out.";

    /// <summary>
    /// Returns <paramref name="error"/> unchanged unless its full text (message, every inner exception
    /// and stack) contains <paramref name="secret"/>. Then it returns a replacement with no inner exception,
    /// so the secret cannot travel further.
    /// </summary>
    /// <param name="error">The error to check.</param>
    /// <param name="secret">The secret that must not appear.</param>
    /// <returns>The original error or a redacted replacement.</returns>
    public static Exception Redact(Exception error, string secret)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (string.IsNullOrEmpty(secret))
        {
            return error;
        }

        // ToString() covers the whole tree: every inner exception, including all of an AggregateException's.
        return error.ToString().Contains(secret, StringComparison.Ordinal)
            ? new InvalidOperationException(RedactedMessage)
            : error;
    }

    /// <summary>
    /// Decides what a provider error becomes on its way out. Returns <see langword="null"/> when the
    /// original should be rethrown unchanged: cancellation the caller asked for, or an error that does
    /// not carry the secret. Returns a replacement for an SDK timeout (so it is not mistaken for the
    /// caller cancelling) or for an error whose text carries the secret.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="secret">The secret that must not appear in errors.</param>
    /// <param name="ct">The caller's cancellation token.</param>
    /// <returns>A replacement to throw, or <see langword="null"/> to rethrow the original.</returns>
    public static Exception? Translate(Exception error, string secret, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error is OperationCanceledException && ct.IsCancellationRequested)
        {
            return null;
        }

        if (error is TaskCanceledException { InnerException: TimeoutException })
        {
            return new TimeoutException(TimedOutMessage);
        }

        var redacted = Redact(error, secret);
        return ReferenceEquals(redacted, error) ? null : redacted;
    }

    /// <summary>
    /// Enumerates a stream, translating any error thrown while it is created, advanced or disposed
    /// (see <see cref="Translate"/>).
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="create">Creates the stream.</param>
    /// <param name="secret">The secret that must not appear in errors.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The same items, in order.</returns>
    public static async IAsyncEnumerable<T> RedactStream<T>(Func<IAsyncEnumerable<T>> create, string secret, [EnumeratorCancellation] CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(create);
        var enumerator = Guard(() => create().GetAsyncEnumerator(ct), secret, ct);
        try
        {
            while (true)
            {
                bool moved;
                try
                {
                    moved = await enumerator.MoveNextAsync().ConfigureAwait(false);
                }
                catch (Exception error) when (Translate(error, secret, ct) is { } replacement)
                {
                    throw replacement;
                }

                if (!moved)
                {
                    yield break;
                }

                yield return enumerator.Current;
            }
        }
        finally
        {
            await DisposeTranslatedAsync(enumerator, secret, ct).ConfigureAwait(false);
        }
    }

    private static async ValueTask DisposeTranslatedAsync<T>(IAsyncEnumerator<T> enumerator, string secret, CancellationToken ct)
    {
        try
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception error) when (Translate(error, secret, ct) is { } replacement)
        {
            throw replacement;
        }
    }

    private static TResult Guard<TResult>(Func<TResult> action, string secret, CancellationToken ct)
    {
        try
        {
            return action();
        }
        catch (Exception error) when (Translate(error, secret, ct) is { } replacement)
        {
            throw replacement;
        }
    }
}

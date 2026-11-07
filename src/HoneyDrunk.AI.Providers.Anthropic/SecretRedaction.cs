using System.Runtime.CompilerServices;

namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>Keeps the API key out of errors that leave the provider.</summary>
internal static class SecretRedaction
{
    /// <summary>The message used when an error had to be replaced.</summary>
    public const string RedactedMessage = "Anthropic request failed.";

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
    /// Enumerates a stream, redacting any error thrown while it is created, advanced or disposed.
    /// Cancellation passes through unchanged.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="create">Creates the stream.</param>
    /// <param name="secret">The secret that must not appear in errors.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The same items, in order.</returns>
    public static async IAsyncEnumerable<T> RedactStream<T>(Func<IAsyncEnumerable<T>> create, string secret, [EnumeratorCancellation] CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(create);
        var enumerator = Guard(() => create().GetAsyncEnumerator(ct), secret);
        try
        {
            while (true)
            {
                bool moved;
                try
                {
                    moved = await enumerator.MoveNextAsync().ConfigureAwait(false);
                }
                catch (Exception error) when (error is not OperationCanceledException)
                {
                    throw Redact(error, secret);
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
            await DisposeRedactedAsync(enumerator, secret).ConfigureAwait(false);
        }
    }

    private static async ValueTask DisposeRedactedAsync<T>(IAsyncEnumerator<T> enumerator, string secret)
    {
        try
        {
            await enumerator.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            throw Redact(error, secret);
        }
    }

    private static TResult Guard<TResult>(Func<TResult> action, string secret)
    {
        try
        {
            return action();
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            throw Redact(error, secret);
        }
    }
}

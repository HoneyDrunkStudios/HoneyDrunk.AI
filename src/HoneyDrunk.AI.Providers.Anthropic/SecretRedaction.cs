namespace HoneyDrunk.AI.Providers.Anthropic;

/// <summary>Keeps the API key out of errors that leave the provider.</summary>
internal static class SecretRedaction
{
    /// <summary>The message used when an error had to be replaced.</summary>
    public const string RedactedMessage = "Anthropic request failed.";

    /// <summary>
    /// Returns <paramref name="error"/> unchanged unless its message, or any inner exception's message,
    /// contains <paramref name="secret"/>. Then it returns a replacement with no inner exception,
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

        for (var current = error; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains(secret, StringComparison.Ordinal))
            {
                return new InvalidOperationException(RedactedMessage);
            }
        }

        return error;
    }
}

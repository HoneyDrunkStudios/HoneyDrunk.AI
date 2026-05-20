namespace HoneyDrunk.AI.Abstractions;

/// <summary>Represents a single chat message.</summary>
/// <param name="Role">The message role.</param>
/// <param name="Content">The message content.</param>
/// <param name="Name">Optional speaker or tool name.</param>
public sealed record ChatMessage(ChatRole Role, string Content, string? Name = null);

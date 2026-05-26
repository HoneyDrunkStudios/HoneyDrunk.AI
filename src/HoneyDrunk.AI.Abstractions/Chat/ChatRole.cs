namespace HoneyDrunk.AI.Abstractions.Chat;

/// <summary>Role assigned to a chat message.</summary>
public enum ChatRole
{
    /// <summary>System instruction message.</summary>
    System,

    /// <summary>User-authored message.</summary>
    User,

    /// <summary>Assistant-authored message.</summary>
    Assistant,

    /// <summary>Tool-authored message.</summary>
    Tool,
}

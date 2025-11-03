namespace NebulaModel.DataStructures.Chat;

using System;

/// <summary>
/// Represents a raw, data-only chat message, independent of any UI framework.
/// </summary>
public class RawChatMessage
{
    /// <summary>
    /// Gets or sets the raw message text, which may include rich text tags.
    /// </summary>
    public string MessageText { get; set; }

    /// <summary>
    /// Gets or sets the type of the chat message (e.g., Player, System, Command).
    /// </summary>
    public ChatMessageType MessageType { get; set; }

    /// <summary>
    /// Gets or sets the name of the player who sent the message.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the time the message was sent or received.
    /// </summary>
    public DateTime Timestamp { get; set; }
}

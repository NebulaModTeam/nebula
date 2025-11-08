#region

using System;

#endregion

namespace NebulaWorld.Chat.Commands;

/// <summary>
///     Describes a chat command
/// </summary>
public interface IChatCommandHandler
{
    void Execute(ChatService chatService, string[] parameters);

    /// <summary>
    ///     Provide command description without mentioning command name
    /// </summary>
    string GetDescription();

    /// <summary>
    ///     Provide argument usage (If needed) without starting with command name
    /// </summary>
    string[] GetUsage();
}

public class ChatCommandUsageException(string message) : Exception(message)
{
}

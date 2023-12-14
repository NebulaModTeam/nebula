#region

using System;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

/// <summary>
///     Describes a chat command
/// </summary>
public interface IChatCommandHandler
{
    void Execute(ChatWindow window, string[] parameters);

    /// <summary>
    ///     Provide command description without mentioning command name
    /// </summary>
    string GetDescription();

    /// <summary>
    ///     Provide argument usage (If needed) without starting with command name
    /// </summary>
    string[] GetUsage();
}

public class ChatCommandUsageException : Exception
{
    public ChatCommandUsageException(string message) : base(message)
    {
    }
}

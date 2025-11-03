#region

using System;
using System.Collections.Generic;
using NebulaModel;
using NebulaModel.DataStructures.Chat;
using NebulaWorld.Chat.Commands;

#endregion

namespace NebulaWorld.Chat;

/// <summary>
/// Core chat service managing message history and command processing
/// </summary>
public class ChatService
{
    private const int MAX_MESSAGES = 200;

    /// <summary>
    /// Singleton instance of ChatService
    /// </summary>
    public static ChatService Instance { get; private set; } = new();

    private readonly List<RawChatMessage> messageHistory = new();
    private readonly Queue<RawChatMessage> outgoingMessages = new();

    /// <summary>
    /// Event triggered when a new message is added
    /// </summary>
    public event Action<RawChatMessage> OnMessageAdded;

    /// <summary>
    /// Event triggered when the message content changed
    /// </summary>
    public event Action<RawChatMessage> OnMessageRefresh;

    /// <summary>
    /// Event triggered when a message is removed
    /// </summary>
    public event Action<RawChatMessage> OnMessageRemoved;

    /// <summary>
    /// Gets read-only access to message history
    /// </summary>
    public IReadOnlyList<RawChatMessage> MessageHistory => messageHistory.AsReadOnly();

    /// <summary>
    /// Processes user input (commands or messages)
    /// </summary>
    /// <param name="input">The user input text</param>
    /// <param name="userName">The username of the sender</param>
    public void ProcessUserInput(string input, string userName)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        if (input.StartsWith(ChatCommandRegistry.CommandPrefix))
        {
            ExecuteCommand(input);
        }
        else
        {
            var msg = new RawChatMessage
            {
                MessageText = input,
                UserName = userName,
                Timestamp = DateTime.Now,
                MessageType = ChatMessageType.PlayerMessage
            };

            QueueOutgoingMessage(msg);

            // Show player message locally
            AddMessage(msg.MessageText, msg.MessageType, msg.UserName, msg.Timestamp);
        }
    }

    /// <summary>
    /// Adds a message to the chat history and triggers display
    /// </summary>
    /// <param name="messageText">The message text</param>
    /// <param name="messageType">The type of message</param>
    /// <param name="userName">The username of the sender (optional)</param>
    /// <param name="timestamp">Optional timestamp (defaults to now)</param>
    /// <returns>The added RawChatMessage reference</returns>
    public RawChatMessage AddMessage(string messageText, ChatMessageType messageType, string userName = null, DateTime? timestamp = null)
    {
        // Filter messages based on config
        if (!ShouldDisplayMessage(messageType)) return null;

        var msg = new RawChatMessage
        {
            MessageText = messageText,
            UserName = userName ?? string.Empty,
            Timestamp = timestamp ?? DateTime.Now,
            MessageType = messageType
        };

        messageHistory.Add(msg);

        // Maintain max message limit
        if (messageHistory.Count > MAX_MESSAGES)
        {
            messageHistory.RemoveAt(0);
        }

        OnMessageAdded?.Invoke(msg);
        return msg;
    }

    /// <summary>
    /// Clear messages that match the filter predicate
    /// </summary>
    /// <param name="filter">Predicate to filter messages</param>
    public void ClearMessages(Func<RawChatMessage, bool> filter)
    {
        for (var i = messageHistory.Count - 1; i >= 0; i--)
        {
            var msg = messageHistory[i];
            if (filter(msg))
            {
                messageHistory.RemoveAt(i);
                OnMessageRemoved?.Invoke(msg);
            }
        }
    }

    /// <summary>
    /// Notify view to refresh the message
    /// </summary>
    /// <param name="message">The changed message</param>
    public void NotifyMessageChange(RawChatMessage message)
    {
        OnMessageRefresh?.Invoke(message);
    }

    /// <summary>
    /// Gets the next queued outgoing message
    /// </summary>
    /// <returns>The next message to send, or null if queue is empty</returns>
    public RawChatMessage GetQueuedMessage()
    {
        return outgoingMessages.Count > 0 ? outgoingMessages.Dequeue() : null;
    }

    private void ExecuteCommand(string commandInput)
    {
        var args = commandInput.Substring(1).Split(' ');
        if (args.Length == 0) return;

        var commandName = args[0];
        var handler = ChatCommandRegistry.GetCommandHandler(commandName);

        if (handler != null)
        {
            try
            {
                //handler.Execute(this, args.Skip(1).ToArray()); // TODO: change handlers' parameter from ChatWindow to ChatService
            }
            catch (ChatCommandUsageException e)
            {
                AddMessage(
                    $"Invalid usage: {e.Message}! Usage: {ChatCommandRegistry.CommandPrefix}{commandName} {handler.GetUsage()}",
                    ChatMessageType.CommandUsageMessage);
            }
        }
        else
        {
            AddMessage(
                $"Unknown command {commandName}. Use /help to get list of commands",
                ChatMessageType.CommandUsageMessage);
        }
    }

    private void QueueOutgoingMessage(RawChatMessage message)
    {
        outgoingMessages.Enqueue(message);
    }

    private static bool ShouldDisplayMessage(ChatMessageType messageType)
    {
        return messageType switch
        {
            ChatMessageType.SystemInfoMessage => Config.Options.EnableInfoMessage,
            ChatMessageType.SystemWarnMessage => Config.Options.EnableWarnMessage,
            ChatMessageType.BattleMessage => Config.Options.EnableBattleMessage,
            _ => true
        };
    }
}

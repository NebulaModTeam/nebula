#region

using System;
using NebulaModel.DataStructures.Chat;

#endregion

namespace NebulaModel.Packets.Chat;

public class NewChatMessagePacket
{
    public NewChatMessagePacket() { }

    public NewChatMessagePacket(ChatMessageType messageType, string messageText, DateTime sentAt, string userName)
    {
        MessageType = messageType;
        MessageText = messageText;
        SentAt = sentAt.ToBinary();
        UserName = userName;
    }

    public ChatMessageType MessageType { get; }
    public string MessageText { get; }
    public long SentAt { get; }
    public string UserName { get; }
}

using System;

namespace NebulaModel.Packets.Players
{
    public enum ChatMessageType
    {
        PlayerMessage = 0,
        SystemMessage = 1,
        Command = 2
    }

    public class NewChatMessagePacket
    {
        public ChatMessageType MessageType { get; set; }
        public string MessageText { get; set; }
        public long SentAt { get; set; }
        public string UserName { get; set; }

        public NewChatMessagePacket() { }

        public NewChatMessagePacket(ChatMessageType messageType, string messageText, DateTime sentAt, string userName)
        {
            MessageType = messageType;
            MessageText = messageText;
            SentAt = sentAt.ToBinary();
            UserName = userName;
        }
    }
}
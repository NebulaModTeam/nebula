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

        // these two don't get set until the message has gone to the host to be distributed back out
        public long SentAt { get; set; }
        public string UserName { get; set; }

        public NewChatMessagePacket() { }

        public NewChatMessagePacket(ChatMessageType messageType, string messageText)
        {
            MessageType = messageType;
            MessageText = messageText;
        }
    }
}
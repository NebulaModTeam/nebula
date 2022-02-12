namespace NebulaModel.Packets.Players
{
    public class ChatCommandWhisperPacket
    {
        public string SenderUsername { get; set; }
        public string RecipientUsername { get; set; }
        public string Message { get; set; }

        public ChatCommandWhisperPacket() { }

        public ChatCommandWhisperPacket(string sender, string recipient, string message)
        {
            SenderUsername = sender;
            RecipientUsername = recipient;
            Message = message;
        }
    }
}
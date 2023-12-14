namespace NebulaModel.Packets.Chat;

public class ChatCommandWhisperPacket
{
    public ChatCommandWhisperPacket() { }

    public ChatCommandWhisperPacket(string sender, string recipient, string message)
    {
        SenderUsername = sender;
        RecipientUsername = recipient;
        Message = message;
    }

    public string SenderUsername { get; }
    public string RecipientUsername { get; }
    public string Message { get; }
}

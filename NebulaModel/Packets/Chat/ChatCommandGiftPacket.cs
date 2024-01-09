using NebulaModel.DataStructures.Chat;

namespace NebulaModel.Packets.Chat;

public class ChatCommandGiftPacket
{
    public ChatCommandGiftPacket() { }

    public ChatCommandGiftPacket(string sender, string recipient, ChatCommandGiftType type, long quantity)
    {
        SenderUsername = sender;
        RecipientUsername = recipient;
        Type = type;
        Quantity = quantity;
    }

    public string SenderUsername { get; set; }
    public string RecipientUsername { get; set; }
    public ChatCommandGiftType Type { get; set; }
    public long Quantity { get; set; }
}

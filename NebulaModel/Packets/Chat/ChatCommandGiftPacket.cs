using NebulaModel.DataStructures.Chat;

namespace NebulaModel.Packets.Chat;

public class ChatCommandGiftPacket
{
    public ChatCommandGiftPacket() { }

    public ChatCommandGiftPacket(ushort senderUserId, ushort recipientUserId, ChatCommandGiftType type, long quantity)
    {
        SenderUserId = senderUserId;
        RecipientUserId = recipientUserId;
        Type = type;
        Quantity = quantity;
    }

    public ushort SenderUserId { get; set; }
    public ushort RecipientUserId { get; set; }
    public ChatCommandGiftType Type { get; set; }
    public long Quantity { get; set; }
}

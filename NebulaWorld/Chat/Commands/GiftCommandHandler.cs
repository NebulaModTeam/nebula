#region

using NebulaModel.DataStructures.Chat;
using NebulaModel.Packets.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class GiftCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        if (parameters.Length < 3)
        {
            throw new ChatCommandUsageException("Not enough arguments!".Translate());
        }

        var senderUsername = Multiplayer.Session?.LocalPlayer?.Data?.Username ?? "UNKNOWN";
        if (senderUsername == "UNKNOWN" || Multiplayer.Session == null || Multiplayer.Session.LocalPlayer == null)
        {
            window.SendLocalChatMessage("Invalid sender (not connected), can't send gift".Translate(), ChatMessageType.CommandErrorMessage);
            return;
        }

        // TODO: Add support for using id instead of username
        var recipientUsername = parameters[0];
        if (senderUsername == recipientUsername)
        {
            window.SendLocalChatMessage("Invalid recipient (self), can't send gift".Translate(), ChatMessageType.CommandErrorMessage);
            return;
        }
        //var fullMessageBody = string.Join(" ", parameters.Skip(1));
        // first echo what the player typed so they know something actually happened
        //ChatManager.Instance.SendChatMessage($"[{DateTime.Now:HH:mm}] [To: {recipientUserName}] : {fullMessageBody}",
        //    ChatMessageType.PlayerMessage);

        ChatCommandGiftType type;
        switch (parameters[1])
        {
            case "soil":
            case "sand":
            case "s":
                type = ChatCommandGiftType.Soil;
                break;
            // TODO: Implement Item and Energy variants.
            default:
                window.SendLocalChatMessage("Invalid gift type, can't send gift".Translate(), ChatMessageType.CommandErrorMessage);
                return;
        }

        long quantity;
        if (!long.TryParse(parameters[2], out quantity) || quantity == 0)
        {
            window.SendLocalChatMessage("Invalid gift quantity, can't send gift".Translate(), ChatMessageType.CommandErrorMessage);
            return;
        }

        // Validate that you acctually have the required soil/items/energy to gift
        switch (type)
        {
            case ChatCommandGiftType.Soil:
                var mainPlayer = GameMain.data.mainPlayer;
                lock (mainPlayer)
                {
                    if (mainPlayer.sandCount < quantity)
                    {
                        window.SendLocalChatMessage("You dont have enough soil to send, can't send gift".Translate(), ChatMessageType.CommandErrorMessage);
                        return;
                    }

                    mainPlayer.SetSandCount(mainPlayer.sandCount - quantity);
                    // TODO: Do we need to do something with soil sync?
                }
                break;
                // TODO: Implement Item and Energy variants.
        }

        var packet = new ChatCommandGiftPacket(senderUsername, recipientUsername, type, quantity);
        Multiplayer.Session.Network.SendPacketToClient(packet, recipientUsername);
    }

    public string GetDescription()
    {
        return string.Format("Send gift to player. Use /who for valid user names. Valid types are soil (s), item (i), energy (e)".Translate());
    }

    public string[] GetUsage()
    {
        return ["<player> <type> <quantity>"];
    }

    // TODO: We should add logic here that acctually adds the gifted materials (and devise something to substract the materials)
    //public static void SendWhisperToLocalPlayer(string sender, string mesageBody)
    //{
    //    ChatManager.Instance.SendChatMessage($"[{DateTime.Now:HH:mm}] [{sender} whispered] : {mesageBody}",
    //        ChatMessageType.PlayerMessagePrivate);
    //}
}

#region

using System;
using System.Linq;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Packets.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class GiftCommandHandler : IChatCommandHandler
{
    private struct UserInfo
    {
        public ushort id;
        public string name;
    }

    public void Execute(ChatWindow window, string[] parameters)
    {
        if (parameters.Length < 3)
        {
            throw new ChatCommandUsageException("Not enough arguments!".Translate());
        }

        UserInfo sender;
        {
            if (
                Multiplayer.Session?.LocalPlayer?.Data?.PlayerId is ushort senderUserId
                && Multiplayer.Session?.LocalPlayer?.Data?.Username is string senderUsername
            )
            {
                sender = new UserInfo
                {
                    id = senderUserId,
                    name = senderUsername
                };
            }
            else
            {
                window.SendLocalChatMessage("Invalid sender (not connected), can't send gift".Translate(), ChatMessageType.CommandErrorMessage);
                return;
            };
        }

        var userIdOrNameParameter = parameters[0];

        var foundRecipient = false;
        var recipient = new UserInfo();
        // TODO: Abstract this into a utility function to get the user data by id (that also works on clients
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            foreach (var remotePlayerModel in remotePlayersModels)
            {
                var movement = remotePlayerModel.Value.Movement;
                if ((ushort.TryParse(userIdOrNameParameter, out var recipientUserId) && movement.PlayerID == recipientUserId) || movement.Username == userIdOrNameParameter)
                {
                    foundRecipient = true;
                    recipient = new UserInfo
                    {
                        id = movement.PlayerID,
                        name = movement.Username
                    };
                    break;
                }
            }
        }

        if (!foundRecipient)
        {
            window.SendLocalChatMessage("Invalid recipient (user id or username not found), can't send gift".Translate(), ChatMessageType.CommandErrorMessage);
            return;
        }

        if (sender.id == recipient.id)
        {
            window.SendLocalChatMessage("Invalid recipient (self), can't send gift".Translate(), ChatMessageType.CommandErrorMessage);
            return;
        }

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

        // Add support for scientific notation and other notation types
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

        var packet = new ChatCommandGiftPacket(sender.id, recipient.id, type, quantity);
        Multiplayer.Session.Network.SendPacketToClient(packet, recipient.id);

        switch (type)
        {
            case ChatCommandGiftType.Soil:
                ChatManager.Instance.SendChatMessage($"[{DateTime.Now:HH:mm}] You gifted [{recipient.id}] {recipient.name} soil ({packet.Quantity})", ChatMessageType.SystemInfoMessage);
                break;
                // TODO: Implement Item and Energy variants.
        }
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

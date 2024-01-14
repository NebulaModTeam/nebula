#region

using System;
using System.Collections.Generic;
using System.Linq;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
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

        UserInfo recipient;
        {
            var userIdOrNameParameter = parameters[0];
            var couldParseUserId = ushort.TryParse(userIdOrNameParameter, out var recipientUserId);

            UserInfo? recipientOrNull = null;
            if (couldParseUserId)
            {
                recipientOrNull = getUserInfoById(recipientUserId);
            }

            if (recipientOrNull is UserInfo recipientNotNull)
            {
                recipient = recipientNotNull;
            }
            else
            {
                var recipientsByUsername = getUserInfosByUsername(userIdOrNameParameter);

                if (recipientsByUsername.Count == 0)
                {
                    window.SendLocalChatMessage("Invalid recipient (user id or username not found), can't send gift".Translate(), ChatMessageType.CommandErrorMessage);
                    return;
                }

                if (recipientsByUsername.Count > 1)
                {
                    window.SendLocalChatMessage("Ambiguous recipient (multiple recipients with same username), can't send gift".Translate(), ChatMessageType.CommandErrorMessage);
                    return;
                }

                recipient = recipientsByUsername.First();
            }
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
        if (!long.TryParse(parameters[2], out var quantity) || quantity == 0)
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
                window.SendLocalChatMessage($"[{DateTime.Now:HH:mm}] You gifted [{recipient.id}] {recipient.name} soil ({packet.Quantity})", ChatMessageType.SystemInfoMessage);
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

    private UserInfo? getUserInfoById(ushort userId)
    {
        // TODO: This does not include self, perhaps we should include this
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            foreach (var remotePlayerModel in remotePlayersModels)
            {
                var movement = remotePlayerModel.Value.Movement;
                if (movement.PlayerID == userId)
                {
                    return new UserInfo
                    {
                        id = movement.PlayerID,
                        name = movement.Username
                    };
                }
            }
        }
        return null;
    }

    private List<UserInfo> getUserInfosByUsername(string username)
    {
        List<UserInfo> userInfos = new();
        // TODO: This does not include self, perhaps we should include this
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            foreach (var remotePlayerModel in remotePlayersModels)
            {
                var movement = remotePlayerModel.Value.Movement;
                if (movement.Username == username)
                {
                    userInfos.Add(new UserInfo
                    {
                        id = movement.PlayerID,
                        name = movement.Username
                    });
                }
            }
        }

        return userInfos;
    }
}

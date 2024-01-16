#region

using System;
using NebulaAPI.Packets;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Chat;
using NebulaWorld;
using NebulaWorld.Chat.Commands;
using NebulaWorld.MonoBehaviours.Local.Chat;
using static UnityEngine.Analytics.Analytics;

#endregion

namespace NebulaNetwork.PacketProcessors.Chat;

[RegisterPacketProcessor]
internal class ChatCommandGiftProcessor : PacketProcessor<ChatCommandGiftPacket>
{
    protected override void ProcessPacket(ChatCommandGiftPacket packet, NebulaConnection conn)
    {
        // If you are not the intended recipient of this packet do not process this packet
        if (packet.RecipientUserId != Multiplayer.Session.LocalPlayer.Data.PlayerId)
        {
            // However if you are the host relay the packet to the recipient
            if (IsHost)
            {
                var recipient = Multiplayer.Session.Network.PlayerManager.GetPlayerById(packet.RecipientUserId);
                if (recipient != null)
                {
                    recipient.SendPacket(packet);
                }
                else
                {
                    Log.Warn($"Could not relay packet because recipient was not found with clientId: {packet.RecipientUserId}");
                    // TODO: if the recipient is not found return the failure packet
                    //conn.SendPacket(giftFailedPacket); // the giftFailedPacket needs the same kind of handling as that can also need to be relayed
                }
            }
            return;
        }

        //window.SendLocalChatMessage("Invalid gift type".Translate(), ChatMessageType.CommandErrorMessage);
        string senderUserName = null;
        // TODO: Unify this into something
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            foreach (var remotePlayerModel in remotePlayersModels)
            {
                var movement = remotePlayerModel.Value.Movement;
                if (movement.PlayerID == packet.SenderUserId)
                {
                    senderUserName = movement.Username;
                    break;
                }
            }
        }

        switch (packet.Type)
        {
            case ChatCommandGiftType.Soil:
                var mainPlayer = GameMain.data.mainPlayer;
                lock (mainPlayer)
                {
                    mainPlayer.SetSandCount(mainPlayer.sandCount + packet.Quantity);
                    // TODO: Do we need to do something with soil sync?
                }
                ChatManager.Instance.SendChatMessage($"[{DateTime.Now:HH:mm}] [{packet.SenderUserId}] {senderUserName} gifted you soil ({packet.Quantity})", ChatMessageType.SystemInfoMessage);
                break;
            // TODO: Implement Item and Energy variants.
            default:
                return;
        }

        // TODO: Logic for adding the soil, items, energy etc (restransmission logic no longer needed with the ClientRelayPacket

        //if (IsClient)
        //{
        //    WhisperCommandHandler.SendWhisperToLocalPlayer(packet.SenderUsername, packet.Message);
        //}
        //else
        //{
        //    // two cases, simplest is that whisper is meant for host
        //    if (Multiplayer.Session.LocalPlayer.Data.Username == packet.RecipientUsername)
        //    {
        //        WhisperCommandHandler.SendWhisperToLocalPlayer(packet.SenderUsername, packet.Message);
        //        return;
        //    }

        //    // second case, relay message to recipient
        //    var recipient = Multiplayer.Session.Network
        //        .PlayerManager.GetConnectedPlayerByUsername(packet.RecipientUsername);
        //    if (recipient == null)
        //    {
        //        Log.Warn($"Recipient not found {packet.RecipientUsername}");
        //        var sender = Multiplayer.Session.Network.PlayerManager.GetPlayer(conn);
        //        sender.SendPacket(new ChatCommandWhisperPacket("SYSTEM".Translate(), packet.SenderUsername,
        //            string.Format("User not found {0}".Translate(), packet.RecipientUsername)));
        //        return;
        //    }

        //    recipient.SendPacket(packet);
        //}
    }
}

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

#endregion

namespace NebulaNetwork.PacketProcessors.Chat;

[RegisterPacketProcessor]
internal class ChatCommandGiftProcessor : PacketProcessor<ChatCommandGiftPacket>
{
    protected override void ProcessPacket(ChatCommandGiftPacket packet, NebulaConnection conn)
    {

        //window.SendLocalChatMessage("Invalid gift type".Translate(), ChatMessageType.CommandErrorMessage);

        switch (packet.Type)
        {
            case ChatCommandGiftType.Soil:
                var mainPlayer = GameMain.data.mainPlayer;
                lock (mainPlayer)
                {
                    mainPlayer.SetSandCount(mainPlayer.sandCount + packet.Quantity);
                    // TODO: Do we need to do something with soil sync?
                }
                ChatManager.Instance.SendChatMessage($"[{DateTime.Now:HH:mm}] {packet.SenderUsername} gifted you {packet.Quantity} soil", ChatMessageType.SystemInfoMessage);
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

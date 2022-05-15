using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.MonoBehaviours.Local;
using System;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    internal class NewChatMessageProcessor : PacketProcessor<NewChatMessagePacket>
    {

        public NewChatMessageProcessor()
        {
        }

        public override void ProcessPacket(NewChatMessagePacket packet, NebulaConnection conn)
        {
            if (ChatManager.Instance == null)
            {
                Log.Warn($"Unable to process chat packet, chat window assets were not loaded properly");
                return;
            }

            if (IsHost)
            {
                INebulaPlayer player = Multiplayer.Session.Network.PlayerManager?.GetPlayer(conn);
                Multiplayer.Session.Network.PlayerManager?.SendPacketToOtherPlayers(packet, player);
            }

            DateTime sentAt = packet.SentAt == 0 ? DateTime.Now : DateTime.FromBinary(packet.SentAt);
            if (string.IsNullOrEmpty(packet.UserName))
            {
                ChatManager.Instance.SendChatMessage($"[{sentAt:HH:mm}] {packet.MessageText}", packet.MessageType);
            }
            else
            {
                ChatManager.Instance.SendChatMessage($"[{sentAt:HH:mm}] [{packet.UserName}] : {packet.MessageText}", packet.MessageType);
            }
        }
    }
}
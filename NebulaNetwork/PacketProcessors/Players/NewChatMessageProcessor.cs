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
        private readonly ChatManager _chatManager;

        public NewChatMessageProcessor()
        {
            _chatManager = InGameChatAssetLoader.ChatManager();
        }

        public override void ProcessPacket(NewChatMessagePacket packet, NebulaConnection conn)
        {
            if (_chatManager == null)
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
            _chatManager.QueueChatMessage($"[{sentAt:HH:mm}] [{packet.UserName}] : {packet.MessageText}", packet.MessageType);
        }
    }
}
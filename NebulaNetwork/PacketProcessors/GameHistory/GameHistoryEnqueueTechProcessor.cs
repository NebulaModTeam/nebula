﻿using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld.GameDataHistory;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryEnqueueTechProcessor : PacketProcessor<GameHistoryEnqueueTechPacket>
    {
        private PlayerManager playerManager;

        public GameHistoryEnqueueTechProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(GameHistoryEnqueueTechPacket packet, NetworkConnection conn)
        {
            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    using (GameDataHistoryManager.IsIncomingRequest.On())
                    {
                        GameMain.history.EnqueueTech(packet.TechId);
                    }
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
            }
            else
            {
                using (GameDataHistoryManager.IsIncomingRequest.On())
                {
                    GameMain.history.EnqueueTech(packet.TechId);
                }
            }

        }
    }
}

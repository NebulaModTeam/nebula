using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using NebulaWorld.GameDataHistory;

namespace NebulaHost.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryEnqueueTechProcessor : IPacketProcessor<GameHistoryEnqueueTechPacket>
    {
        private PlayerManager playerManager;

        public GameHistoryEnqueueTechProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(GameHistoryEnqueueTechPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                GameDataHistoryManager.IsIncommingRequest = true;
                GameMain.history.EnqueueTech(packet.TechId);
                GameDataHistoryManager.IsIncommingRequest = false;
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}

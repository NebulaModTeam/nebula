using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using NebulaWorld.GameDataHistory;

namespace NebulaHost.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryRemoveTechProcessor : IPacketProcessor<GameHistoryRemoveTechPacket>
    {
        private PlayerManager playerManager;

        public GameHistoryRemoveTechProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(GameHistoryRemoveTechPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                GameDataHistoryManager.IsIncommingRequest = true;
                GameMain.history.RemoveTechInQueue(packet.Index);
                GameDataHistoryManager.IsIncommingRequest = false;
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}

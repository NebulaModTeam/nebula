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
                using (GameDataHistoryManager.IsIncomingRequest.On())
                {
                    int index = System.Array.IndexOf(GameMain.history.techQueue, packet.techId);
                    //sanity: packet wanted to remove tech, which is not queued on this client
                    index = (index >= 0) ? index : 0;
                    GameMain.history.RemoveTechInQueue(index);
                }
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}

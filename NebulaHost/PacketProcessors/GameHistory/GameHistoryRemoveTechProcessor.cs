using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using NebulaWorld.GameDataHistory;
using System.Collections.Generic;

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
                    int index = System.Array.IndexOf(GameMain.history.techQueue, packet.TechId);
                    //sanity: packet wanted to remove tech, which is not queued on this client, ignore it
                    if(index < 0)
                    {
                        return;
                    }
                    GameMain.history.RemoveTechInQueue(index);
                }
            }
        }
    }
}

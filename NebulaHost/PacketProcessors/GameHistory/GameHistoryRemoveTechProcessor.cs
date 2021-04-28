using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using NebulaWorld.GameDataHistory;
using NebulaModel.Logger;
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
                //we need to prepare an potential refund, get currently needed items for research
                List<int> usedItems = new List<int>();
                foreach (KeyValuePair<int, int> item in GameMain.data.mainPlayer.mecha.lab.itemPoints.items)
                {
                    usedItems.Add(item.Key);
                }

                using (GameDataHistoryManager.IsIncomingRequest.On())
                {
                    int index = System.Array.IndexOf(GameMain.history.techQueue, packet.TechId);
                    //sanity: packet wanted to remove tech, which is not queued on this client, ignore, but re
                    //index = (index >= 0) ? index : 0;
                    if(index < 0)
                    {
                        return;
                    }
                    GameMain.history.RemoveTechInQueue(index);
                }

                //send players their contributions back
                using (playerManager.GetConnectedPlayers(out var connectedPlayers))
                {
                    foreach (var kvp in connectedPlayers)
                    {
                        Player curPlayer = kvp.Value;
                        Log.Info($"Sending Recoverrequest for player {curPlayer.Id}: refunding for techId {packet.TechId} - progress: {curPlayer.TechProgressContributed}");
                        GameHistoryTechRefundPacket refundPacket = new GameHistoryTechRefundPacket(packet.TechId, usedItems.ToArray() , curPlayer.ReleaseResearchProgress());
                        curPlayer.SendPacket(refundPacket);
                    }
                }
                
            }
        }
    }
}

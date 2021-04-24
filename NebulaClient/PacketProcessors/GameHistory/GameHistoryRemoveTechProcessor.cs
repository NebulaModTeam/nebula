using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using NebulaWorld.GameDataHistory;

namespace NebulaClient.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryRemoveTechProcessor : IPacketProcessor<GameHistoryRemoveTechPacket>
    {
        public void ProcessPacket(GameHistoryRemoveTechPacket packet, NebulaConnection conn)
        {
            Log.Info($"Removing tech (ID: {packet.techId}) from queue");
            using (GameDataHistoryManager.IsIncomingRequest.On())
            {
                int index = -1;
                Log.Info($"ProcessPacket: Client techqueue is at length {GameMain.history.techQueueLength} now");
                for (int i = 0; i < GameMain.history.techQueueLength; i++)
                {
                    Log.Info($"ProcessPacket: Client Techqueue at {i} has techid {GameMain.history.techQueue[i]}");
                    if (GameMain.history.techQueue[i] == packet.techId)
                    {
                        Log.Info($"ProcessPacket: Client Found Tech at index {i}!");
                        index = i;
                        break;
                    }
                }
                //int index = System.Array.IndexOf(GameMain.history.techQueue, packet.techId);
                //Log.Info($"Removing techid {packet.techId} was at pos {index}");
                if (index < 0)
                {
                    //sanity: packet wanted to remove tech, which is not queued on this client
                    Log.Info($"ProcessPacket: Client TechId: {packet.techId} was not in queue, discarding paket");
                    return;
                }
                //recover spend items from inventory before removing it from queue
                GameMain.mainPlayer.mecha.lab.ManageTakeback();
                GameMain.history.RemoveTechInQueue(index);
                //Wenn nodes noch da sind: UIResearchQueue::UpdateNodes()
                Log.Info($"ProcessPacket: Client Techqueue is at length {GameMain.history.techQueueLength} after");
            }
        }
    }
}

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
            Log.Info($"Removing tech (ID: {packet.TechId}) from queue");
            using (GameDataHistoryManager.IsIncomingRequest.On())
            {
                int index = -1;
                for (int i = 0; i < GameMain.history.techQueueLength; i++)
                {
                    if (GameMain.history.techQueue[i] == packet.TechId)
                    {
                        index = i;
                        break;
                    }
                }
                if (index < 0)
                {
                    //sanity: packet wanted to remove tech, which is not queued on this client
                    Log.Info($"ProcessPacket: TechId: {packet.TechId} was not in queue, discarding paket");
                    return;
                }
                GameMain.history.RemoveTechInQueue(index);
                Log.Info($"ProcessPacket: Techqueue is at length {GameMain.history.techQueueLength} after");
            }
        }
    }
}

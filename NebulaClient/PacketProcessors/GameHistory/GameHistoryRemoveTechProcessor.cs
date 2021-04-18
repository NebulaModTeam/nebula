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
            Log.Info($"Removing tech (ID: {packet.Index}) from queue");
            using (GameDataHistoryManager.IsIncomingRequest.On())
            {
                GameMain.history.RemoveTechInQueue(packet.Index);
            }
        }
    }
}

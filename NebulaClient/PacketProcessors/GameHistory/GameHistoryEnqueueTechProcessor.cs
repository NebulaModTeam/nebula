using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets;
using NebulaWorld.GameDataHistory;

namespace NebulaClient.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryEnqueueTechProcessor : PacketProcessor<GameHistoryEnqueueTechPacket>
    {
        public override void ProcessPacket(GameHistoryEnqueueTechPacket packet, NebulaConnection conn)
        {
            Log.Info($"Enquing new tech ID: {packet.TechId}");
            using (GameDataHistoryManager.IsIncomingRequest.On())
            {
                GameMain.history.EnqueueTech(packet.TechId);
            }
        }
    }
}

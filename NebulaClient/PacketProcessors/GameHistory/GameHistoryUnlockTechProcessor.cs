using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using NebulaWorld.GameDataHistory;

namespace NebulaClient.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryUnlockTechProcessor : IPacketProcessor<GameHistoryUnlockTechPacket>
    {
        public void ProcessPacket(GameHistoryUnlockTechPacket packet, NebulaConnection conn)
        {
            Log.Info($"Unlocking tech (ID: {packet.TechId})");
            using (GameDataHistoryManager.IsIncomingRequest.On())
            {
                GameMain.history.UnlockTech(packet.TechId);
                GameMain.mainPlayer.mecha.lab.itemPoints.Clear();
                GameMain.history.DequeueTech();
            }
        }
    }
}
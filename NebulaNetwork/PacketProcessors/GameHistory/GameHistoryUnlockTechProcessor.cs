using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld.GameDataHistory;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryUnlockTechProcessor : PacketProcessor<GameHistoryUnlockTechPacket>
    {
        public override void ProcessPacket(GameHistoryUnlockTechPacket packet, NebulaConnection conn)
        {
            Log.Info($"Unlocking tech (ID: {packet.TechId})");
            using (GameDataHistoryManager.IsIncomingRequest.On())
            {
                GameMain.mainPlayer.mecha.lab.itemPoints.Clear();
                GameMain.history.DequeueTech();
                GameMain.history.UnlockTech(packet.TechId);
            }
        }
    }
}
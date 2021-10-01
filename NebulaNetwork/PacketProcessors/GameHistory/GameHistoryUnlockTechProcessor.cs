using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    internal class GameHistoryUnlockTechProcessor : PacketProcessor<GameHistoryUnlockTechPacket>
    {
        public override void ProcessPacket(GameHistoryUnlockTechPacket packet, NebulaConnection conn)
        {
            Log.Info($"Unlocking tech (ID: {packet.TechId})");
            using (Multiplayer.Session.History.IsIncomingRequest.On())
            {
                GameMain.mainPlayer.mecha.lab.itemPoints.Clear();
                GameMain.history.DequeueTech();
                GameMain.history.UnlockTech(packet.TechId);
            }
        }
    }
}
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
            using (Multiplayer.Session.History.IsIncomingRequest.On())
            {
                // Let the default method give back the items
                GameMain.mainPlayer.mecha.lab.ManageTakeback();

                // Update techState
                TechState techState = GameMain.history.techStates[packet.TechId];
                Log.Info($"Unlocking tech={packet.TechId} local:{techState.curLevel} remote:{packet.Level}");
                techState.curLevel = packet.Level;
                GameMain.history.techStates[packet.TechId] = techState;

                GameMain.history.UnlockTechUnlimited(packet.TechId, false);
                GameMain.history.DequeueTech();                
            }
        }
    }
}
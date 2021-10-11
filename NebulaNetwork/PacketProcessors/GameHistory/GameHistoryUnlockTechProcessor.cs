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
                // Let the default method give back the items
                GameMain.mainPlayer.mecha.lab.ManageTakeback();

                // Update techState 
                TechProto techProto = LDB.techs.Select(packet.TechId);
                TechState techState = GameMain.history.techStates[packet.TechId];
                if (techState.curLevel >= techState.maxLevel)
                {
                    techState.curLevel = techState.maxLevel;
                    techState.hashUploaded = techState.hashNeeded;
                    techState.unlocked = true;
                }
                else
                {
                    techState.curLevel++;
                    techState.hashUploaded = 0L;
                    techState.hashNeeded = techProto.GetHashNeeded(techState.curLevel);
                }
                // UnlockTech() unlocks tech to techState.maxLevel, so change it to curLevel temporarily
                int maxLevl = techState.maxLevel;
                techState.maxLevel = techState.curLevel;
                GameMain.history.techStates[packet.TechId] = techState;
                GameMain.history.UnlockTech(packet.TechId);
                techState.maxLevel = maxLevl;
                GameMain.history.techStates[packet.TechId] = techState;
                GameMain.history.DequeueTech();                
            }
        }
    }
}
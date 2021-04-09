using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameScenarioLogic))]
    class GameScenarioLogic_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("NotifyOnUnlockTech")]
        public static void Postfix1(int techId)
        {
            //Synchronize unlocking techs
            // Do not run if it is not multiplayer and if the player is not a client
            if (!SimulatedWorld.Initialized || !LocalPlayer.IsMasterClient)
            {
                return;
            }
            //Notify all clients about unlocked tech
            Log.Info($"Sending Tech Unlocked notification");
            GameMain.mainPlayer.mecha.lab.itemPoints.Clear();
            LocalPlayer.SendPacket(new GameHistoryUnlockTechPacket(techId));
        }
    }
}

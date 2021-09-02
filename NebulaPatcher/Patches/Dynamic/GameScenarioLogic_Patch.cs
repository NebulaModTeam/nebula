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
        [HarmonyPatch(nameof(GameScenarioLogic.NotifyOnUnlockTech))]
        public static void NotifyOnUnlockTech_Postfix(int techId)
        {
            //Synchronize unlocking techs
            // Do not run if it is not multiplayer and if the player is not a client
            if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsHost)
            {
                return;
            }
            //Notify all clients about unlocked tech
            Log.Info($"Sending Tech Unlocked notification");
            GameMain.mainPlayer.mecha.lab.itemPoints.Clear();
            Multiplayer.Session.Network.SendPacket(new GameHistoryUnlockTechPacket(techId));
        }
    }
}

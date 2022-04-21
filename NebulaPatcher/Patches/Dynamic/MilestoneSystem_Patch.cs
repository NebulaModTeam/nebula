using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(MilestoneSystem))]
    internal class MilestoneSystem_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MilestoneSystem.SetForNewGame))]
        public static void SetForNewGame_Postfix()
        {
            // Do not run if it is not multiplayer and the player is not a client
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return;
            }
            // Request milestone data
            Log.Info($"Requesting MilestoneData from the server");
            Multiplayer.Session.Network.SendPacket(new MilestoneDataRequest());
        }
    }
}

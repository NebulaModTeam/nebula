#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(FactoryProductionStat))]
internal class FactoryProductionStat_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FactoryProductionStat.GameTick))]
    public static bool GameTick_Prefix(FactoryProductionStat __instance)
    {
        //Do not run in single player for host
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return true;
        }

        //Multiplayer clients should not include their own calculated statistics
        if (Multiplayer.Session.Statistics.IsIncomingRequest)
        {
            return true;
        }
        __instance.ClearRegisters();
        return false;

    }
}

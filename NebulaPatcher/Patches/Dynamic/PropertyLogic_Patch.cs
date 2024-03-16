#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(PropertyLogic))]
internal class PropertyLogic_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PropertyLogic.GameTick))]
    public static bool PrepareTick_Prefix()
    {
        if (!Multiplayer.IsActive) return true;

        // Disable UpdateProduction in client to prevent errror
        return Multiplayer.Session.IsServer;
    }
}

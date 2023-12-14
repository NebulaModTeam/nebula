#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(RaycastLogic))]
internal class RaycastLogic_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RaycastLogic.GameTick))]
    public static bool GameTick_Prefix(RaycastLogic __instance)
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsClient)
        {
            return true;
        }
        return __instance.factory != null;
        // while we wait for factory data from server this is still null, prevent running into NRE
    }
}

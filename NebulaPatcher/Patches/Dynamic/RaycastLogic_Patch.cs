using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(RaycastLogic))]
    internal class RaycastLogic_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(RaycastLogic.GameTick))]
        public static bool GameTick_Prefix(RaycastLogic __instance)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
            {
                if(__instance.factory == null)
                {
                    // while we wait for factory data from server this is still null, prevent running into NRE
                    return false;
                }
            }

            return true;
        }
    }
}

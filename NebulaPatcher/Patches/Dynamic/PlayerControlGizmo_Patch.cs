using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlayerControlGizmo))]
    public class PlayerControlGizmo_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerControlGizmo.GameTick))]
        public static bool GameTick_Prefix(PlayerControlGizmo __instance)
        {
            // index above 100000 indicates we added navigation to a player which gets handled by RemotePlayerMovement.cs
            if (Multiplayer.IsActive && __instance.player.navigation.indicatorAstroId > 100000)
            {
                return false;
            }
            return true;
        }
    }
}

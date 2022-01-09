using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(WarningSystem))]
    class WarningSystem_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(WarningSystem), nameof(WarningSystem.NewWarningData))]
        [HarmonyPatch(typeof(WarningSystem), nameof(WarningSystem.RemoveWarningData))]
        public static bool AlterWarningData_Prefix()
        {
            return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
        }
    }
}

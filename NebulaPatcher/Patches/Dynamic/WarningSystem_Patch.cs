using HarmonyLib;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(WarningSystem))]
    class WarningSystem_Patch
    {
        /*
         * Until we have a proper syncing of this system we avoid index out of bounds by skipping the method if needed.
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(WarningSystem), nameof(WarningSystem.RemoveWarningData))]
        public static bool RemoveWarningData_Prefix(WarningSystem __instance, int id)
        {
            if(id > __instance.warningCursor)
            {
                return false;
            }
            return true;
        }
    }
}

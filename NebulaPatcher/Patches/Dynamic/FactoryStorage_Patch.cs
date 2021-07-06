using HarmonyLib;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(FactoryProductionStat))]
    class FactoryStorage_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GameTick")]
        public static bool GameTick_Prefix()
        {
            StorageManager.IsHumanInput = false;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("GameTick")]
        public static void GameTick_Postfix()
        {
            StorageManager.IsHumanInput = true;
        }
    }
}

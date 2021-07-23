using HarmonyLib;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(FactoryStorage))]
    class FactoryStorage_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FactoryStorage.GameTick))]
        public static bool GameTick_Prefix()
        {
            StorageManager.IsHumanInput = false;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(FactoryStorage.GameTick))]
        public static void GameTick_Postfix()
        {
            StorageManager.IsHumanInput = true;
        }
    }
}

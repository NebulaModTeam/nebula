using HarmonyLib;
using NebulaWorld;
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
            Multiplayer.Session.Storage.IsHumanInput = false;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(FactoryStorage.GameTick))]
        public static void GameTick_Postfix()
        {
            Multiplayer.Session.Storage.IsHumanInput = true;
        }
    }
}

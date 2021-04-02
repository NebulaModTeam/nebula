using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(ProductionStatistics))]
    class ProductionStatistics_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("PrepareTick")]
        public static bool PrepareTick_Prefix(ProductionStatistics __instance)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                for (int i = 0; i < __instance.gameData.factoryCount; i++)
                {
                    __instance.factoryStatPool[i]?.PrepareTick();
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("AfterTick")]
        public static bool AfterTick_Prefix(ProductionStatistics __instance)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                for (int i = 0; i < __instance.gameData.factoryCount; i++)
                {
                    __instance.factoryStatPool[i]?.AfterTick();
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GameTick")]
        public static bool GameTick_Prefix(ProductionStatistics __instance)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                //Do not run on client if you do not have all data
                for (int i = 0; i < __instance.gameData.factoryCount; i++)
                {
                    if (__instance.factoryStatPool[i] == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

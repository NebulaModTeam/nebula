using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PowerSystem))]
    class PowerSystem_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GameTick")]
        public static void PowerSystem_GameTick_Postfix(PowerSystem __instance, long time, bool isActive, bool isMultithreadMode)
        {
            if (SimulatedWorld.Initialized)
            {
                for(int i = 1; i < __instance.netCursor; i++)
                {
                    PowerNetwork pNet = __instance.netPool[i];
                    pNet.energyRequired += PowerTowerManager.GetExtraDemand(__instance.planet.id, i);
                }
                PowerTowerManager.GivePlayerPower();
                PowerTowerManager.UpdateAllAnimations(__instance.planet.id);
            }
        }
    }
}

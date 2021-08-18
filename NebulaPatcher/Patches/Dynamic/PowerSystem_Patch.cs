using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PowerSystem))]
    class PowerSystem_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PowerSystem.GameTick))]
        public static void PowerSystem_GameTick_Postfix(PowerSystem __instance, long time, bool isActive, bool isMultithreadMode)
        {
            if (SimulatedWorld.Instance.Initialized)
            {
                for (int i = 1; i < __instance.netCursor; i++)
                {
                    PowerNetwork pNet = __instance.netPool[i];
                    pNet.energyRequired += PowerTowerManager.GetExtraDemand(__instance.planet.id, i);
                }
                PowerTowerManager.GivePlayerPower();
                PowerTowerManager.UpdateAllAnimations(__instance.planet.id);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PowerSystem.RemoveNodeComponent))]
        public static bool RemoveNodeComponent(PowerSystem __instance, int id)
        {
            if (SimulatedWorld.Instance.Initialized)
            {
                // as the destruct is synced accross players this event is too
                // and as such we can safely remove power demand for every player
                PowerNodeComponent pComp = __instance.nodePool[id];
                PowerTowerManager.RemExtraDemand(__instance.planet.id, pComp.networkId, id);
            }

            return true;
        }
    }
}

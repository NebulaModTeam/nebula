using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PowerSystem))]
    internal class PowerSystem_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PowerSystem.GameTick))]
        public static void PowerSystem_GameTick_Prefix(long time, ref bool isActive)
        {
            //Enable signType update on remote planet every 64 tick
            isActive |= (time & 63) == 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PowerSystem.GameTick))]
        public static void PowerSystem_GameTick_Postfix(PowerSystem __instance, long time, bool isActive, bool isMultithreadMode)
        {
            if (Multiplayer.IsActive)
            {
                for (int i = 1; i < __instance.netCursor; i++)
                {
                    PowerNetwork pNet = __instance.netPool[i];
                    pNet.energyRequired += Multiplayer.Session.PowerTowers.GetExtraDemand(__instance.planet.id, i);
                }
                Multiplayer.Session.PowerTowers.GivePlayerPower();
                Multiplayer.Session.PowerTowers.UpdateAllAnimations(__instance.planet.id);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PowerSystem.RemoveNodeComponent))]
        public static bool RemoveNodeComponent(PowerSystem __instance, int id)
        {
            if (Multiplayer.IsActive)
            {
                // as the destruct is synced accross players this event is too
                // and as such we can safely remove power demand for every player
                PowerNodeComponent pComp = __instance.nodePool[id];
                Multiplayer.Session.PowerTowers.RemExtraDemand(__instance.planet.id, pComp.networkId, id);
            }

            return true;
        }
    }
}

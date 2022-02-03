using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIDESwarmOrbitInfo))]
    internal class UIDESwarmOrbitInfo_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIDESwarmOrbitInfo.CalculateSailLifeDistribution))]
        public static bool CalculateSailLifeDistribution_Prefix(UIDESwarmOrbitInfo __instance, int division, out float maxCount, ref float[] __result)
        {
            maxCount = 0f;
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return true;
            }

            if (division <= 1 || __instance.swarm == null)
            {
                __result = null;
                return false;
            }
            ExpiryOrder[] expiryOrder = __instance.swarm.expiryOrder;
            long gameTick = GameMain.gameTick;
            float[] array = new float[division];
            float gap = GameMain.history.solarSailLife * 60f / division;
            for (int i = 0; i < expiryOrder.Length; i++)
            {
                if (expiryOrder[i].index != 0 && __instance.orbits.Contains((int)__instance.swarm.sailInfos[expiryOrder[i].index].orbit))
                {
                    // Make sure index is not out of array
                    int index = (int)((expiryOrder[i].time - gameTick) / gap);
                    index = index >= 0 ? index : 0;
                    index = index < division ? index : division - 1;
                    array[index] += 1f;
                }
            }
            for (int k = 0; k < array.Length; k++)
            {
                maxCount = ((maxCount > array[k]) ? maxCount : array[k]);
            }
            __result = array;
            return false;
        }
    }
}

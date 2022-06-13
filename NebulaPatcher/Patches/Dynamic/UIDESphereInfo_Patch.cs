using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIDESphereInfo))]
    internal class UIDESphereInfo_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIDESphereInfo.CalculateSailLifeDistribution))]
        public static bool CalculateSailLifeDistribution_Prefix(UIDESphereInfo __instance, int division, out float maxCount, ref float[] __result)
        {
            maxCount = 0f;
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return true;
            }

            if (division <= 1 || __instance.dysonSphere.swarm == null)
            {
                __result = null;
                return false;
            }
            ExpiryOrder[] expiryOrder = __instance.dysonSphere.swarm.expiryOrder;
            long gameTick = GameMain.gameTick;
            float[] array = new float[division];
            float gap = GameMain.history.solarSailLife * 60f / division;
            for (int i = 0; i < expiryOrder.Length; i++)
            {
                if (expiryOrder[i].index != 0)
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

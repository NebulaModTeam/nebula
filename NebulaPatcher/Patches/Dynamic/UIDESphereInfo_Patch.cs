#region

using System.Linq;
using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

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
        var expiryOrder = __instance.dysonSphere.swarm.expiryOrder;
        var gameTick = GameMain.gameTick;
        var array = new float[division];
        var gap = GameMain.history.solarSailLife * 60f / division;
        for (var i = 0; i < expiryOrder.Length; i++)
        {
            if (expiryOrder[i].index == 0)
            {
                continue;
            }
            // Make sure index is not out of array
            var index = (int)((expiryOrder[i].time - gameTick) / gap);
            index = index >= 0 ? index : 0;
            index = index < division ? index : division - 1;
            array[index] += 1f;
        }
        maxCount = array.Aggregate(0f, (current, t) => current > t ? current : t);
        __result = array;
        return false;
    }
}
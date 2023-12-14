#region

using System.Linq;
using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

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
        var expiryOrder = __instance.swarm.expiryOrder;
        var gameTick = GameMain.gameTick;
        var array = new float[division];
        var gap = GameMain.history.solarSailLife * 60f / division;
        for (var i = 0; i < expiryOrder.Length; i++)
        {
            if (expiryOrder[i].index == 0 ||
                !__instance.orbits.Contains((int)__instance.swarm.sailInfos[expiryOrder[i].index].orbit))
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
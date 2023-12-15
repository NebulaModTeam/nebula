#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(FactoryAudio))]
internal class FactoryAudio_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FactoryAudio.OnEntityBuild))]
    public static bool OnEntityBuild_Prefix(FactoryAudio __instance, int objId)
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.Factories.IsIncomingRequest.Value || objId <= 0)
        {
            return true;
        }
        var pos = __instance.planet.factory.entityPool[objId].pos;
        return !((pos - GameMain.mainPlayer.position).sqrMagnitude > __instance.planet.radius * __instance.planet.radius / 4);
        // Don't make sounds if distance is over half of the planet radius
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FactoryAudio.OnEntityDismantle))]
    public static bool OnEntityDismantle_Prefix(FactoryAudio __instance, int objId)
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            return true;
        }
        var pos = objId > 0
            ? __instance.planet.factory.entityPool[objId].pos
            : __instance.planet.factory.prebuildPool[-objId].pos;
        return !((pos - GameMain.mainPlayer.position).sqrMagnitude > __instance.planet.radius * __instance.planet.radius / 4);
        // Don't make sounds if distance is over half of the planet radius
    }
}

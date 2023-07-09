using HarmonyLib;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(FactoryAudio))]
    class FactoryAudio_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FactoryAudio.OnEntityBuild))]
        public static bool OnEntityBuild_Prefix(FactoryAudio __instance, int objId)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.Factories.IsIncomingRequest.Value && objId > 0)
            {
                Vector3 pos = __instance.planet.factory.entityPool[objId].pos;
                if ((pos - GameMain.mainPlayer.position).sqrMagnitude > (__instance.planet.radius * __instance.planet.radius / 4))
                {
                    // Don't make sounds if distance is over half of the planet radius
                    return false;
                }
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(FactoryAudio.OnEntityDismantle))]
        public static bool OnEntityDismantle_Prefix(FactoryAudio __instance, int objId)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                Vector3 pos;
                if (objId > 0)
                {
                    pos = __instance.planet.factory.entityPool[objId].pos;
                }
                else
                {
                    pos = __instance.planet.factory.prebuildPool[-objId].pos;
                }
                if ((pos - GameMain.mainPlayer.position).sqrMagnitude > (__instance.planet.radius * __instance.planet.radius / 4))
                {
                    // Don't make sounds if distance is over half of the planet radius
                    return false;
                }
            }
            return true;
        }
    }
}

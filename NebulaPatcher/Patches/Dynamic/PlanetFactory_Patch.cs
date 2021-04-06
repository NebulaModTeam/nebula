using HarmonyLib;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetFactory))]
    class BuildFinally_patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("BuildFinally")]
        public static bool BuildFinally_Prefix(PlanetFactory __instance, Player player, int prebuildId)
        {
            if (prebuildId != 0)
            {
                PrebuildData data = __instance.prebuildPool[prebuildId];
                if (data.id == prebuildId)
                {
                    OnEntityPlaced(data.protoId, data.pos, data.rot, false);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddPrebuildDataWithComponents")]
        public static bool AddPrebuildDataWithComponents_Prefix(PlanetFactory __instance, PrebuildData prebuild)
        {
            for (int i = 0; i < LocalPlayer.prebuildReceivedList.Count; i++)
            {
                foreach (PrebuildData pBuild in LocalPlayer.prebuildReceivedList.Keys)
                {
                    if (pBuild.pos == prebuild.pos && pBuild.rot == prebuild.rot)
                    {
                        return true;
                    }
                }
            }
            OnEntityPlaced(prebuild.protoId, prebuild.pos, prebuild.rot, true);
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GameTick")]
        public static bool InternalUpdate_Prefix()
        {
            StorageManager.IsHumanInput = false;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("GameTick")]
        public static void InternalUpdate_Postfix()
        {
            StorageManager.IsHumanInput = true;
        }

        private static void OnEntityPlaced(short protoId, Vector3 pos, Quaternion rot, bool isPrebuild)
        {
            var packet = new EntityPlaced(GameMain.localPlanet.id, protoId, pos, rot, isPrebuild);
            LocalPlayer.SendPacket(packet);
        }
    }
}

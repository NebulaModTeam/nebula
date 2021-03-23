using HarmonyLib;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetFactory), "BuildFinally")]
    class BuildFinally_patch
    {
        public static bool Prefix(PlanetFactory __instance, Player player, int prebuildId)
        {

            if (prebuildId != 0)
            {
                PrebuildData data = __instance.prebuildPool[prebuildId];
                if (data.id == prebuildId)
                {
                    OnEntityPlaced(data.protoId, data.pos, data.rot);
                }
            }

            return true;

        }

        private static void OnEntityPlaced(short protoId, Vector3 pos, Quaternion rot)
        {
            var packet = new EntityPlaced(protoId, pos, rot);
            LocalPlayer.SendPacket(packet);
        }
    }
}

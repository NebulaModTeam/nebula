using HarmonyLib;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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
                    sendPacket.OnEntityPlaced(data.protoId, data.pos, data.rot, false);
                }
            }

            return true;

        }
    }

    [HarmonyPatch(typeof(PlanetFactory), "AddPrebuildDataWithComponents")]
    class CreatePrebuildDataWithComponents_patch
    {
        public static bool Prefix(PlanetFactory __instance, PrebuildData prebuild)
        {
            for(int i = 0; i < LocalPlayer.prebuildReceivedList.Count; i++)
            {
                foreach(PrebuildData pBuild in LocalPlayer.prebuildReceivedList.Keys)
                {
                    if(pBuild.pos == prebuild.pos && pBuild.rot == prebuild.rot)
                    {
                        return true;
                    }
                }
            }
            sendPacket.OnEntityPlaced(prebuild.protoId, prebuild.pos, prebuild.rot, true);
            return true;
        }
    }
    class sendPacket
    {
        public static void OnEntityPlaced(short protoId, Vector3 pos, Quaternion rot, bool isPrebuild)
        {
            var packet = new EntityPlaced(GameMain.localPlanet.id, protoId, pos, rot, isPrebuild);
            LocalPlayer.SendPacket(packet);
        }
    }
}

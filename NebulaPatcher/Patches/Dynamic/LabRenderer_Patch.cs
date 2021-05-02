using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;
using System;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(LabRenderer))]
    class LabRenderer_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddInst")]
        public static bool AddInst_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.TargetPlanet == FactoryManager.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AlterInst", new Type[] { typeof(int), typeof(int), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
        public static bool AlterInst_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.TargetPlanet == FactoryManager.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveInst")]
        public static bool RemoveInst_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Initialized || FactoryManager.TargetPlanet == FactoryManager.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.TargetPlanet;
        }
    }
}

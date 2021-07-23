using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;
using System;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(InserterRenderer))]
    class InserterRenderer_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(InserterRenderer.AddInst), new Type[] { typeof(int), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
        [HarmonyPatch(nameof(InserterRenderer.AddInst), new Type[] { typeof(int), typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Quaternion), typeof(int), typeof(int), typeof(bool) })]
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

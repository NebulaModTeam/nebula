using HarmonyLib;
using NebulaAPI;
using NebulaWorld;
using NebulaWorld.Factory;
using System;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(ObjectRenderer))]
    class ObjectRenderer_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ObjectRenderer), nameof(ObjectRenderer.AddInst), new Type[] { typeof(int), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
        [HarmonyPatch(typeof(ObjectRenderer), nameof(ObjectRenderer.AddInst), new Type[] { typeof(int), typeof(Vector3), typeof(Quaternion), typeof(uint), typeof(bool) })]
        public static bool AddInst_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Instance.Initialized || FactoryManager.Instance.TargetPlanet == NebulaModAPI.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.Instance.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ObjectRenderer.AlterInst), new Type[] { typeof(int), typeof(int), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
        public static bool AlterInst_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Instance.Initialized || FactoryManager.Instance.TargetPlanet == NebulaModAPI.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.Instance.TargetPlanet;
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ObjectRenderer.AlterInst), new Type[] { typeof(int), typeof(int), typeof(Vector3), typeof(bool) })]
        public static bool AlterInst_Prefix2()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Instance.Initialized || FactoryManager.Instance.TargetPlanet == NebulaModAPI.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.Instance.TargetPlanet;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ObjectRenderer.RemoveInst))]
        public static bool RemoveInst_Prefix()
        {
            //Do not call renderer, if user is not on the planet as the request
            return !SimulatedWorld.Instance.Initialized || FactoryManager.Instance.TargetPlanet == NebulaModAPI.PLANET_NONE || GameMain.mainPlayer.planetId == FactoryManager.Instance.TargetPlanet;
        }
    }
}

#region

using HarmonyLib;
using NebulaAPI;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(InserterRenderer))]
internal class InserterRenderer_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(InserterRenderer), nameof(InserterRenderer.AddInst), typeof(int), typeof(Vector3), typeof(Quaternion),
        typeof(bool))]
    [HarmonyPatch(typeof(InserterRenderer), nameof(InserterRenderer.AddInst), typeof(int), typeof(Vector3), typeof(Quaternion),
        typeof(Vector3), typeof(Quaternion), typeof(int), typeof(int), typeof(bool))]
    public static bool AddInst_Prefix()
    {
        //Do not call renderer, if user is not on the planet as the request
        return !Multiplayer.IsActive || Multiplayer.Session.Factories.TargetPlanet == NebulaModAPI.PLANET_NONE ||
               GameMain.mainPlayer.planetId == Multiplayer.Session.Factories.TargetPlanet;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InserterRenderer.AlterInst), typeof(int), typeof(int), typeof(Vector3), typeof(Quaternion),
        typeof(bool))]
    public static bool AlterInst_Prefix()
    {
        //Do not call renderer, if user is not on the planet as the request
        return !Multiplayer.IsActive || Multiplayer.Session.Factories.TargetPlanet == NebulaModAPI.PLANET_NONE ||
               GameMain.mainPlayer.planetId == Multiplayer.Session.Factories.TargetPlanet;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InserterRenderer.RemoveInst))]
    public static bool RemoveInst_Prefix()
    {
        //Do not call renderer, if user is not on the planet as the request
        return !Multiplayer.IsActive || Multiplayer.Session.Factories.TargetPlanet == NebulaModAPI.PLANET_NONE ||
               GameMain.mainPlayer.planetId == Multiplayer.Session.Factories.TargetPlanet;
    }
}

#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GPUInstancingManager))]
internal class GPUInstancingManager_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GPUInstancingManager.AddModel))]
    public static bool AddModel_Prefix(ref int __result)
    {
        //Do not add model to the GPU queue if player is not on the same planet as building that was build
        if (!Multiplayer.IsActive || Multiplayer.Session.Factories.EventFactory == null ||
            Multiplayer.Session.Factories.EventFactory.planet == GameMain.localPlanet)
        {
            return true;
        }
        __result = 0;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GPUInstancingManager.AddPrebuildModel))]
    public static bool AddPrebuildModel_Prefix(ref int __result)
    {
        //Do not add model to the GPU queue if player is not on the same planet as building that was build
        if (!Multiplayer.IsActive || Multiplayer.Session.Factories.EventFactory == null ||
            Multiplayer.Session.Factories.EventFactory.planet == GameMain.localPlanet)
        {
            return true;
        }
        __result = 0;
        return false;
    }
}

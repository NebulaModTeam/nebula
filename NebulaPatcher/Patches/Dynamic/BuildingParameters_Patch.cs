#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(BuildingParameters))]
internal class BuildingParameters_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BuildingParameters.ApplyPrebuildParametersToEntity))]
    [HarmonyPatch(nameof(BuildingParameters.PasteToFactoryObject))]
    public static void ApplyPrebuildParametersToEntity_Prefix()
    {
        if (Multiplayer.IsActive)
        {
            // Let the upper patch handle the SetBans, SetFilter syncing
            Multiplayer.Session.Storage.IsHumanInput = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(BuildingParameters.ApplyPrebuildParametersToEntity))]
    [HarmonyPatch(nameof(BuildingParameters.PasteToFactoryObject))]
    public static void ApplyPrebuildParametersToEntity_Postfix()
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Storage.IsHumanInput = true;
        }
    }
}

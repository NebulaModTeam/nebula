#region

using HarmonyLib;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UITechNode))]
public class UITechNode_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITechNode.DeterminePrerequisiteSuffice))]
    public static void DeterminePrerequisiteSuffice_Postfix(ref bool __result)
    {
        // Skip metadata requirement of blueprint tech due to client can't get metadata currently
        __result = true;
    }
}

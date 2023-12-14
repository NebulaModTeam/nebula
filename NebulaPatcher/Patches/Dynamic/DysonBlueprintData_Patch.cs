#region

using HarmonyLib;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(DysonBlueprintData))]
internal class DysonBlueprintData_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DysonBlueprintData.FromBase64String))]
    public static void FromBase64String_Prefix()
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.DysonSpheres.InBlueprint = true;
        }
    }

#pragma warning disable Harmony003
    [HarmonyPostfix]
    [HarmonyPatch(nameof(DysonBlueprintData.FromBase64String))]
    public static void FromBase64String_Postfix(DysonBlueprintDataIOError __result, string str64Data,
        EDysonBlueprintType requestType, DysonSphere sphere, DysonSphereLayer layer)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        Multiplayer.Session.DysonSpheres.InBlueprint = false;
        if (Multiplayer.Session.DysonSpheres.IsIncomingRequest || __result != DysonBlueprintDataIOError.OK)
        {
            return;
        }
        var starIndex = sphere.starData.index;
        var layerId = layer?.id ?? -1;
        Multiplayer.Session.Network.SendPacket(new DysonBlueprintPacket(starIndex, layerId, requestType, str64Data));
    }
#pragma warning restore Harmony003
}

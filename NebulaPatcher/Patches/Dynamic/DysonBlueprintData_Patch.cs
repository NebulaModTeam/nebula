using HarmonyLib;
using NebulaAPI;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(DysonBlueprintData))]
    internal class DysonBlueprintData_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(DysonBlueprintData.ContentFromBase64String))]
        public static void ContentFromBase64String_Prefix()
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.DysonSpheres.InBlueprint = true;
            }            
        }

#pragma warning disable Harmony003
        [HarmonyPostfix]
        [HarmonyPatch(nameof(DysonBlueprintData.ContentFromBase64String))]
        public static void ContentFromBase64String_Postfix(DysonBlueprintDataIOError __result, string __0, EDysonBlueprintType __1, DysonSphere __2, DysonSphereLayer __3)
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.DysonSpheres.InBlueprint = false;
                if (!Multiplayer.Session.DysonSpheres.IsIncomingRequest && __result == DysonBlueprintDataIOError.OK)
                {
                    int starIndex = __2.starData.index;
                    int layerId = __3?.id ?? -1;
                    Multiplayer.Session.Network.SendPacket(new DysonBlueprintPacket(starIndex, layerId, __1, __0));
                }
            }
        }
#pragma warning restore Harmony003
    }
}

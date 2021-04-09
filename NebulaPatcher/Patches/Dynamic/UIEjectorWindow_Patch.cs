using HarmonyLib;
using NebulaModel.Packets.Factory.Ejector;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIEjectorWindow))]
    class UIEjectorWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnManualServingContentChange")]
        public static void OnManualServingContentChange_Postfix(UIEjectorWindow __instance)
        {
            //Notify about manual bullet inserting / withdrawing change
            StorageComponent storage = (StorageComponent)AccessTools.Field(typeof(UIEjectorWindow), "servingStorage").GetValue(__instance);
            LocalPlayer.SendPacketToLocalPlanet(new EjectorStorageUpdatePacket(__instance.ejectorId, storage.grids[0].count));
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnSetOrbit")]
        public static void OnSetOrbit_Postfix(UIEjectorWindow __instance, int orbitId)
        {
            //Notify about target orbit change
            LocalPlayer.SendPacketToLocalPlanet(new EjectorOrbitUpdatePacket(__instance.ejectorId, orbitId));
        }
    }
}

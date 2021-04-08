using HarmonyLib;
using NebulaModel.Packets.Factory.Silo;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UISiloWindow))]
    class UISiloWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnManualServingContentChange")]
        public static void OnManualServingContentChange_Postfix(UISiloWindow __instance)
        {
            //Notify about manual rockets inserting / withdrawing change
            StorageComponent storage = (StorageComponent)AccessTools.Field(typeof(UISiloWindow), "servingStorage").GetValue(__instance);
            LocalPlayer.SendPacketToLocalPlanet(new SiloStorageUpdatePacket(__instance.siloId, storage.grids[0].count));
        }
    }
}

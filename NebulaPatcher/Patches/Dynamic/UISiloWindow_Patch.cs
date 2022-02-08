using HarmonyLib;
using NebulaModel.Packets.Factory.Silo;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UISiloWindow))]
    internal class UISiloWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UISiloWindow.OnManualServingContentChange))]
        public static void OnManualServingContentChange_Postfix(UISiloWindow __instance)
        {
            //Notify about manual rockets inserting / withdrawing change
            if (Multiplayer.IsActive)
            {
                StorageComponent storage = __instance.servingStorage;
                Multiplayer.Session.Network.SendPacketToLocalStar(new SiloStorageUpdatePacket(__instance.siloId, storage.grids[0].count, storage.grids[0].inc, GameMain.localPlanet?.id ?? -1));
            }
        }
    }
}

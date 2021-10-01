using HarmonyLib;
using NebulaModel.Packets.Factory.Ejector;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIEjectorWindow))]
    internal class UIEjectorWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIEjectorWindow.OnManualServingContentChange))]
        public static void OnManualServingContentChange_Postfix(UIEjectorWindow __instance)
        {
            //Notify about manual bullet inserting / withdrawing change
            if (Multiplayer.IsActive)
            {
                StorageComponent storage = __instance.servingStorage;
                Multiplayer.Session.Network.SendPacketToLocalStar(new EjectorStorageUpdatePacket(__instance.ejectorId, storage.grids[0].count, GameMain.localPlanet?.id ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIEjectorWindow.OnSetOrbit))]
        public static void OnSetOrbit_Postfix(UIEjectorWindow __instance, int orbitId)
        {
            //Notify about target orbit change
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Network.SendPacketToLocalStar(new EjectorOrbitUpdatePacket(__instance.ejectorId, orbitId, GameMain.localPlanet?.id ?? -1));
            }
        }
    }
}

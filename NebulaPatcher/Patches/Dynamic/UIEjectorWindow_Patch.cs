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
            if (SimulatedWorld.Initialized)
            {
                StorageComponent storage = (StorageComponent)AccessTools.Field(typeof(UIEjectorWindow), "servingStorage").GetValue(__instance);
                LocalPlayer.SendPacketToLocalStar(new EjectorStorageUpdatePacket(__instance.ejectorId, storage.grids[0].count, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnSetOrbit")]
        public static void OnSetOrbit_Postfix(UIEjectorWindow __instance, int orbitId)
        {
            //Notify about target orbit change
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new EjectorOrbitUpdatePacket(__instance.ejectorId, orbitId, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }
    }
}

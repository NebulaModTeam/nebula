using HarmonyLib;
using NebulaModel.Packets.Factory.Inserter;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIInserterWindow))]
    class UIInserterWindow_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnResetFilterButtonClick")]
        public static void OnResetFilterButtonClick_Prefix(UIInserterWindow __instance)
        {
            //Notify about reseting inserter's filter
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new InserterFilterUpdatePacket(__instance.inserterId, 0, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnItemPickerReturn")]
        public static void OnItemPickerReturn_Prefix(UIInserterWindow __instance, ItemProto item)
        {
            //Notify about changing filter item
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new InserterFilterUpdatePacket(__instance.inserterId, (item != null) ? item.ID : 0, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }
    }
}

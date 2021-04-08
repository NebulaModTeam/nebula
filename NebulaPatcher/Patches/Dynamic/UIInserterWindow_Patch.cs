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
            LocalPlayer.SendPacketToLocalPlanet(new InserterFilterUpdatePacket(__instance.inserterId, 0));
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnItemPickerReturn")]
        public static void OnItemPickerReturn_Prefix(UIInserterWindow __instance, ItemProto item)
        {
            //Notify about changing filter item
            LocalPlayer.SendPacketToLocalPlanet(new InserterFilterUpdatePacket(__instance.inserterId, (item != null) ? item.ID : 0));
        }
    }
}

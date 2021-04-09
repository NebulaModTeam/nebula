using HarmonyLib;
using NebulaModel.Packets.Factory.Splitter;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UISplitterWindow))]
    class UISplitterWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnItemPickerReturn")]
        public static void OnItemPickerReturn_Postfix(UISplitterWindow __instance, ItemProto item)
        {
            //Send notification about changing splitter output filter
            LocalPlayer.SendPacketToLocalPlanet(new SplitterFilterChangePacket(__instance.splitterId, ((item != null) ? item.ID : 0)));
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnCircleFilterRightClick")]
        public static void OnCircleFilterRightClick_Prefix(UISplitterWindow __instance, int slot)
        {
            //Send notification about reseting splitter output filter, if user rightclicked on output node with filter
            SplitterComponent thisComponent = __instance.traffic.splitterPool[__instance.splitterId];
            bool sendResetOutputFilter = slot == 0 && thisComponent.output0 != 0 ||
                                         slot == 1 && thisComponent.output1 != 0 ||
                                         slot == 2 && thisComponent.output2 != 0 ||
                                         slot == 3 && thisComponent.output3 != 0;

            if (sendResetOutputFilter && thisComponent.outFilter != 0)
            {
                LocalPlayer.SendPacketToLocalPlanet(new SplitterFilterChangePacket(__instance.splitterId, 0));
            }
        }
    }
}

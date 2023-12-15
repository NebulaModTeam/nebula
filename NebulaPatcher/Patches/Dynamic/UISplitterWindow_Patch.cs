#region

using HarmonyLib;
using NebulaModel.Packets.Factory.Splitter;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UISplitterWindow))]
internal class UISplitterWindow_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UISplitterWindow.OnItemPickerReturn))]
    public static void OnItemPickerReturn_Postfix(UISplitterWindow __instance, ItemProto item)
    {
        //Send notification about changing splitter output filter
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new SplitterFilterChangePacket(__instance.splitterId,
                item?.ID ?? 0, GameMain.localPlanet?.id ?? -1));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UISplitterWindow.OnCircleFilterClick))]
    public static void OnCircleFilterRightClick_Prefix(UISplitterWindow __instance, int slot)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        //Send notification about reseting splitter output filter, if user rightclicked on output node with filter
        var thisComponent = __instance.traffic.splitterPool[__instance.splitterId];
        var sendResetOutputFilter = slot == 0 && thisComponent.output0 != 0 ||
                                    slot == 1 && thisComponent.output1 != 0 ||
                                    slot == 2 && thisComponent.output2 != 0 ||
                                    slot == 3 && thisComponent.output3 != 0;

        if (sendResetOutputFilter && thisComponent.outFilter != 0)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new SplitterFilterChangePacket(__instance.splitterId, 0,
                GameMain.localPlanet?.id ?? -1));
        }
    }
}

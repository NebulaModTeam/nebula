#region

using HarmonyLib;
using NebulaModel.Packets.Factory.Inserter;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIInserterWindow))]
internal class UIInserterWindow_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIInserterWindow.OnItemPickerReturn))]
    [HarmonyPatch(nameof(UIInserterWindow.OnResetFilterButtonClick))]
    public static void OnFilterChange_Postfix(UIInserterWindow __instance)
    {
        //Notify about chaning inserter's filter
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new InserterFilterUpdatePacket(__instance.inserterId,
                __instance.factorySystem.inserterPool[__instance.inserterId].filter, __instance.factory.planetId));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIInserterWindow.OnTakeBackButtonClick))]
    public static void OnTakeBackButtonClick_Postfix(UIInserterWindow __instance)
    {
        //Notify about taking inserter buffer item
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new InserterItemUpdatePacket(
                in __instance.factorySystem.inserterPool[__instance.inserterId], __instance.factory.planetId));
        }
    }
}

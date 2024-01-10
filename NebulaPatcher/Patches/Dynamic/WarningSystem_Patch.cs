#region

using HarmonyLib;
using NebulaModel.Packets.Warning;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(WarningSystem))]
internal class WarningSystem_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(WarningSystem), nameof(WarningSystem.RemoveWarningData))]
    [HarmonyPatch(typeof(WarningSystem), nameof(WarningSystem.WarningLogic))]
    public static bool AlterWarningData_Prefix()
    {
        //Let warningPool only be updated by packet
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WarningSystem), nameof(WarningSystem.NewWarningData))]
    public static void NewWarningData_Prefix(WarningSystem __instance, ref int factoryId)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        // Stop the code to access unreachable factory
        factoryId = -1;
        // Let it return a dummy WarningData pool[0]
        __instance.warningRecycleCursor = 1;
        __instance.warningRecycle[0] = 0;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WarningSystem), nameof(WarningSystem.CalcFocusDetail))]
    public static void CalcFocusDetail_Prefix(int __0)
    {
        if (__0 == 0 || !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        if (Multiplayer.Session.Warning.TickSignal == Multiplayer.Session.Warning.TickData)
        {
            return;
        }
        if (GameMain.gameTick - Multiplayer.Session.Warning.LastRequestTime <= 240)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(new WarningDataRequest(WarningRequestEvent.Data));
        Multiplayer.Session.Warning.LastRequestTime = GameMain.gameTick;
    }
}

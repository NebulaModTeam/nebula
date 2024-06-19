#region

using HarmonyLib;
using NebulaModel.Packets.Combat;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIDFCommunicatorWindow))]
internal class UIDFCommunicatorWindow_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIDFCommunicatorWindow._OnOpen))]
    public static void OnOpen_Prefix(UIDFCommunicatorWindow __instance, ref bool __state)
    {
        if (!Multiplayer.IsActive) return;

        //Set this.isSandbox = true to remove metadata cost in multiplayer
        __state = __instance.gameData.gameDesc.isSandboxMode;
        __instance.gameData.gameDesc.isSandboxMode = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIDFCommunicatorWindow._OnOpen))]
    public static void OnOpen_Postfix(UIDFCommunicatorWindow __instance, bool __state)
    {
        if (!Multiplayer.IsActive) return;

        __instance.gameData.gameDesc.isSandboxMode = __state;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIDFCommunicatorWindow.OnTruceButtonClicked))]
    public static void OnTruceButtonClicked_Prefix(ref long __state)
    {
        if (!Multiplayer.IsActive) return;

        __state = GameMain.history.dfTruceTimer;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIDFCommunicatorWindow.OnTruceButtonClicked))]
    public static void OnTruceButtonClicked_Postfix(long __state)
    {
        if (!Multiplayer.IsActive) return;

        if (__state != GameMain.history.dfTruceTimer)
        {
            //If truce is signed, broadcast to other players
            var truceEndTime = GameMain.gameTick + GameMain.history.dfTruceTimer;
            Multiplayer.Session.Network.SendPacket(new CombatTruceUpdatePacket(
                Multiplayer.Session.LocalPlayer.Id, truceEndTime));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIDFCommunicatorWindow.WithdrawTruceConfirm))]
    public static void WithdrawTruceConfirm_Postfix()
    {
        if (!Multiplayer.IsActive) return;

        Multiplayer.Session.Network.SendPacket(new CombatTruceUpdatePacket(
            Multiplayer.Session.LocalPlayer.Id, GameMain.gameTick));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIDFCommunicatorWindow.OnAggressiveIncButtonClicked))]
    [HarmonyPatch(nameof(UIDFCommunicatorWindow.OnAggressiveDecButtonClicked))]
    public static void OnAggressiveButtonClicked_Prefix(ref float __state)
    {
        if (!Multiplayer.IsActive) return;

        __state = GameMain.data.history.combatSettings.aggressiveness;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIDFCommunicatorWindow.OnAggressiveIncButtonClicked))]
    [HarmonyPatch(nameof(UIDFCommunicatorWindow.OnAggressiveDecButtonClicked))]
    public static void OnAggressiveButtonClicked_Postfix(float __state)
    {
        if (!Multiplayer.IsActive) return;

        var history = GameMain.history;
        if (__state != history.combatSettings.aggressiveness)
        {
            //If aggressiveness has changed, broadcast to other players
            Multiplayer.Session.Network.SendPacket(new CombatAggressivenessUpdatePacket(
                Multiplayer.Session.LocalPlayer.Id, history.combatSettings.aggressiveness));
        }
    }
}

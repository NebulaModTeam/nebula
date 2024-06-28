#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using HarmonyLib;
using NebulaModel.Packets.Players;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;
using NebulaWorld.GameStates;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIEscMenu))]
internal class UIEscMenu_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIEscMenu._OnOpen))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnOpen_Prefix(UIEscMenu __instance)
    {
        // Disable save game button if you are a client in a multiplayer session
        var saveGameWindowButton = __instance.button2;
        SetButtonEnableState(saveGameWindowButton, !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost);

        // Disable load game button if in a multiplayer session
        var loadGameWindowButton = __instance.button3;
        SetButtonEnableState(loadGameWindowButton, !Multiplayer.IsActive);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIEscMenu._OnOpen))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnOpen_Postfix(UIEscMenu __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost) return;

        var timeSinceSave = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - GameStatesManager.LastSaveTime;
        var second = (int)(timeSinceSave);
        var minute = second / 60;
        var hour = minute / 60;
        var saveBtnText = "存档时间".Translate() + $" {hour}h{minute % 60}m{second % 60}s ago";
        __instance.button2Text.text = saveBtnText;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIEscMenu.OnButton5Click))]
    public static void OnButton5Click_Prefix()
    {
        QuitGame();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIEscMenu.OnButton6Click))]
    public static void OnButton6Click_Prefix()
    {
        QuitGame();
    }

    private static void QuitGame()
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            // Because GameSave.SaveAsLastExit() is disable, we have to save game here to match the vanilla behavior.
            GameSave.SaveCurrentGame(GameSave.LastExit);
        }
        else if (GameMain.mainPlayer?.mecha != null)
        {
            GameMain.mainPlayer.mecha.lab.ManageTakeback(); // Refund items to player package
            Multiplayer.Session.Network.SendPacket(new PlayerMechaData(GameMain.mainPlayer));
            Thread.Sleep(100); // Wait for async packet send
        }
        PlanetFactory_Transpiler.CheckPopupPresent.Clear();
        PlanetFactory_Transpiler.FaultyVeins.Clear();
        Multiplayer.LeaveGame();
    }

    private static void SetButtonEnableState(Selectable button, bool enable)
    {
        var buttonColors = button.colors;
        buttonColors.disabledColor = new Color(1f, 1f, 1f, 0.15f);
        button.interactable = enable;
        button.colors = buttonColors;
    }
}

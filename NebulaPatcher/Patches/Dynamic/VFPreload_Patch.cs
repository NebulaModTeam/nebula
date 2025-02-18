#region

using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Utils;
using NebulaWorld;
using NebulaWorld.GameStates;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(VFPreload))]
internal class VFPreload_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(VFPreload), nameof(VFPreload.InvokeOnLoad))]
    public static void InvokeOnLoad_Postfix()
    {
        if (!Multiplayer.IsDedicated)
        {
            return;
        }
        VFAudio.audioVolume = 0f;
        NativeInterop.HideWindow();
        NativeInterop.SetConsoleCtrlHandler();
        // Logging to provide progression to user
        Log.Info($"Loading game version {GameConfig.gameVersion.ToFullString()}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VFPreload), nameof(VFPreload.InvokeOnLoadHalf))]
    public static void InvokeOnLoadHalf_Postfix()
    {
        if (Multiplayer.IsDedicated)
        {
            Log.Info("VFPreload.InvokeOnLoadHalf");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(VFPreload.InvokeOnLoadWorkEnded))]
    public static void InvokeOnLoadWorkEnded_Postfix()
    {
        if (GameStatesManager.DuringReconnect)
        {
            var ip = "127.0.0.1:8469";
            if (Config.Options.RememberLastIP && !string.IsNullOrWhiteSpace(Config.Options.LastIP))
            {
                ip = Config.Options.LastIP;
            }

            var password = "";
            if (Config.Options.RememberLastClientPassword && !string.IsNullOrWhiteSpace(Config.Options.LastClientPassword))
            {
                password = Config.Options.LastClientPassword;
            }

            UIMainMenu_Patch.JoinGame(ip, password);
            GameStatesManager.DuringReconnect = false;
        }

        if (!Multiplayer.IsDedicated)
        {
            return;
        }
        if (GameStatesManager.ImportedSaveName != null)
        {
            NebulaPlugin.StartDedicatedServer(GameStatesManager.ImportedSaveName);
        }
        else if (GameStatesManager.NewGameDesc != null)
        {
            NebulaPlugin.StartDedicatedServer(GameStatesManager.NewGameDesc);
        }
        else
        {
            Log.Warn("No game start option provided!");
        }
    }
}

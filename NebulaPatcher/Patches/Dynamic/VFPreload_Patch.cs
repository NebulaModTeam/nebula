using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaWorld;
using NebulaWorld.GameStates;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(VFPreload))]
    internal class VFPreload_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VFPreload), nameof(VFPreload.InvokeOnLoad))]
        public static void InvokeOnLoad_Postfix()
        {
            if (Multiplayer.IsDedicated)
            {
                // Logging to provide progression to user
                Log.Info("VFPreload.InvokeOnLoad");
            }
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
                string ip = "127.0.0.1:8469";
                if (Config.Options.RememberLastIP && !string.IsNullOrWhiteSpace(Config.Options.LastIP))
                {
                    ip = Config.Options.LastIP;
                }

                UIMainMenu_Patch.JoinGame(ip);
                GameStatesManager.DuringReconnect = false;
            }

            if (Multiplayer.IsDedicated)
            {
                NebulaPlugin.StartDedicatedServer();
            }
        }
    }
}

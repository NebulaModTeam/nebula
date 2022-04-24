using HarmonyLib;
using NebulaModel;
using NebulaWorld.GameStates;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(VFPreload))]
    internal class VFPreload_Patch
    {
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
        }
    }
}

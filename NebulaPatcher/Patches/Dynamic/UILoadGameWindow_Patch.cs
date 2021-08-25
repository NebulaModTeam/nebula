using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaNetwork;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UILoadGameWindow))]
    class UILoadGameWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("DoLoadSelectedGame")]
        public static void DoLoadSelectedGame_Postfix()
        {
            if (MainMenuManager.IsInMultiplayerMenu)
            {
                Log.Info($"Listening server on port {Config.Options.HostPort}");
                Multiplayer.HostGame(new Server(Config.Options.HostPort, true));
            }
        }
    }
}

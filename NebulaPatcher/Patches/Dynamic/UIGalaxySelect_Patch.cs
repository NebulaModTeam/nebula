using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaNetwork;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIGalaxySelect))]
    class UIGalaxySelect_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIGalaxySelect.EnterGame))]
        public static bool EnterGame_Prefix(UIGalaxySelect __instance)
        {
            if (MainMenuManager.IsInMultiplayerMenu)
            {
                Log.Info($"Listening server on port {Config.Options.HostPort}");
                Multiplayer.HostGame(new Server(Config.Options.HostPort));
                
                GameDesc gameDesc = AccessTools.Field(__instance.GetType(), "gameDesc").GetValue(__instance) as GameDesc;
                DSPGame.StartGameSkipPrologue(gameDesc);
                return false;
            }

            return true;
        }
    }
}

using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaPatcher.MonoBehaviours;

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
                var session = NebulaBootstrapper.Instance.CreateMultiplayerHostSession();
                session.StartServer(Config.Options.HostPort);

                GameDesc gameDesc = __instance.gameDesc;
                DSPGame.StartGameSkipPrologue(gameDesc);
                return false;
            }

            return true;
        }
    }
}

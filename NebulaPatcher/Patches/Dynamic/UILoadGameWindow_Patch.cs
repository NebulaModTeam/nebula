using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaPatcher.MonoBehaviours;

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
                var session = NebulaBootstrapper.Instance.CreateMultiplayerHostSession();
                session.StartServer(Config.Options.HostPort, true);
            }
        }
    }
}

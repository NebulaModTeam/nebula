using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaPatcher.MonoBehaviours;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UILoadGameWindow))]
    class UILoadGameWindow_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("DoLoadSelectedGame")]
        public static void DoLoadSelectedGame_Prefix()
        {
            if (MainMenuManager.IsInMultiplayerMenu)
            {
                Log.Info($"Listening server on port {Config.DefaultPort}");
                var session = NebulaBootstrapper.Instance.CreateMultiplayerHostSession();
                session.StartServer(Config.DefaultPort);
            }
        }
    }
}

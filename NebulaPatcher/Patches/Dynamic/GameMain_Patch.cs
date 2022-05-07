using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
using NebulaWorld.SocialIntegration;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameMain))]
    public class GameMain_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameMain.HandleApplicationQuit))]
        public static void HandleApplicationQuit_Prefix()
        {
            if (Multiplayer.IsActive)
            {
                Log.Warn("Multiplayer is still running, closing now...");
                Multiplayer.LeaveGame();
            }
            DiscordManager.Cleanup();
        }
    }
}

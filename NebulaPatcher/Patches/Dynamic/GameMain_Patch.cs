#region

using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
using NebulaWorld.SocialIntegration;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

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

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameMain.DetermineGameTickRate))]
    public static void DetermineGameTickRate_Postfix(ref int __result)
    {
        // If in multiplayer game and Multiplayer.Session.CanPause is false, prevent the pause
        if (Multiplayer.Session != null && Multiplayer.Session.CanPause == false)
        {
            __result = 1;
        }
    }
}

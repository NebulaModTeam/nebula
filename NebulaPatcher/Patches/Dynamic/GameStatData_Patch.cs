#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GameStatData))]
internal class GameStatData_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameStatData.AfterTick))]
    public static void AfterTick_Prefix(GameStatData __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        if (GameMain.history.currentTech == 0)
        {
            return;
        }
        var hostTechHashedFor10Frames = Multiplayer.Session.Statistics.TechHashedFor10Frames;
        __instance.techHashedThisFrame = hostTechHashedFor10Frames / 10;
        if (GameMain.gameTick % 10 < hostTechHashedFor10Frames % 10)
        {
            ++__instance.techHashedThisFrame;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameStatData.AfterTick))]
    public static void AfterTick_Postfix()
    {
        if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
        {
            Multiplayer.Session.Statistics.CaptureStatisticalSnapshot();
        }
    }
}

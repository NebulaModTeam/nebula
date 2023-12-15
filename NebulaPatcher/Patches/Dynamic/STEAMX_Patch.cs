#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(STEAMX))]
internal class STEAMX_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(STEAMX.UploadScoreToLeaderboard))]
    public static bool UploadScoreToLeaderboard_Prefix()
    {
        // We don't want to upload steam leaderboard data if we are playing MP
        return !Multiplayer.IsActive;
    }
}

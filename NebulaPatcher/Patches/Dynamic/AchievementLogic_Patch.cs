#region

using HarmonyLib;
using NebulaModel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(AchievementLogic))]
internal class AchievementLogic_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AchievementLogic.isSelfFormalGame), MethodType.Getter)]
    public static bool IsSelfFormalGame_Prefix(ref bool __result)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        // Decide to enable achievement or not in multiplayer game
        __result = Config.Options.EnableAchievement;
        return false;
    }
}

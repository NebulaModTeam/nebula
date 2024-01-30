#region

using HarmonyLib;
using NebulaModel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(AbnormalityLogic))]
internal class AbnormalityLogic_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AbnormalityLogic.GameTick))]
    public static bool GameTick_Prefix()
    {
        return !Multiplayer.IsActive || !Config.Options.EnableAchievement;
    }
}

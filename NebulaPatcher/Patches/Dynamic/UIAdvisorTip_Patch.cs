using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIAdvisorTip))]
    class UIAdvisorTip_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("PlayAdvisorTip")]
        public static bool PlayAdvisorTip_Prefix()
        {
            // do nothing in single player mode
            if (!SimulatedWorld.Initialized && !SimulatedWorld.ExitingMultiplayerSession)
                return true;

            return !Config.Options.AdvisorDisabled;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RunAdvisorTip")]
        public static bool RunAdvisorTip_Prefix()
        {
            // do nothing in single player mode
            if (!SimulatedWorld.Initialized && !SimulatedWorld.ExitingMultiplayerSession)
                return true;

            return !Config.Options.AdvisorDisabled;
        }
    }
}

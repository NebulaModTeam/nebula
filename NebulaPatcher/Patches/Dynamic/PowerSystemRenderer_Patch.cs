using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PowerSystemRenderer))]
    class PowerSystemRenderer_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Init")]
        public static void Init_Postfix(UIGameMenu __instance)
        {
            if (!SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient)
            {
                return;
            }

            AccessTools.StaticFieldRefAccess<bool>(typeof(PowerSystemRenderer), "powerGraphOn") = Config.Options.PowerGridEnabled;
        }
    }
}

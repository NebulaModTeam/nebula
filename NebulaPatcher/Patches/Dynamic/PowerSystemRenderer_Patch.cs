using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PowerSystemRenderer))]
    class PowerSystemRenderer_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PowerSystemRenderer.Init))]
        public static void Init_Postfix(UIGameMenu __instance)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            PowerSystemRenderer.powerGraphOn = Config.Options.PowerGridEnabled;
        }
    }
}

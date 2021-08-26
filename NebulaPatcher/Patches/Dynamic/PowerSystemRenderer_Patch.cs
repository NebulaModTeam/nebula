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
        public static void Init_Postfix()
        {
            if (!SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient)
            {
                return;
            }

            PowerSystemRenderer.powerGraphOn = Config.Options.PowerGridEnabled;
        }
    }
}

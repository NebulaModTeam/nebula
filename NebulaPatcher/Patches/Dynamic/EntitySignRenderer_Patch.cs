using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(EntitySignRenderer))]
    class EntitySignRenderer_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(EntitySignRenderer.Init))]
        public static void Init_Postfix()
        {
            if (!SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient)
            {
                return;
            }

            EntitySignRenderer.showIcon = Config.Options.BuildingIconEnabled;
            EntitySignRenderer.showSign = Config.Options.BuildingWarningEnabled;
        }
    }
}

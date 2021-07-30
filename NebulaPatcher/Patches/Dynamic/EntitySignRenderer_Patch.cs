using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(EntitySignRenderer))]
    class EntitySignRenderer_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Init")]
        public static void Init_Postfix()
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            EntitySignRenderer.showIcon = Config.Options.BuildingIconEnabled;
            EntitySignRenderer.showSign = Config.Options.BuildingWarningEnabled;
        }
    }
}

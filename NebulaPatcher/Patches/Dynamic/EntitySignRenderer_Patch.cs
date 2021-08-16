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
            if (!SimulatedWorld.Initialized || LocalPlayer.Instance.IsMasterClient)
            {
                return;
            }

            AccessTools.StaticFieldRefAccess<bool>(typeof(EntitySignRenderer), "showIcon") = Config.Options.BuildingIconEnabled;
            AccessTools.StaticFieldRefAccess<bool>(typeof(EntitySignRenderer), "showSign") = Config.Options.BuildingWarningEnabled;
        }
    }
}

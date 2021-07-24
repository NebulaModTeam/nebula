using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Factory;
using UnityEngine.UI;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIGameMenu))]
    class UIDetailsView_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnDfGuideButtonClick")]
        public static void OnDfGuideButtonClick_Postfix(UIGameMenu __instance)
        {
            Config.Options.SpaceNavigationEnabled = __instance.uiGame.dfSpaceGuideOn;
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDfIconButtonClick")]
        public static void OnDfIconButtonClick_Postfix(UIGameMenu __instance)
        {
            Config.Options.BuildingIconEnabled = (bool)AccessTools.StaticFieldRefAccess<bool>(typeof(EntitySignRenderer), "showIcon");
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDfLightButtonClick")]
        public static void OnDfLightButtonClick_Postfix(UIGameMenu __instance)
        {
            Config.Options.GuidingLightEnabled = (bool)AccessTools.StaticFieldRefAccess<bool>(typeof(PowerSystemRenderer), "powerGraphOn");
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDfPowerButtonClick")]
        public static void OnDfPowerButtonClick_Postfix(UIGameMenu __instance)
        {
            Config.Options.PowerGridEnabled = (bool)AccessTools.StaticFieldRefAccess<bool>(typeof(PostEffectController), "headlight");
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDfSignButtonClick")]
        public static void OnDfSignButtonClick_Postfix(UIGameMenu __instance)
        {
            Config.Options.BuildingWarningEnabled = (bool)AccessTools.StaticFieldRefAccess<bool>(typeof(EntitySignRenderer), "showSign");
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDfVeinButtonClick")]
        public static void OnDfVeinButtonClick_Postfix(UIGameMenu __instance)
        {
            Config.Options.VeinDistributionEnabled = __instance.uiGame.dfVeinOn;
            Config.SaveOptions();
        }
    }
}

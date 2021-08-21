using HarmonyLib;
using NebulaModel;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIGameMenu))]
    class UIGameMenu_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIGameMenu.OnDfGuideButtonClick))]
        public static void OnDfGuideButtonClick_Postfix(UIGameMenu __instance)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Config.Options.SpaceNavigationEnabled = __instance.uiGame.dfSpaceGuideOn;
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIGameMenu.OnDfIconButtonClick))]
        public static void OnDfIconButtonClick_Postfix()
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Config.Options.BuildingIconEnabled = EntitySignRenderer.showIcon;
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIGameMenu.OnDfLightButtonClick))]
        public static void OnDfLightButtonClick_Postfix()
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Config.Options.GuidingLightEnabled = PowerSystemRenderer.powerGraphOn;
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIGameMenu.OnDfPowerButtonClick))]
        public static void OnDfPowerButtonClick_Postfix()
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Config.Options.PowerGridEnabled = PostEffectController.headlight;
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIGameMenu.OnDfSignButtonClick))]
        public static void OnDfSignButtonClick_Postfix()
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Config.Options.BuildingWarningEnabled = EntitySignRenderer.showSign;
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIGameMenu.OnDfVeinButtonClick))]
        public static void OnDfVeinButtonClick_Postfix(UIGameMenu __instance)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Config.Options.VeinDistributionEnabled = __instance.uiGame.dfVeinOn;
            Config.SaveOptions();
        }
    }
}

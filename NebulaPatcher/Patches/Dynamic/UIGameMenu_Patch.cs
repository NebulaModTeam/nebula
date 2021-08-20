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
    class UIGameMenu_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnDfGuideButtonClick")]
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
        [HarmonyPatch("OnDfIconButtonClick")]
        public static void OnDfIconButtonClick_Postfix(UIGameMenu __instance)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Config.Options.BuildingIconEnabled = EntitySignRenderer.showIcon;
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDfLightButtonClick")]
        public static void OnDfLightButtonClick_Postfix(UIGameMenu __instance)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Config.Options.GuidingLightEnabled = PowerSystemRenderer.powerGraphOn;
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDfPowerButtonClick")]
        public static void OnDfPowerButtonClick_Postfix(UIGameMenu __instance)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Config.Options.PowerGridEnabled = PostEffectController.headlight;
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDfSignButtonClick")]
        public static void OnDfSignButtonClick_Postfix(UIGameMenu __instance)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Config.Options.BuildingWarningEnabled = EntitySignRenderer.showSign;
            Config.SaveOptions();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDfVeinButtonClick")]
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

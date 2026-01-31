#region

using HarmonyLib;
using NebulaModel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIGameMenu))]
internal class UIGameMenu_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIGameMenu.OnDfGuideButtonClick))]
    [HarmonyPatch(nameof(UIGameMenu.OnDfIconButtonClick))]
    [HarmonyPatch(nameof(UIGameMenu.OnDfLightButtonClick))]
    [HarmonyPatch(nameof(UIGameMenu.OnDfPowerButtonClick))]
    [HarmonyPatch(nameof(UIGameMenu.OnDfSignButtonClick))]
    [HarmonyPatch(nameof(UIGameMenu.OnDfVeinButtonClick))]
    [HarmonyPatch(nameof(UIGameMenu.OnDfDefenseButtonClick))]
    [HarmonyPatch(nameof(UIGameMenu.OnDfHpBarButtonClick))]
    public static void SaveDetailOptions()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        var uiGame = UIRoot.instance.uiGame;

        Config.Options.ShowDetailPowerGrid = PowerSystemRenderer.powerGraphOn;
        Config.Options.ShowDetailVeinDistribution = uiGame.dfVeinOn;
        Config.Options.ShowDetailSpaceNavigation = uiGame.dfSpaceGuideOn;
        Config.Options.ShowDetailDefenseArea = DefenseSystemRenderer.turretGraphOn;

        Config.Options.ShowDetailBuildingAlarm = EntitySignRenderer.showSign;
        Config.Options.ShowDetailBuildingIcon = EntitySignRenderer.showIcon;
        Config.Options.ShowGuidingLight = PostEffectController.headlight;
        if (GameMain.sectorModel != null)
        {
            Config.Options.ShowDetailHpBars = !GameMain.sectorModel.disableHPBars;
        }
        Config.SaveOptions();
    }
}

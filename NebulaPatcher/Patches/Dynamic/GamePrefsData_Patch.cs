#region

using HarmonyLib;
using NebulaModel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GamePrefsData))]
internal class GamePrefsData_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GamePrefsData.Restore))]
    public static void Restore_Postfix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        NebulaModel.Logger.Log.Debug("Apply save prefs");
        var uiGame = UIRoot.instance.uiGame;
        PowerSystemRenderer.powerGraphOn = Config.Options.ShowDetailPowerGrid;
        uiGame.dfVeinOn = Config.Options.ShowDetailVeinDistribution;
        uiGame.dfSpaceGuideOn = Config.Options.ShowDetailSpaceNavigation;
        DefenseSystemRenderer.turretGraphOn = Config.Options.ShowDetailDefenseArea;
        EntitySignRenderer.showSign = Config.Options.ShowDetailBuildingAlarm;
        EntitySignRenderer.showIcon = Config.Options.ShowDetailBuildingIcon;
        PostEffectController.headlight = Config.Options.ShowGuidingLight;
        if (GameMain.sectorModel != null)
        {
            GameMain.sectorModel.disableHPBars = !Config.Options.ShowDetailHpBars;
        }
    }
}

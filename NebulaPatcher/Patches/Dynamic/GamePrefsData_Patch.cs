#region

using System.Collections.Generic;
using HarmonyLib;
using NebulaModel;
using NebulaWorld;
using NebulaWorld.GameStates;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GamePrefsData))]
internal class GamePrefsData_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GamePrefsData.Restore))]
    public static void Restore_Postfix()
    {
        if (Multiplayer.IsActive && !Multiplayer.Session.LocalPlayer.IsHost)
        {
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

        var replicator = UIRoot.instance?.uiGame?.replicator;
        if (replicator != null)
        {
            GameStatesManager.RestoreReplicatorMultipliers(replicator);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GamePrefsData.Collect))]
    public static void Collect_Prefix(GamePrefsData __instance)
    {
        var replicator = UIRoot.instance?.uiGame?.replicator;
        if (replicator != null && replicator.multipliers != null && replicator.multipliers.Count > 0)
        {
            __instance.replicatorMultipliers ??= new Dictionary<int, int>();
            foreach (var kv in replicator.multipliers)
            {
                __instance.replicatorMultipliers[kv.Key] = kv.Value;
            }
        }
    }
}

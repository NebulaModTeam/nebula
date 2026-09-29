#region

using HarmonyLib;
using NebulaWorld;
using NebulaWorld.GameStates;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(AdvisorLogic))]
public class AdvisorLogic_Patch
{
    private const int MAX_ADVISOR_TIP_ID = FeatureID.ADVISOR_TIP_USED_START - FeatureID.ADVISOR_TIP_START; // 1000

    [HarmonyPostfix]
    [HarmonyPatch(nameof(AdvisorLogic.SetAdvisorTipFinished))]
    public static void SetAdvisorTipFinished_Postfix(AdvisorLogic __instance, int tipId)
    {
        if (tipId < 0 || tipId >= MAX_ADVISOR_TIP_ID || !Multiplayer.IsActive || Multiplayer.Session.History.IsIncomingRequest)
        {
            return;
        }

        int featureId = FeatureID.ADVISOR_TIP_START + tipId;
        GameStatesManager.PreserveFeatureKey(featureId);
        var history = __instance?.gameData?.history ?? GameMain.data?.history;
        history?.RegFeatureKey(featureId);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(AdvisorLogic.SetAdvisorTipUsed))]
    public static void SetAdvisorTipUsed_Postfix(AdvisorLogic __instance, int tipId)
    {
        if (tipId < 0 || tipId >= MAX_ADVISOR_TIP_ID || !Multiplayer.IsActive || Multiplayer.Session.History.IsIncomingRequest)
        {
            return;
        }

        int featureId = FeatureID.ADVISOR_TIP_USED_START + tipId;
        GameStatesManager.PreserveFeatureKey(featureId);
        var history = __instance?.gameData?.history ?? GameMain.data?.history;
        history?.RegFeatureKey(featureId);
    }
}

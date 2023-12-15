#region

using HarmonyLib;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(MilestoneSystem))]
internal class MilestoneSystem_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MilestoneSystem.UnlockMilestone))]
    public static void UnlockMilestone_Postfix(MilestoneSystem __instance, int id, long unlockTick)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Statistics.IsIncomingRequest)
        {
            return;
        }

        if (!__instance.milestoneDatas.TryGetValue(id, out var milestoneData))
        {
            return;
        }
        var patternId = milestoneData.journalData.patternId;
        var parameters = milestoneData.journalData.parameters;
        Multiplayer.Session.Network.SendPacket(new MilestoneUnlockPacket(id, unlockTick, patternId, parameters));
    }
}

﻿using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(MilestoneSystem))]
    internal class MilestoneSystem_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MilestoneSystem.SetForNewGame))]
        public static void SetForNewGame_Postfix()
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.LocalPlayer.IsHost)
            {
                // Request milestone data
                Log.Info($"Requesting MilestoneData from the server");
                Multiplayer.Session.Network.SendPacket(new MilestoneDataRequest());
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MilestoneSystem.UnlockMilestone))]
        public static void UnlockMilestone_Postfix(MilestoneSystem __instance, int id, long unlockTick)
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.Statistics.IsIncomingRequest)
            {
                return;
            }
            
            if (__instance.milestoneDatas.TryGetValue(id, out MilestoneData milestoneData))
            {
                int patternId = milestoneData.journalData.patternId;
                long[] parameters = milestoneData.journalData.parameters;
                Multiplayer.Session.Network.SendPacket(new MilestoneUnlockPacket(id, unlockTick, patternId, parameters));
            }
        }
    }
}

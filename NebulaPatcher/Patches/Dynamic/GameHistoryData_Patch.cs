using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;
using NebulaWorld.GameDataHistory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameHistoryData))]
    class GameHistoryData_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetForNewGame")]
        public static void Postfix()
        {
            // Do not run if it is not multiplayer and the player is not a client
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }
            // Request history data
            Log.Info($"Requesting GameHistoryData from the server");
            LocalPlayer.SendPacket(new GameHistoryDataRequest());
        }

        [HarmonyPostfix]
        [HarmonyPatch("EnqueueTech")]
        public static void Postfix2(int techId)
        {
            //Do not run if this was triggered by incomming request
            if (GameDataHistoryManager.IsIncomingRequest)
            {
                return;
            }
            //Synchronize enqueueing techs by players
            Log.Info($"Sending Enqueque Tech notification");
            LocalPlayer.SendPacket(new GameHistoryEnqueueTechPacket(techId));
        }

        [HarmonyPostfix]
        [HarmonyPatch("RemoveTechInQueue")]
        public static void Postfix3(int index)
        {
            //Do not run if this was triggered by incomming request
            if (GameDataHistoryManager.IsIncomingRequest)
            {
                return;
            }
            //Synchronize dequeueing techs by players
            Log.Info($"Sending Enqueque Tech notification");
            LocalPlayer.SendPacket(new GameHistoryRemoveTechPacket(index));
        }

        [HarmonyPostfix]
        [HarmonyPatch("PauseTechQueue")]
        public static void Postfix4()
        {
            //Do not run if this was triggered by incomming request
            if (GameDataHistoryManager.IsIncomingRequest)
            {
                return;
            }
            //Synchronize pausing techs by players
            Log.Info($"Sending Pause Tech queue notification");
            LocalPlayer.SendPacket(new GameHistoryNotificationPacket(GameHistoryEvent.PauseQueue));
        }

        [HarmonyPostfix]
        [HarmonyPatch("ResumeTechQueue")]
        public static void Postfix5()
        {
            //Do not run if this was triggered by incomming request
            if (GameDataHistoryManager.IsIncomingRequest)
            {
                return;
            }
            //Synchronize resuming techs by players
            Log.Info($"Sending Resume Tech queue notification");
            LocalPlayer.SendPacket(new GameHistoryNotificationPacket(GameHistoryEvent.ResumeQueue));
        }

        [HarmonyPrefix]
        [HarmonyPatch("UnlockRecipe")]
        public static bool Prefix1()
        {
            //Wait for the authoritative packet for unlocking recipes in multiplayer for clients
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || GameDataHistoryManager.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch("UnlockTechFunction")]
        public static bool Prefix2()
        {
            //Wait for the authoritative packet for unlocking tech features in multiplayer for clients
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || GameDataHistoryManager.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GainTechAwards")]
        public static bool Prefix3()
        {
            //Wait for the authoritative packet for gaining tech awards in multiplayer for clients
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || GameDataHistoryManager.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddTechHash")]
        public static bool Prefix4(GameHistoryData __instance, long addcnt)
        {
            //Host in multiplayer can do normal research in the mecha
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }

            //Clients just sends contributing packet to the server
            LocalPlayer.SendPacket(new GameHistoryResearchContributionPacket(addcnt, __instance.currentTech));
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("DequeueTech")]
        public static bool Prefix5()
        {
            ///Wait for the authoritative packet for dequeing tech in multiplayer for clients
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || GameDataHistoryManager.IsIncomingRequest;
        }
    }
}
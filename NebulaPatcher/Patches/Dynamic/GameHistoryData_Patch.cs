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
        [HarmonyPatch(nameof(GameHistoryData.SetForNewGame))]
        public static void SetForNewGame_Postfix()
        {
            // Do not run if it is not multiplayer and the player is not a client
            if (!SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient)
            {
                return;
            }
            // Request history data
            Log.Info($"Requesting GameHistoryData from the server");
            LocalPlayer.Instance.SendPacket(new GameHistoryDataRequest());
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameHistoryData.EnqueueTech))]
        public static void EnqueueTech_Postfix(int techId)
        {
            if (!SimulatedWorld.Instance.Initialized)
            {
                return;
            }
            //Do not run if this was triggered by incomming request
            if (GameDataHistoryManager.IsIncomingRequest)
            {
                return;
            }
            //Synchronize enqueueing techs by players
            Log.Info($"Sending Enqueue Tech notification");
            LocalPlayer.Instance.SendPacket(new GameHistoryEnqueueTechPacket(techId));
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameHistoryData.RemoveTechInQueue))]
        public static void RemoveTechInQueue_Postfix(int index, int __state)
        {
            if (!SimulatedWorld.Instance.Initialized)
            {
                return;
            }
            //Do not run if this was triggered by incomming request
            if (GameDataHistoryManager.IsIncomingRequest)
            {
                return;
            }
            //Synchronize dequeueing techs by players and trigger refunds for all clients
            Log.Info($"Sending Dequeue Tech notification: remove techID{__state}");
            LocalPlayer.Instance.SendPacket(new GameHistoryRemoveTechPacket(__state));
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameHistoryData.PauseTechQueue))]
        public static void PauseTechQueue_Postfix()
        {
            if (!SimulatedWorld.Instance.Initialized)
            {
                return;
            }
            //Do not run if this was triggered by incomming request
            if (GameDataHistoryManager.IsIncomingRequest)
            {
                return;
            }
            //Synchronize pausing techs by players
            Log.Info($"Sending Pause Tech queue notification");
            LocalPlayer.Instance.SendPacket(new GameHistoryNotificationPacket(GameHistoryEvent.PauseQueue));
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameHistoryData.ResumeTechQueue))]
        public static void ResumeTechQueue_Postfix()
        {
            if (!SimulatedWorld.Instance.Initialized)
            {
                return;
            }
            //Do not run if this was triggered by incomming request
            if (GameDataHistoryManager.IsIncomingRequest)
            {
                return;
            }
            //Synchronize resuming techs by players
            Log.Info($"Sending Resume Tech queue notification");
            LocalPlayer.Instance.SendPacket(new GameHistoryNotificationPacket(GameHistoryEvent.ResumeQueue));
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameHistoryData.UnlockRecipe))]
        public static bool UnlockRecipe_Prefix()
        {
            //Wait for the authoritative packet for unlocking recipes in multiplayer for clients
            return !SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient || GameDataHistoryManager.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameHistoryData.UnlockTechFunction))]
        public static bool UnlockTechFunction_Prefix()
        {
            //Wait for the authoritative packet for unlocking tech features in multiplayer for clients
            return !SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient || GameDataHistoryManager.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameHistoryData.GainTechAwards))]
        public static bool GainTechAwards_Prefix()
        {
            //Wait for the authoritative packet for gaining tech awards in multiplayer for clients
            return !SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient || GameDataHistoryManager.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameHistoryData.AddTechHash))]
        public static bool AddTechHash_Prefix(GameHistoryData __instance, long addcnt)
        {
            //Host in multiplayer can do normal research in the mecha
            if (!SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient)
            {
                return true;
            }

            //Clients just sends contributing packet to the server
            LocalPlayer.Instance.SendPacket(new GameHistoryResearchContributionPacket(addcnt, __instance.currentTech));
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameHistoryData.DequeueTech))]
        public static bool DequeueTech_Prefix()
        {
            ///Wait for the authoritative packet for dequeing tech in multiplayer for clients
            return !SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient || GameDataHistoryManager.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameHistoryData.UnlockTech))]
        public static bool UnlockTech_Prefix()
        {
            //Wait for the authoritative packet for unlocking tech features in multiplayer for clients
            return !SimulatedWorld.Instance.Initialized || LocalPlayer.Instance.IsMasterClient || GameDataHistoryManager.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameHistoryData.RemoveTechInQueue))]
        public static void RemoveTechInQueue_Prefix(int index, out int __state)
        {
            __state = GameMain.history.techQueue[index];
            if (SimulatedWorld.Instance.Initialized && LocalPlayer.Instance.IsMasterClient)
            {
                //we need to know which itemtypes are currently needed for refunds, so trigger refund before cancelling own research
                NebulaNetwork.MultiplayerHostSession.Instance.PlayerManager.SendTechRefundPackagesToClients(__state);
            }
            Log.Info($"RemoveTechInQueue: remove tech at index {index} with techId { __state}");
        }
    }
}
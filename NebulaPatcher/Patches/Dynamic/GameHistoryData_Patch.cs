#region

using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GameHistoryData))]
internal class GameHistoryData_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameHistoryData.EnqueueTech))]
    public static void EnqueueTech_Postfix(int techId)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        //Do not run if this was triggered by incomming request
        if (Multiplayer.Session.History.IsIncomingRequest)
        {
            return;
        }
        //Synchronize enqueueing techs by players
        Log.Info("Sending Enqueue Tech notification");
        Multiplayer.Session.Network.SendPacket(new GameHistoryEnqueueTechPacket(techId));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameHistoryData.RemoveTechInQueue))]
    public static void RemoveTechInQueue_Prefix(int index, out int __state)
    {
        __state = GameMain.history.techQueue[index];
        Log.Info($"RemoveTechInQueue: remove tech at index {index} with techId {__state}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameHistoryData.RemoveTechInQueue))]
    public static void RemoveTechInQueue_Postfix(int __state)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        //Do not run if this was triggered by incomming request
        if (Multiplayer.Session.History.IsIncomingRequest)
        {
            return;
        }
        //Synchronize dequeueing techs by players and trigger refunds for all clients
        Log.Info($"Sending Dequeue Tech notification: remove techID{__state}");
        Multiplayer.Session.Network.SendPacket(new GameHistoryRemoveTechPacket(__state));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameHistoryData.PauseTechQueue))]
    public static void PauseTechQueue_Postfix()
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        //Do not run if this was triggered by incomming request
        if (Multiplayer.Session.History.IsIncomingRequest)
        {
            return;
        }
        //Synchronize pausing techs by players
        Log.Info("Sending Pause Tech queue notification");
        Multiplayer.Session.Network.SendPacket(new GameHistoryNotificationPacket(GameHistoryEvent.PauseQueue));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameHistoryData.ResumeTechQueue))]
    public static void ResumeTechQueue_Postfix()
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        //Do not run if this was triggered by incomming request
        if (Multiplayer.Session.History.IsIncomingRequest)
        {
            return;
        }
        //Synchronize resuming techs by players
        Log.Info("Sending Resume Tech queue notification");
        Multiplayer.Session.Network.SendPacket(new GameHistoryNotificationPacket(GameHistoryEvent.ResumeQueue));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameHistoryData.UnlockRecipe))]
    public static bool UnlockRecipe_Prefix()
    {
        //Wait for the authoritative packet for unlocking recipes in multiplayer for clients
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.History.IsIncomingRequest;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameHistoryData.UnlockTechFunction))]
    public static bool UnlockTechFunction_Prefix()
    {
        //Wait for the authoritative packet for unlocking tech features in multiplayer for clients
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.History.IsIncomingRequest;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameHistoryData.GainTechAwards))]
    public static bool GainTechAwards_Prefix()
    {
        //Wait for the authoritative packet for gaining tech awards in multiplayer for clients
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.History.IsIncomingRequest;
    }

    [HarmonyPatch(nameof(GameHistoryData.DequeueTech))]
    public static bool DequeueTech_Prefix()
    {
        // Wait for the authoritative packet for dequeuing tech in multiplayer for clients
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.History.IsIncomingRequest;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameHistoryData.UnlockTech))]
    public static bool UnlockTech_Prefix()
    {
        //Wait for the authoritative packet for unlocking tech features in multiplayer for clients
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.History.IsIncomingRequest;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameHistoryData.UnlockTechUnlimited))]
    public static bool UnlockTechUnlimited_Prefix(GameHistoryData __instance, int techId)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.History.IsIncomingRequest)
        {
            return true;
        }
        // If client initial the manual unlock event (metadata, sandbox), send to server and wait for the authoritative
        var techState = __instance.TechState(techId);
        var level = techState.hashUploaded >= techState.hashNeeded ? techState.maxLevel : techState.curLevel;
        Multiplayer.Session.Network.SendPacket(new GameHistoryUnlockTechPacket(techId, level));
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameHistoryData.NotifyTechUnlock))]
    public static void NotifyTechUnlock_Postfix(int _techId, int _level)
    {
        Log.Info($"NotifyTechUnlock techId={_techId} level={_level}");
        if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        // Synchronize unlocking techs
        Log.Info("Sending Tech Unlocked notification");
        GameMain.mainPlayer.mecha.lab.itemPoints.Clear();
        Multiplayer.Session.Network.SendPacket(new GameHistoryUnlockTechPacket(_techId, _level));
    }
}

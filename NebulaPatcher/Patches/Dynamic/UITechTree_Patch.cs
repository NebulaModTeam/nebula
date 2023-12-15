#region

using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UITechTree))]
public class UITechTree_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UITechTree.Do1KeyUnlock))]
    public static bool Do1KeyUnlock_Prefix()
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsClient)
        {
            return true;
        }
        // Let host run one key unlock function
        Multiplayer.Session.Network.SendPacket(new GameHistoryNotificationPacket(GameHistoryEvent.OneKeyUnlock));
        Log.Info("Sent OneKeyUnlock request to host");
        return false;
    }
}

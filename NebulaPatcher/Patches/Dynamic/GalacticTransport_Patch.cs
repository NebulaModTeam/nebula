#region

using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GalacticTransport))]
internal class GalacticTransport_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GalacticTransport.SetForNewGame))]
    public static void SetForNewGame_Postfix()
    {
        if (Multiplayer.IsActive && !Multiplayer.Session.LocalPlayer.IsHost)
        {
            Multiplayer.Session.Network.SendPacket(new ILSRequestgStationPoolSync());
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.AddStationComponent))]
    public static bool AddStationComponent_Prefix(StationComponent station)
    {
        if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient && station.gid == 0)
        {
            // When client build a new station (gid == 0), we will let host decide the value of gid
            // ILS will be added when ILSAddStationComponent from host arrived
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.RemoveStationComponent))]
    public static bool RemoveStationComponent_Prefix()
    {
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Ships.PatchLockILS;
    }
}

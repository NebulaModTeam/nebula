#region

using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
#pragma warning disable IDE1006

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(StationComponent))]
internal class StationComponent_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(StationComponent.InternalTickRemote))]
    public static bool InternalTickRemote_Prefix(StationComponent __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return true;
        }
        // skip vanilla code entirely and use our modified version instead (which focuses on ship movement)
        // call our InternalTickRemote() for every StationComponent in game. Normally this would be done by each PlanetFactory, but as a client
        // we dont have every PlanetFactory at hand.
        // so iterate over the GameMain.data.galacticTransport.stationPool array which should also include the fake entries for factories we have not loaded yet.
        // but do this at another place to not trigger it more often than needed (GameData::GameTick())
        if (__instance.warperCount >= __instance.warperMaxCount)
        {
            return false;
        }
        // refill warpers from ILS storage
        for (var i = 0; i < __instance.storage.Length; i++)
        {
            if (__instance.storage[i].itemId != 1210 || __instance.storage[i].count <= 0)
            {
                continue;
            }
            __instance.warperCount++;
            lock (__instance.storage)
            {
                var num = __instance.storage[i].inc / __instance.storage[i].count;
                var array = __instance.storage;
                array[i].count -= 1;
                var array2 = __instance.storage;
                array2[i].inc -= num;
            }
            break;
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(StationComponent.RematchRemotePairs))]
    public static bool RematchRemotePairs_Prefix()
    {
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
        // skip vanilla code entirely for clients as we do this event based triggered by the server
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(StationComponent.IdleShipGetToWork))]
    public static void IdleShipGetToWork_Postfix(StationComponent __instance)
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        var packet = new ILSIdleShipBackToWork(in __instance.workShipDatas[__instance.workShipCount - 1], __instance.gid,
            __instance.workShipDatas.Length, __instance.warperCount);
        Multiplayer.Session.Network.SendPacket(packet);
    }

    // as we unload PlanetFactory objects when leaving the star system we need to prevent the call on ILS entries in the gStationPool array
    [HarmonyPrefix]
    [HarmonyPatch(nameof(StationComponent.Free))]
    public static bool Free_Prefix(StationComponent __instance)
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        return !__instance.isStellar;
    }
}

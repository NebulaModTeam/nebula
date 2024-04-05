#region

using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(PlanetTransport))]
internal class PlanetTransport_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetTransport.SetStationStorage))]
    public static bool SetStationStorage_Prefix(PlanetTransport __instance, int stationId, int storageIdx, int itemId,
        int itemCountMax, ELogisticStorage localLogic, ELogisticStorage remoteLogic)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        var stationComponent = __instance.stationPool[stationId];

        if (stationComponent == null)
        {
            return Multiplayer.Session.LocalPlayer.IsHost;
        }
        var packet = new StorageUI(__instance.planet.id, stationComponent.id, stationComponent.gid, storageIdx, itemId,
            itemCountMax, localLogic, remoteLogic);
        Multiplayer.Session.Network.SendPacket(packet);

        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetTransport.NewStationComponent))]
    public static void NewStationComponent_Postfix(PlanetTransport __instance, StationComponent __result, PrefabDesc _desc)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        if (!__result.isStellar && __result.planetId == 0)
        {
            // for the PLS slot to sync properly the StationComponent of the PLS needs to have planetId set to the correct value.
            // as the game does not do that for some reason, we need to do it here
            __result.planetId = __instance.planet.id;
        }

        if (__result.gid <= 0 || !Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        // After host has added the StationComponent it has planetId, id and gId, now we can inform all clients about this station
        // so they can add it to their GalacticTransport as they don't do that. Note that we're doing this in
        // PlanetTransport.NewStationComponent and not GalacticTransport.AddStationComponent because stationId will be set at this point.
        Log.Info(
            $"Send AddStationComponent to all clients for planet {__result.planetId}, id {__result.id} with gId of {__result.gid}");
        Multiplayer.Session.Network.SendPacket(new ILSAddStationComponent(__result.planetId, __result.id, __result.gid,
            __result.entityId, _desc.stationMaxShipCount));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetTransport.Import))]
    public static void Import_Postfix(PlanetTransport __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        foreach (var stationComponent in __instance.stationPool)
        {
            if (stationComponent is { planetId: 0, isStellar: false })
            {
                stationComponent.planetId = __instance.planet.id;
            }
        }
    }

    /*
     * As clients need to access the StationComponent in gStationPool when RematchRemotePairs() is called (and this also gets called by RemoveStationComponent())
     * we need to prevent the call for client here to avoid a NRE and instead call it triggered by host after RematchRemotePairs() got called.
     * basically in a Postfix of RemoveStationComponent()
     */
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetTransport.RemoveStationComponent))]
    public static bool RemoveStationComponent_Prefix(PlanetTransport __instance, int id, ref int __state)
    {
        __state = __instance.stationPool[id].gid; // cache this as we need it in the postfix but its gone there already.
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Ships.PatchLockILS;
    }

    /*
     * Host has called RematchRemotePairs() now and thus has send the ILSShipDataUpdate packet, so we can savely tell clients to remove the station component now.
     */
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlanetTransport.RemoveStationComponent))]
    public static void RemoveStationComponent_Postfix(PlanetTransport __instance, int id, int __state)
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(new ILSRemoveStationComponent(id, __instance.planet.id, __state));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetTransport.SetDispenserFilter))]
    public static void SetDispenserFilter_Prefix(PlanetTransport __instance, int dispenserId, int filter)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new DispenserSettingPacket(__instance.planet.id, dispenserId,
                EDispenserSettingEvent.SetFilter, filter));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetTransport.SetDispenserPlayerDeliveryMode))]
    public static void SetDispenserPlayerDeliveryMode_Prefix(PlanetTransport __instance, int dispenserId,
        EPlayerDeliveryMode playerDeliveryMode)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new DispenserSettingPacket(__instance.planet.id, dispenserId,
                EDispenserSettingEvent.SetPlayerDeliveryMode, (int)playerDeliveryMode));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetTransport.SetDispenserStorageDeliveryMode))]
    public static void SetDispenserStorageDeliveryMode_Prefix(PlanetTransport __instance, int dispenserId,
        EStorageDeliveryMode storageDeliveryMode)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new DispenserSettingPacket(__instance.planet.id, dispenserId,
                EDispenserSettingEvent.SetStorageDeliveryMode, (int)storageDeliveryMode));
        }
    }
}

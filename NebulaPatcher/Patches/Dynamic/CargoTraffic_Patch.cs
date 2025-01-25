#region

using HarmonyLib;
using NebulaAPI;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(CargoTraffic))]
internal class CargoTraffic_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CargoTraffic.PickupBeltItems))]
    public static void PickupBeltItems_Prefix()
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Belts.BeltPickupStarted();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CargoTraffic.PickupBeltItems))]
    public static void PickupBeltItems_Postfix()
    {
        if (Multiplayer.IsActive && GameMain.data.localPlanet != null)
        {
            Multiplayer.Session.Belts.BeltPickupEnded();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CargoTraffic.PutItemOnBelt))]
    public static void PutItemOnBelt_Postfix(int beltId, int itemId, byte itemInc, bool __result)
    {
        // Only send packet when insertion successes
        if (Multiplayer.IsActive && __result && !Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new BeltUpdatePutItemOnPacket(beltId, itemId, 1, itemInc,
                GameMain.data.localPlanet.id));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CargoTraffic.AlterBeltRenderer))]
    public static bool AlterBeltRenderer_Prefix()
    {
        //Do not call renderer, if user is not on the planet as the request
        return !Multiplayer.IsActive || Multiplayer.Session.Factories.TargetPlanet == NebulaModAPI.PLANET_NONE ||
               GameMain.mainPlayer.planetId == Multiplayer.Session.Factories.TargetPlanet;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CargoTraffic.RemoveBeltRenderer))]
    public static bool RemoveBeltRenderer_Prefix()
    {
        //Do not call renderer, if user is not on the planet as the request
        return !Multiplayer.IsActive || Multiplayer.Session.Factories.TargetPlanet == NebulaModAPI.PLANET_NONE ||
               GameMain.mainPlayer.planetId == Multiplayer.Session.Factories.TargetPlanet;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CargoTraffic.AlterPathRenderer))]
    public static bool AlterPathRenderer_Prefix()
    {
        //Do not call renderer, if user is not on the planet as the request
        return !Multiplayer.IsActive || Multiplayer.Session.Factories.TargetPlanet == NebulaModAPI.PLANET_NONE ||
               GameMain.mainPlayer.planetId == Multiplayer.Session.Factories.TargetPlanet;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CargoTraffic.RemovePathRenderer))]
    public static bool RemovePathRenderer_Prefix()
    {
        //Do not call renderer, if user is not on the planet as the request
        return !Multiplayer.IsActive || Multiplayer.Session.Factories.TargetPlanet == NebulaModAPI.PLANET_NONE ||
               GameMain.mainPlayer.planetId == Multiplayer.Session.Factories.TargetPlanet;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CargoTraffic.RefreshPathUV))]
    public static bool RefreshPathUV_Prefix()
    {
        //Do not call renderer, if user is not on the planet as the request
        return !Multiplayer.IsActive || Multiplayer.Session.Factories.TargetPlanet == NebulaModAPI.PLANET_NONE ||
               GameMain.mainPlayer.planetId == Multiplayer.Session.Factories.TargetPlanet;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CargoTraffic.ConnectToMonitor))]
    public static void ConnectToMonitor_Postfix(int monitorId, int targetBeltId, int offset)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        // If host build, or client receive his build request
        if (Multiplayer.Session.LocalPlayer.IsHost && !Multiplayer.Session.Factories.IsIncomingRequest.Value ||
            Multiplayer.Session.Factories.PacketAuthor == Multiplayer.Session.LocalPlayer.Id)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new ConnectToMonitorPacket(monitorId, targetBeltId, offset,
                GameMain.data.localPlanet.id));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CargoTraffic.ConnectToSpraycoater))]
    public static void ConnectToSpraycoater(int spraycoaterId, int cargoBeltId, int incBeltId)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        // If host build, or client receive his build request
        if (Multiplayer.Session.LocalPlayer.IsHost && !Multiplayer.Session.Factories.IsIncomingRequest.Value ||
            Multiplayer.Session.Factories.PacketAuthor == Multiplayer.Session.LocalPlayer.Id)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new ConnectToSpraycoaterPacket(spraycoaterId, cargoBeltId,
                incBeltId, GameMain.data.localPlanet.id));
        }
    }
}

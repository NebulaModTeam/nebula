using HarmonyLib;
using LiteNetLib;
using NebulaModel.Packets.Planet;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameData), "ArrivePlanet")]
    class ArrivePlanet_Patch
    {
        public static void Postfix(GameData __instance)
        {
            var packet = new localPlanetSyncPckt(__instance.localPlanet.id, false);
            packet.playerId = LocalPlayer.PlayerId;
            LocalPlayer.SendPacket(packet, DeliveryMethod.ReliableUnordered);
        }
    }

    [HarmonyPatch(typeof(GameData), "LeavePlanet")]
    class LeavePlanet_Patch
    {
        public static void Postfix(GameData __instance)
        {
            if (LocalPlayer.FinishedGameLoad)
            {
                var packet = new localPlanetSyncPckt(0, false);
                packet.playerId = LocalPlayer.PlayerId;
                LocalPlayer.SendPacket(packet, DeliveryMethod.ReliableUnordered);
            }
        }
    }
}


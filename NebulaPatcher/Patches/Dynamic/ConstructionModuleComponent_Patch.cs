#region

using HarmonyLib;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.Player;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(ConstructionModuleComponent))]
internal class ConstructionModuleComponent_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ConstructionModuleComponent.EjectMechaDrone))]
    public static void EjectMechaDrone_Prefix(PlanetFactory factory, Player player, int targetObjectId,
        int next1ObjectId, int next2ObjectId, int next3ObjectId)
    {
        if (!Multiplayer.IsActive) return;

        // Notify other players for eject of mecha drone
        var playerId = Multiplayer.Session.LocalPlayer.Id;
        var planetId = factory.planetId;
        var priority = player.mecha.constructionModule.dronePriority;
        var packet = new PlayerEjectMechaDronePacket(playerId, planetId, targetObjectId, next1ObjectId, next2ObjectId, next3ObjectId, priority);
        Multiplayer.Session.Network.SendPacketToLocalStar(packet);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ConstructionModuleComponent.InsertTmpBuildTarget))]
    public static bool InsertTmpBuildTarget_Prefix(ConstructionModuleComponent __instance, int objectId, float value)
    {
        if (__instance.entityId != 0 || value < DroneManager.MinSqrDistance) return true; // BAB, or distance is very close
        if (!Multiplayer.IsActive) return true;

        // Only send out mecha drones if local player is the closest player to the target prebuild
        if (GameMain.localPlanet == null) return true;
        var sqrDistToOtherPlayer = Multiplayer.Session.Drones.GetClosestRemotePlayerSqrDistance(GameMain.localPlanet.factory.prebuildPool[objectId].pos);
        return value <= sqrDistToOtherPlayer;
    }
}

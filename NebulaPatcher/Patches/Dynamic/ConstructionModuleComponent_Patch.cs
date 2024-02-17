#region

using HarmonyLib;
using NebulaModel.Packets.Players;
using NebulaWorld;

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
}

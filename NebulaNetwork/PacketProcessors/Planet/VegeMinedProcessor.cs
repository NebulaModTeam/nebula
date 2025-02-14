#region

using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Planet;

// Processes events for mining vegetation or veins
[RegisterPacketProcessor]
internal class VegeMinedProcessor : PacketProcessor<VegeMinedPacket>
{
    protected override void ProcessPacket(VegeMinedPacket packet, NebulaConnection conn)
    {
        var planetData = GameMain.galaxy.PlanetById(packet.PlanetId);
        var factory = planetData?.factory;
        if (factory is not { vegePool: not null })
        {
            return;
        }
        using (Multiplayer.Session.Planets.IsIncomingRequest.On())
        {
            Multiplayer.Session.Planets.TargetPlanet = packet.PlanetId;
            if (packet.Amount == 0)
            {
                if (packet.IsVein)
                {
                    var veinData = factory.GetVeinData(packet.VegeId);
                    var veinProto = LDB.veins.Select((int)veinData.type);

                    factory.RemoveVeinWithComponents(packet.VegeId);

                    if (veinProto == null || GameMain.localPlanet != planetData)
                    {
                        return;
                    }
                    VFEffectEmitter.Emit(veinProto.MiningEffect, veinData.pos,
                        Maths.SphericalRotation(veinData.pos, 0f));
                    VFAudio.Create(veinProto.MiningAudio, null, veinData.pos, true);
                }
                else
                {
                    var vegeData = factory.GetVegeData(packet.VegeId);
                    var vegeProto = LDB.veges.Select(vegeData.protoId);

                    factory.RemoveVegeWithComponents(packet.VegeId);

                    if (vegeProto == null || GameMain.localPlanet != planetData)
                    {
                        return;
                    }
                    VFEffectEmitter.Emit(vegeProto.MiningEffect, vegeData.pos,
                        Maths.SphericalRotation(vegeData.pos, 0f));
                    VFAudio.Create(vegeProto.MiningAudio, null, vegeData.pos, true);
                }
            }
            else
            {
                // Taken from if (!isInfiniteResource) part of PlayerAction_Mine.GameTick()
                var veinData = factory.GetVeinData(packet.VegeId);
                var veinGroups = factory.veinGroups;
                var groupIndex = veinData.groupIndex;

                // must be a vein/oil patch (i think the game treats them same now as oil patches can run out too)
                factory.veinPool[packet.VegeId].amount = packet.Amount;
                veinGroups[groupIndex].amount -= 1L;
            }
            Multiplayer.Session.Planets.TargetPlanet = NebulaModAPI.PLANET_NONE;
        }
    }
}

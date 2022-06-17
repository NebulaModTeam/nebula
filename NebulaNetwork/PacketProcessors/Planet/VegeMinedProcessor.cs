using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Planet
{
    // Processes events for mining vegetation or veins
    [RegisterPacketProcessor]
    internal class VegeMinedProcessor : PacketProcessor<VegeMinedPacket>
    {
        public override void ProcessPacket(VegeMinedPacket packet, NebulaConnection conn)
        {
            if (GameMain.galaxy.PlanetById(packet.PlanetId)?.factory != null && GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.vegePool != null)
            {
                using (Multiplayer.Session.Planets.IsIncomingRequest.On())
                {
                    PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
                    if (packet.Amount == 0 && factory != null)
                    {
                        if (packet.IsVein)
                        {
                            VeinData veinData = factory.GetVeinData(packet.VegeId);
                            VeinProto veinProto = LDB.veins.Select((int)veinData.type);

                            factory.RemoveVeinWithComponents(packet.VegeId);

                            if (veinProto != null)
                            {
                                VFEffectEmitter.Emit(veinProto.MiningEffect, veinData.pos, Maths.SphericalRotation(veinData.pos, 0f));
                                VFAudio.Create(veinProto.MiningAudio, null, veinData.pos, true, 0, -1, -1L);
                            }
                        }
                        else
                        {
                            VegeData vegeData = factory.GetVegeData(packet.VegeId);
                            VegeProto vegeProto = LDB.veges.Select(vegeData.protoId);

                            factory.RemoveVegeWithComponents(packet.VegeId);

                            if (vegeProto != null)
                            {
                                VFEffectEmitter.Emit(vegeProto.MiningEffect, vegeData.pos, Maths.SphericalRotation(vegeData.pos, 0f));
                                VFAudio.Create(vegeProto.MiningAudio, null, vegeData.pos, true, 0, -1, -1L);
                            }
                        }
                    }
                    else if (factory != null)
                    {
                        // Taken from if (!isInfiniteResource) part of PlayerAction_Mine.GameTick()
                        VeinData veinData = factory.GetVeinData(packet.VegeId);
                        VeinGroup[] veinGroups = factory.veinGroups;
                        short groupIndex = veinData.groupIndex;
                        NebulaModel.Logger.Log.Warn(groupIndex);

                        // must be a vein/oil patch (i think the game treats them same now as oil patches can run out too)
                        factory.veinPool[packet.VegeId].amount = packet.Amount;
                        veinGroups[groupIndex].amount = veinGroups[groupIndex].amount - 1L;
                    }
                    else
                    {
                        Log.Warn("Received VegeMinedPacket but could not do as i was told :C");
                    }
                }
            }
        }
    }
}

using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class VegetationMinedProcessor : IPacketProcessor<VegeMined>
    {
        public void ProcessPacket(VegeMined packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy?.PlanetById(packet.PlanetID);
            if (planet == null)
            {
                return;
            }

            if (packet.isVegetable) // Trees, rocks, leaves, etc
            {
                VegeData vData = (VegeData)planet.factory?.GetVegeData(packet.MiningID);
                VegeProto vProto = LDB.veges.Select((int)vData.protoId);
                if (vProto != null && planet.id == GameMain.localPlanet?.id)
                {
                    VFEffectEmitter.Emit(vProto.MiningEffect, vData.pos, vData.rot);
                    VFAudio.Create(vProto.MiningAudio, null, vData.pos, true);
                }
                planet.factory?.RemoveVegeWithComponents(vData.id);
            }
            else // veins
            {
                VeinData vData = (VeinData)planet.factory?.GetVeinData(packet.MiningID);
                VeinProto vProto = LDB.veins.Select((int)vData.type);
                if (vProto != null)
                {
                    if (planet.factory?.veinPool[packet.MiningID].amount > 0)
                    {
                        VeinData[] vPool = planet.factory?.veinPool;
                        PlanetData.VeinGroup[] vGroups = planet.factory?.planet.veinGroups;
                        long[] vAmounts = planet.veinAmounts;
                        vPool[packet.MiningID].amount -= 1;
                        vGroups[(int)vData.groupIndex].amount -= 1;
                        vAmounts[(int)vData.type] -= 1;

                        if (planet.id == GameMain.localPlanet?.id)
                        {
                            VFEffectEmitter.Emit(vProto.MiningEffect, vData.pos, Maths.SphericalRotation(vData.pos, 0f));
                            VFAudio.Create(vProto.MiningAudio, null, vData.pos, true);
                        }
                    }
                    else
                    {
                        PlanetData.VeinGroup[] vGroups = planet.factory?.planet.veinGroups;
                        vGroups[vData.groupIndex].count -= 1;

                        if (planet.id == GameMain.localPlanet?.id)
                        {
                            VFEffectEmitter.Emit(vProto.MiningEffect, vData.pos, Maths.SphericalRotation(vData.pos, 0f));
                            VFAudio.Create(vProto.MiningAudio, null, vData.pos, true);
                        }

                        planet.factory?.RemoveVeinWithComponents(vData.id);
                    }
                }
            }
        }
    }
}

using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using UnityEngine;

namespace NebulaClient.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class FactoryDataProcessor : IPacketProcessor<FactoryData>
    {
        public void ProcessPacket(FactoryData packet, NebulaConnection conn)
        {
            LocalPlayer.PendingFactories.Add(packet.PlanetId, packet.BinaryData);

            lock (PlanetModelingManager.fctPlanetReqList)
            {
                PlanetModelingManager.fctPlanetReqList.Enqueue(GameMain.galaxy.PlanetById(packet.PlanetId));
            }
        }
    }
}

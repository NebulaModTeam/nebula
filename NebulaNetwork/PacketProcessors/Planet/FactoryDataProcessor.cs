using NebulaAPI;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using LocalPlayer = NebulaWorld.LocalPlayer;

namespace NebulaNetwork.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class FactoryDataProcessor : PacketProcessor<FactoryData>
    {
        public override void ProcessPacket(FactoryData packet, NebulaConnection conn)
        {
            if (IsHost) return;

            LocalPlayer.Instance.PendingFactories.Add(packet.PlanetId, packet.BinaryData);

            lock (PlanetModelingManager.fctPlanetReqList)
            {
                PlanetModelingManager.fctPlanetReqList.Enqueue(GameMain.galaxy.PlanetById(packet.PlanetId));
            }
        }
    }
}

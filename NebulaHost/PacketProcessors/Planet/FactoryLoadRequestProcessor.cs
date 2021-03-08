using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using System.IO;

namespace NebulaHost.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class FactoryLoadRequestProcessor : IPacketProcessor<FactoryLoadRequest>
    {
        public void ProcessPacket(FactoryLoadRequest packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetID);
            PlanetFactory factory = GameMain.data.GetOrCreateFactory(planet);

            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryWriter bw = new BinaryWriter(ms))
                {
                    factory.Export(bw);
                }

                conn.SendPacket(new FactoryData(packet.PlanetID, ms.ToArray()));
            }
        }
    }
}

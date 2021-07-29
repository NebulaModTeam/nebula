using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using System.Linq;

namespace NebulaNetwork.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class PlanetDataResponseProcessor : PacketProcessor<PlanetDataResponse>
    {
        public override void ProcessPacket(PlanetDataResponse packet, NebulaConnection conn)
        {
            if (IsHost) return;

            // We have to track the offset we are currently at in the flattened jagged array as we decode
            int currentOffset = 0;

            for (int i = 0; i < packet.PlanetDataIDs.Length; i++)
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetDataIDs[i]);

                Log.Info($"Parsing {packet.PlanetDataBytesLengths[i]} bytes of data for planet {planet.name} (ID: {planet.id})");
                byte[] planetData = packet.PlanetDataBytes.Skip(currentOffset).Take(packet.PlanetDataBytesLengths[i]).ToArray();

                using (BinaryUtils.Reader reader = new BinaryUtils.Reader(planetData))
                {
                    planet.ImportRuntime(reader.BinaryReader);
                }

                lock (PlanetModelingManager.genPlanetReqList)
                {
                    PlanetModelingManager.genPlanetReqList.Enqueue(planet);
                }

                currentOffset += packet.PlanetDataBytesLengths[i];
            }
        }
    }
}

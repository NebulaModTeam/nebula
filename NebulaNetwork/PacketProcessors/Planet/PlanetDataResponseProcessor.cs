using Mirror;
using NebulaModel.Logger;
using NebulaModel.Networking;
using System.Collections.Generic;
using System.Linq;

namespace NebulaNetwork.PacketProcessors.Planet
{
    public struct PlanetDataResponse : NetworkMessage
    {
        public int[] PlanetDataIDs;
        public byte[] PlanetDataBytes;
        public int[] PlanetDataBytesLengths;

        public PlanetDataResponse(Dictionary<int, byte[]> planetData)
        {
            PlanetDataIDs = planetData.Keys.ToArray();

            // Can't use a jagged array because LNL serializer says no, so flattening and separate offset array it is
            // TODO: Possibly register a type with the serializer instead of doing this manually
            PlanetDataBytes = planetData.Values.SelectMany(x => x).ToArray();
            PlanetDataBytesLengths = planetData.Values.Select(x => x.Length).ToArray();
            NebulaModel.Logger.Log.Info($"Creating {GetType()}");
        }

        public static void ProcessPacket(PlanetDataResponse packet)
        {
            NebulaModel.Logger.Log.Info($"Processing {packet.GetType()}");

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

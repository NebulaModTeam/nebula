using Mirror;
using NebulaModel.Networking;
using NebulaWorld.Statistics;
using System;
using System.Threading;

namespace NebulaNetwork.PacketProcessors.Planet
{
    public struct FactoryLoadRequest : NetworkMessage
    {
        public int PlanetID;

        public FactoryLoadRequest(int planetID)
        {
            PlanetID = planetID;
            NebulaModel.Logger.Log.Info($"Creating {GetType()}");
        }

        public static void ProcessPacket(NetworkConnection conn, FactoryLoadRequest packet)
        {
            NebulaModel.Logger.Log.Info($"Processing {packet.GetType()}");

            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetID);
            PlanetFactory factory = GameMain.data.GetOrCreateFactory(planet);

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                factory.Export(writer.BinaryWriter);
                conn.Send(new FactoryData(packet.PlanetID, writer.CloseAndGetBytes()));
            }
            conn.SendPacket(StatisticsManager.instance.GetFactoryPlanetIds());
        }
    }
}

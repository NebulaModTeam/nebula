using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld.Statistics;
using NebulaModel;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class FactoryLoadRequestProcessor : PacketProcessor<FactoryLoadRequest>
    {
        public override void ProcessPacket(FactoryLoadRequest packet, NebulaConnection conn)
        {
            if (IsClient) return;

            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetID);
            PlanetFactory factory = GameMain.data.GetOrCreateFactory(planet);

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                if (Config.Options.NetworkSavegameCompression)
                {
                    CustomExporters.CustomFactoryExport(writer.BinaryWriter, ref factory);
                } else
                {
                    factory.Export(writer.BinaryWriter);
                }
                conn.SendPacket(new FactoryData(packet.PlanetID, writer.CloseAndGetBytes()));
            }
            conn.SendPacket(StatisticsManager.instance.GetFactoryPlanetIds());
        }

        

    }
}

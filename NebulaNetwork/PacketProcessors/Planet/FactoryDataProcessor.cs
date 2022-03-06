using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class FactoryDataProcessor : PacketProcessor<FactoryData>
    {
        public override void ProcessPacket(FactoryData packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                return;
            }
            // Stop packet processing until factory is imported
            ((NebulaModel.NetworkProvider)Multiplayer.Session.Network).PacketProcessor.Enable = false;
            Log.Info($"FactoryDataProcessor: Pause PacketProcessor");

            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            Multiplayer.Session.Planets.PendingFactories.Add(packet.PlanetId, packet.BinaryData);
            Log.Info($"Parsing {packet.BinaryData.Length} bytes of data for factory {planet.name} (ID: {planet.id})");

            lock (PlanetModelingManager.fctPlanetReqList)
            {
                PlanetModelingManager.fctPlanetReqList.Enqueue(GameMain.galaxy.PlanetById(packet.PlanetId));
            }
        }
    }
}

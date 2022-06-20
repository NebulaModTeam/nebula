using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using NebulaWorld.GameStates;

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
            // The whole fragment is received
            GameStatesManager.FragmentSize = 0;

            // Stop packet processing until factory is imported and loaded
            ((NebulaModel.NetworkProvider)Multiplayer.Session.Network).PacketProcessor.Enable = false;
            Log.Info($"Pause PacketProcessor (FactoryDataProcessor)");

            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            Multiplayer.Session.Planets.PendingFactories.Add(packet.PlanetId, packet.BinaryData);
            Multiplayer.Session.Planets.PendingTerrainData.Add(packet.PlanetId, packet.TerrainModData);
            Log.Info($"Parsing {packet.BinaryData.Length} bytes of data for factory {planet.name} (ID: {planet.id})");

            lock (PlanetModelingManager.fctPlanetReqList)
            {
                PlanetModelingManager.fctPlanetReqList.Enqueue(planet);
            }
        }
    }
}

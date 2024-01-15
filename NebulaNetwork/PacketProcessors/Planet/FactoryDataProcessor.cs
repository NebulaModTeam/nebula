#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using NebulaWorld.GameStates;

#endregion

namespace NebulaNetwork.PacketProcessors.Planet;

[RegisterPacketProcessor]
public class FactoryDataProcessor : PacketProcessor<FactoryData>
{
    protected override void ProcessPacket(FactoryData packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }
        // The whole fragment is received
        GameStatesManager.FragmentSize = 0;

        // Stop packet processing until factory is imported and loaded
        Multiplayer.Session.Network.PacketProcessor.EnablePacketProcessing = false;
        Log.Info("Pause PacketProcessor (FactoryDataProcessor)");

        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
        Multiplayer.Session.Planets.PendingFactories.Add(packet.PlanetId, packet.BinaryData);
        Multiplayer.Session.Planets.PendingTerrainData.Add(packet.PlanetId, packet.TerrainModData);
        Log.Info($"Parsing {packet.BinaryData.Length} bytes of data for factory {planet.name} (ID: {planet.id})");

        lock (PlanetModelingManager.fctPlanetReqList)
        {
            PlanetModelingManager.fctPlanetReqList.Enqueue(planet);
        }
    }
}

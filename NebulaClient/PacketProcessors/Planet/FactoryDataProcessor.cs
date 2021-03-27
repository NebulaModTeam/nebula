using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaModel.Logger;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class FactoryDataProcessor : IPacketProcessor<FactoryData>
    {
        public void ProcessPacket(FactoryData packet, NebulaConnection conn)
        {
            bool factoryJustForImport = false;

            LocalPlayer.PendingFactories.Add(packet.PlanetId, packet.BinaryData);

            for(int i = 0; i < packet.PlanetIdsWithLogistics.Length && !LocalPlayer.IsMasterClient; i++)
            {
                // if we have no loading queued and have not already loaded the factory data from server.
                PlanetData pData = GameMain.galaxy.PlanetById(packet.PlanetIdsWithLogistics[i]);
                if (!LocalPlayer.PendingFactories.ContainsKey(pData.id) && pData.factory == null && !pData.factoryLoading && !LocalPlayer.requestedAllLogistics)
                {
                    Log.Info($"Requested factory for planet (ID: {packet.PlanetIdsWithLogistics[i]}) from host because it contains logistic towers");
                    LocalPlayer.SendPacket(new FactoryLoadRequest(packet.PlanetIdsWithLogistics[i]));
                }
                else if(packet.PlanetId == packet.PlanetIdsWithLogistics[i] && i != 0)
                {
                    // if its data for a factory we requested because of Logistics just import its data but do not really load it.
                    // planet id at index 0 should always be the planet we spawn on so load it.
                    GameMain.data.GetOrCreateFactory(pData);
                    factoryJustForImport = true;
                }
            }
            LocalPlayer.requestedAllLogistics = true;

            if (!factoryJustForImport)
            {
                lock (PlanetModelingManager.fctPlanetReqList)
                {
                    PlanetModelingManager.fctPlanetReqList.Enqueue(GameMain.galaxy.PlanetById(packet.PlanetId));
                }
            }
        }
    }
}

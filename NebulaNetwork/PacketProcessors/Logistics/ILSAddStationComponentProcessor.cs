#region

using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSAddStationComponentProcessor : PacketProcessor<ILSAddStationComponent>
{
    public override void ProcessPacket(ILSAddStationComponent packet, NebulaConnection conn)
    {
        Log.Info(
            $"ILSAddStationComponentProcessor processing packet for planet {packet.PlanetId}, station {packet.StationId} with gId of {packet.StationGId}");

        using (Multiplayer.Session.Ships.PatchLockILS.On())
        {
            var galacticTransport = GameMain.data.galacticTransport;
            var stationPool = GameMain.galaxy.PlanetById(packet.PlanetId).factory?.transport.stationPool;
            if (stationPool != null)
            {
                // If we have loaded the factory where the new station was created on, should be able to find
                // it in our PlanetTransport.stationPool
                // Assgin gid here so this station will go to galacticTransport.stationPool[gid]
                stationPool[packet.StationId].gid = packet.StationGId;
                galacticTransport.AddStationComponent(packet.PlanetId, stationPool[packet.StationId]);
            }
            else
            {
                // If we haven't loaded the factory the new station was create on, we need to create a 
                // "fake" station that we can put into the GalacticTransport.stationPool instead of a real on
                Multiplayer.Session.Ships.CreateFakeStationComponent(packet.StationGId, packet.PlanetId, packet.MaxShipCount);
            }
        }
    }
}

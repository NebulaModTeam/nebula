#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using NebulaWorld.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSAddStationComponentProcessor : PacketProcessor<ILSAddStationComponent>
{
    protected override void ProcessPacket(ILSAddStationComponent packet, NebulaConnection conn)
    {
        Log.Info(
            $"ILSAddStationComponentProcessor processing packet for planet {packet.PlanetId}, station {packet.StationId} with gId of {packet.StationGId}");

        using (Multiplayer.Session.Ships.PatchLockILS.On())
        {
            var galacticTransport = GameMain.data.galacticTransport;
            var stationPool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.transport.stationPool;
            if (stationPool != null)
            {
                // If we have loaded the factory where the new station was created on, should be able to find
                // it in our PlanetTransport.stationPool
                // Assign gid here so this station will go to galacticTransport.stationPool[gid]
                stationPool[packet.StationId].gid = packet.StationGId;
                if (galacticTransport.AddStationComponent(packet.PlanetId, stationPool[packet.StationId]) != packet.StationGId)
                {
                    Log.WarnInform($"AddStationComponent gid mismatch: {stationPool[packet.StationId].gid} => packet.StationGId");
                    galacticTransport.stationPool[packet.StationGId] = stationPool[packet.StationId];
                }
                galacticTransport.stationCursor = Math.Max(galacticTransport.stationCursor, packet.StationGId + 1);

                if (stationPool[packet.StationId].entityId != packet.EntityId)
                {
                    Log.WarnInform($"Station gid {packet.StationGId} entityId mismatch: {stationPool[packet.StationId].entityId} => {packet.EntityId}");
                }
            }
            else
            {
                // If we haven't loaded the factory the new station was create on, we need to create a 
                // "fake" station that we can put into the GalacticTransport.stationPool instead of a real on
                ILSShipManager.CreateFakeStationComponent(packet.StationGId, packet.PlanetId, packet.MaxShipCount);
            }
        }
    }
}

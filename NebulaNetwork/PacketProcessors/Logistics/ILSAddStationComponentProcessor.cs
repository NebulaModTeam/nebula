using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSAddStationComponentProcessor : PacketProcessor<ILSAddStationComponent>
    {
        public override void ProcessPacket(ILSAddStationComponent packet, NebulaConnection conn)
        {
            Log.Info($"ILSAddStationComponentProcessor processing packet for planet {packet.PlanetId}, station {packet.StationId} with gId of {packet.StationGId}");

            using (ILSShipManager.PatchLockILS.On())
            {
                GalacticTransport galacticTransport = GameMain.data.galacticTransport;

                if (packet.PlanetId == GameMain.localPlanet?.id)
                {
                    // If we're on the same planet as the new station was created on, should be able to find
                    // it in our local PlanetTransport.stationPool
                    StationComponent stationComponent = GameMain.localPlanet.factory.transport.stationPool[packet.StationId];
                    galacticTransport.AddStationComponent(packet.PlanetId, stationComponent);
                }
                else
                {
                    // If we're not on the same planet as the new station was create on, we need to create a 
                    // "fake" station that we can put into the GalacticTransport.stationPool instead of a real on
                    ILSShipManager.CreateFakeStationComponent(packet.StationGId, packet.PlanetId, true);
                }
            }
        }
    }
}

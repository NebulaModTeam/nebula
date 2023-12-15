#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

/*
 * If client knows the planets factory we call the removal there, if not we call it on the gStationPool if possible
 */
namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
internal class ILSRemoveStationComponentProcessor : PacketProcessor<ILSRemoveStationComponent>
{
    protected override void ProcessPacket(ILSRemoveStationComponent packet, NebulaConnection conn)
    {
        var pData = GameMain.galaxy.PlanetById(packet.PlanetId);
        if (pData?.factory?.transport != null && packet.StationId < pData.factory.transport.stationPool.Length)
        {
            using (Multiplayer.Session.Ships.PatchLockILS.On())
            {
                pData.factory.transport.RemoveStationComponent(packet.StationId);
            }
        }
        else
        {
            var gStationPool = GameMain.data.galacticTransport.stationPool;
            if (packet.StationGId >= gStationPool.Length)
            {
                return;
            }
            using (Multiplayer.Session.Ships.PatchLockILS.On())
            {
                GameMain.data.galacticTransport.RemoveStationComponent(packet.StationGId);
            }
        }
    }
}

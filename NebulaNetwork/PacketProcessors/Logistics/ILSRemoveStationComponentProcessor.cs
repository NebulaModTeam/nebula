﻿using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;

/*
 * If client knows the planets factory we call the removal there, if not we call it on the gStationPool if possible
 */
namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSRemoveStationComponentProcessor : PacketProcessor<ILSRemoveStationComponent>
    {
        public override void ProcessPacket(ILSRemoveStationComponent packet, NebulaConnection conn)
        {
            PlanetData pData = GameMain.galaxy.PlanetById(packet.PlanetId);
            if (pData?.factory?.transport != null && packet.StationId < pData.factory.transport.stationPool.Length)
            {
                using (ILSShipManager.PatchLockILS.On())
                {
                    pData.factory.transport.RemoveStationComponent(packet.StationId);
                }
            }
            else
            {
                StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
                if (packet.StationGId < gStationPool.Length)
                {
                    using (ILSShipManager.PatchLockILS.On())
                    {
                        GameMain.data.galacticTransport.RemoveStationComponent(packet.StationGId);
                    }
                }
            }
        }
    }
}

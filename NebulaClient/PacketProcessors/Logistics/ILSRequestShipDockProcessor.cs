using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using UnityEngine;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSRequestShipDockProcessor: IPacketProcessor<ILSRequestShipDock>
    {
        public void ProcessPacket(ILSRequestShipDock packet, NebulaConnection conn)
        {
            // a fake entry should already have been created
            StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.stationGId];
            stationComponent.shipDockPos.x = packet.shipDockPos.x;
            stationComponent.shipDockPos.y = packet.shipDockPos.y;
            stationComponent.shipDockPos.z = packet.shipDockPos.z;

            stationComponent.shipDockRot.x = packet.shipDockRot.x;
            stationComponent.shipDockRot.y = packet.shipDockRot.y;
            stationComponent.shipDockRot.z = packet.shipDockRot.z;
            stationComponent.shipDockRot.w = packet.shipDockRot.w;

            for (int i = 0; i < 10; i++)
            {
                stationComponent.shipDiskRot[i] = Quaternion.Euler(0f, 360f / (float)10 * (float)i, 0f);
                stationComponent.shipDiskPos[i] = stationComponent.shipDiskRot[i] * new Vector3(0f, 0f, 11.5f);
            }
            for (int j = 0; j < 10; j++)
            {
                stationComponent.shipDiskRot[j] = stationComponent.shipDockRot * stationComponent.shipDiskRot[j];
                stationComponent.shipDiskPos[j] = stationComponent.shipDockPos + stationComponent.shipDockRot * stationComponent.shipDiskPos[j];
            }
        }
    }
}

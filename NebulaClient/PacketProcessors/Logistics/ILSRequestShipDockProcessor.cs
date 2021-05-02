using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaModel.DataStructures;
using NebulaWorld.Logistics;
using UnityEngine;

/*
 * In order for the StationComponent.InternalTickRemote() method to correctly animate ship movement it needs to know
 * the position of the stations docking disk.
 * as we use fake entries in gStationPool for clients that have not visited the planet yet we also need to sync that position.
 */
namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSRequestShipDockProcessor: IPacketProcessor<ILSShipDock>
    {
        public void ProcessPacket(ILSShipDock packet, NebulaConnection conn)
        {
            // a fake entry should already have been created
            StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.stationGId];
            
            stationComponent.shipDockPos = DataStructureExtensions.ToVector3(packet.shipDockPos);
            stationComponent.shipDockRot = DataStructureExtensions.ToQuaternion(packet.shipDockRot);

            for (int i = 0; i < ILSShipManager.ILSMaxShipCount; i++)
            {
                stationComponent.shipDiskRot[i] = Quaternion.Euler(0f, 360f / (float)ILSShipManager.ILSMaxShipCount * (float)i, 0f);
                stationComponent.shipDiskPos[i] = stationComponent.shipDiskRot[i] * new Vector3(0f, 0f, 11.5f);
            }
            for (int j = 0; j < ILSShipManager.ILSMaxShipCount; j++)
            {
                stationComponent.shipDiskRot[j] = stationComponent.shipDockRot * stationComponent.shipDiskRot[j];
                stationComponent.shipDiskPos[j] = stationComponent.shipDockPos + stationComponent.shipDockRot * stationComponent.shipDiskPos[j];
            }

            // sync the current position of the ships as they might have been calculated wrong while we did not have the correct dock position and rotation.
            for(int i = 0; i < packet.shipOtherGId.Length; i++)
            {
                /*
                 * fix for #251
                 * for some reason shipOtherGId can be 0 in some cases.
                 * i thought about idle ships not having it set but im not sure. However checking for a 0 here fixes the issue.
                 */
                if(packet.shipOtherGId[i] > 0 && packet.shipOtherGId[i] < GameMain.data.galacticTransport.stationPool.Length)
                {
                    stationComponent = GameMain.data.galacticTransport.stationPool[packet.shipOtherGId[i]];

                    stationComponent.workShipDatas[packet.shipIndex[i]].uPos = DataStructureExtensions.ToVectorLF3(packet.shipPos[i]);
                    stationComponent.workShipDatas[packet.shipIndex[i]].uRot = DataStructureExtensions.ToQuaternion(packet.shipRot[i]);
                    stationComponent.workShipDatas[packet.shipIndex[i]].pPosTemp = DataStructureExtensions.ToVectorLF3(packet.shipPPosTemp[i]);
                    stationComponent.workShipDatas[packet.shipIndex[i]].pRotTemp = DataStructureExtensions.ToQuaternion(packet.shipPRotTemp[i]);
                }
            }
        }
    }
}

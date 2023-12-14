#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using UnityEngine;

#endregion

/*
 * In order for the StationComponent.InternalTickRemote() method to correctly animate ship movement it needs to know
 * the position of the stations docking disk.
 * as we use fake entries in gStationPool for clients that have not visited the planet yet we also need to sync that position.
 */
namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
internal class ILSResponseShipDockProcessor : PacketProcessor<ILSShipDock>
{
    protected override void ProcessPacket(ILSShipDock packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        // a fake entry should already have been created
        var stationComponent = GameMain.data.galacticTransport.stationPool[packet.stationGId];

        stationComponent.shipDockPos = packet.shipDockPos.ToVector3();
        stationComponent.shipDockRot = packet.shipDockRot.ToQuaternion();

        for (var i = 0; i < stationComponent.workShipDatas.Length; i++)
        {
            stationComponent.shipDiskRot[i] = Quaternion.Euler(0f, 360f / stationComponent.workShipDatas.Length * i, 0f);
            stationComponent.shipDiskPos[i] = stationComponent.shipDiskRot[i] * new Vector3(0f, 0f, 11.5f);
        }
        for (var j = 0; j < stationComponent.workShipDatas.Length; j++)
        {
            stationComponent.shipDiskRot[j] = stationComponent.shipDockRot * stationComponent.shipDiskRot[j];
            stationComponent.shipDiskPos[j] =
                stationComponent.shipDockPos + stationComponent.shipDockRot * stationComponent.shipDiskPos[j];
        }

        // sync the current position of the ships as they might have been calculated wrong while we did not have the correct dock position and rotation.
        for (var i = 0; i < packet.shipOtherGId.Length; i++)
        {
            /*
             * fix for #251
             * for some reason shipOtherGId can be 0 in some cases.
             * i thought about idle ships not having it set but im not sure. However checking for a 0 here fixes the issue.
             */
            if (packet.shipOtherGId[i] <= 0 || packet.shipOtherGId[i] >= GameMain.data.galacticTransport.stationPool.Length)
            {
                continue;
            }
            stationComponent = GameMain.data.galacticTransport.stationPool[packet.shipOtherGId[i]];

            stationComponent.workShipDatas[packet.shipIndex[i]].uPos = packet.shipPos[i].ToVectorLF3();
            stationComponent.workShipDatas[packet.shipIndex[i]].uRot = packet.shipRot[i].ToQuaternion();
            stationComponent.workShipDatas[packet.shipIndex[i]].pPosTemp = packet.shipPPosTemp[i].ToVectorLF3();
            stationComponent.workShipDatas[packet.shipIndex[i]].pRotTemp = packet.shipPRotTemp[i].ToQuaternion();
        }
    }
}

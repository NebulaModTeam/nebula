﻿#region

using System.Collections.Generic;
using NebulaAPI.DataStructures;
using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

/*
 * clients need to know the ship dock position for correct computation of the ship movement.
 * as clients dont have every PlanetFactory we use fake entries in gStationPool for ILS on planets that the client did not visit yet.
 * when they create a fake entry they also request the dock position, but we also need to tell the current ship
 * position and rotation for associated ships as they might have ben calculated wrong (without knowledge of dock position)
 */
namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSRequestShipDockProcessor : PacketProcessor<ILSRequestShipDock>
{
    protected override void ProcessPacket(ILSRequestShipDock packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var player = Players.Get(conn, EConnectionStatus.Connected) ?? Players.Get(conn, EConnectionStatus.Syncing);

        if (player == null || GameMain.data.galacticTransport.stationCapacity <= packet.StationGId)
        {
            return;
        }

        var shipOtherGId = new List<int>();
        var shipIndex = new List<int>();
        var shipPos = new List<Double3>();
        var shipRot = new List<Float4>();
        var shipPPosTemp = new List<Double3>();
        var shipPRotTemp = new List<Float4>();

        // find ShipData that has otherGId set to packet.stationGId
        for (var i = 0; i < GameMain.data.galacticTransport.stationCapacity; i++)
        {
            if (GameMain.data.galacticTransport.stationPool[i] == null)
            {
                continue;
            }

            var shipData = GameMain.data.galacticTransport.stationPool[i].workShipDatas;

            for (var j = 0; j < shipData.Length; j++)
            {
                if (shipData[j].otherGId != packet.StationGId)
                {
                    continue;
                }

                shipOtherGId.Add(shipData[j].otherGId);
                shipIndex.Add(j);
                shipPos.Add(new Double3(shipData[j].uPos.x, shipData[j].uPos.y, shipData[j].uPos.z));
                shipRot.Add(new Float4(shipData[j].uRot));
                shipPPosTemp.Add(
                    new Double3(shipData[j].pPosTemp.x, shipData[j].pPosTemp.y, shipData[j].pPosTemp.z));
                shipPRotTemp.Add(new Float4(shipData[j].pRotTemp));
            }
        }

        // also add add ships of current station as they use the dock pos too in the pos calculation
        // NOTE: we need to set this stations gid as otherStationGId so that the client accesses the array in the right way
        var shipData2 = GameMain.data.galacticTransport.stationPool[packet.StationGId].workShipDatas;

        for (var i = 0; i < shipData2.Length; i++)
        {
            shipOtherGId.Add(packet.StationGId);
            shipIndex.Add(i);
            shipPos.Add(new Double3(shipData2[i].uPos.x, shipData2[i].uPos.y, shipData2[i].uPos.z));
            shipRot.Add(new Float4(shipData2[i].uRot));
            shipPPosTemp.Add(new Double3(shipData2[i].pPosTemp.x, shipData2[i].pPosTemp.y, shipData2[i].pPosTemp.z));
            shipPRotTemp.Add(new Float4(shipData2[i].pRotTemp));
        }

        var packet2 = new ILSShipDock(packet.StationGId,
            GameMain.data.galacticTransport.stationPool[packet.StationGId].shipDockPos,
            GameMain.data.galacticTransport.stationPool[packet.StationGId].shipDockRot,
            shipOtherGId.ToArray(),
            shipIndex.ToArray(),
            shipPos.ToArray(),
            shipRot.ToArray(),
            shipPPosTemp.ToArray(),
            shipPRotTemp.ToArray());
        player.SendPacket(packet2);
    }
}

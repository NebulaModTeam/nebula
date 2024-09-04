#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics.ControlPanel;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics.ControlPanel;

[RegisterPacketProcessor]
public class LCPObjectEntryRequestProcessor : PacketProcessor<LCPObjectEntryRequest>
{
    protected override void ProcessPacket(LCPObjectEntryRequest packet, NebulaConnection conn)
    {
        if (IsClient) return;
        var factory = GameMain.galaxy.PlanetById(packet.AstroId)?.factory;
        if (factory == null) return;

        var isInit = false;
        var objId = packet.ObjId;
        if (objId < 0)
        {
            objId = -objId;
            isInit = true;
        }
        if (objId <= 0 || objId > factory.entityPool.Length) return;
        ref var entity = ref factory.entityPool[objId];
        if (entity.id != objId) return;

        switch ((EControlPanelEntryType)packet.EntryType)
        {
            case EControlPanelEntryType.InterstellarStation:
            case EControlPanelEntryType.OrbitCollector:
            case EControlPanelEntryType.LocalStation:
                if (entity.stationId == 0) return;
                if (isInit)
                {
                    LCPObjectEntryEntityInfo.Instance.Set(packet.Index, entity.protoId, entity.stationId, factory.ReadExtraInfoOnEntity(objId));
                    conn.SendPacket(LCPObjectEntryEntityInfo.Instance);
                }
                var station = factory.transport.stationPool[entity.stationId];
                LCPStationEntryUpdate.Instance.Set(packet.Index, station, factory);
                conn.SendPacket(LCPStationEntryUpdate.Instance);
                break;

            case EControlPanelEntryType.VeinCollector:
                if (entity.stationId == 0) return;
                if (isInit)
                {
                    LCPObjectEntryEntityInfo.Instance.Set(packet.Index, entity.protoId, entity.stationId, factory.ReadExtraInfoOnEntity(objId));
                    conn.SendPacket(LCPObjectEntryEntityInfo.Instance);
                }
                var veinCollector = factory.transport.stationPool[entity.stationId];
                LCPAdvancedMinerEntryUpdate.Instance.Set(packet.Index, veinCollector, factory);
                conn.SendPacket(LCPAdvancedMinerEntryUpdate.Instance);
                break;

            case EControlPanelEntryType.Dispenser:
                if (entity.dispenserId == 0) return;
                if (isInit)
                {
                    LCPObjectEntryEntityInfo.Instance.Set(packet.Index, entity.protoId, entity.dispenserId, factory.ReadExtraInfoOnEntity(objId));
                    conn.SendPacket(LCPObjectEntryEntityInfo.Instance);
                }
                var dispenser = factory.transport.dispenserPool[entity.dispenserId];
                LCPDispenserEntryUpdate.Instance.Set(packet.Index, dispenser, factory);
                conn.SendPacket(LCPDispenserEntryUpdate.Instance);
                break;
        }
    }
}

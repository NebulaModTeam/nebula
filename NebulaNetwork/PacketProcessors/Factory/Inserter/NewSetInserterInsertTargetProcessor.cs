#region

using NebulaAPI;
using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Inserter;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Inserter;

[RegisterPacketProcessor]
internal class NewSetInserterInsertTargetProcessor : PacketProcessor<NewSetInserterInsertTargetPacket>
{
    protected override void ProcessPacket(NewSetInserterInsertTargetPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null)
        {
            return;
        }
        Multiplayer.Session.Factories.TargetPlanet = factory.planetId;
        factory.WriteObjectConn(packet.ObjId, 0, true, packet.OtherObjId, -1);

        // setting specifyPlanet here to avoid accessing a null object (see GPUInstancingManager activePlanet getter)
        var pData = GameMain.gpuiManager.specifyPlanet;

        GameMain.gpuiManager.specifyPlanet = GameMain.galaxy.PlanetById(packet.PlanetId);
        factory.factorySystem.SetInserterInsertTarget(packet.InserterId, packet.OtherObjId, packet.Offset);
        GameMain.gpuiManager.specifyPlanet = pData;

        factory.factorySystem.inserterPool[packet.InserterId].pos2 = packet.PointPos.ToVector3();
        Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
    }
}

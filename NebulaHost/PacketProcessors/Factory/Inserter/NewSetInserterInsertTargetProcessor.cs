using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Inserter;
using NebulaModel.Packets.Processors;
using NebulaModel.DataStructures;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.Inserter
{
    [RegisterPacketProcessor]
    class NewSetInserterInsertTargetProcessor : IPacketProcessor<NewSetInserterInsertTargetPacket>
    {
        public void ProcessPacket(NewSetInserterInsertTargetPacket packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.data.factories[packet.FactoryIndex];
            if (factory != null)
            {
                FactoryManager.TargetPlanet = factory.planetId;
                factory.WriteObjectConn(packet.ObjId, 1, false, packet.OtherObjId, -1);
                factory.factorySystem.SetInserterInsertTarget(packet.InserterId, packet.OtherObjId, packet.Offset);
                factory.factorySystem.inserterPool[packet.InserterId].pos2 = packet.PointPos.ToVector3();
                FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
            }
        }
    }
}
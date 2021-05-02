using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Inserter;
using NebulaModel.Packets.Processors;
using NebulaModel.DataStructures;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.Inserter
{
    [RegisterPacketProcessor]
    class NewSetInserterPickTargetProcessor : IPacketProcessor<NewSetInserterPickTargetPacket>
    {
        public void ProcessPacket(NewSetInserterPickTargetPacket packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.data.factories[packet.FactoryIndex];
            if (factory != null)
            {
                FactoryManager.TargetPlanet = factory.planetId;
                factory.WriteObjectConn(packet.ObjId, 1, false, packet.OtherObjId, -1);
                factory.factorySystem.SetInserterPickTarget(packet.InserterId, packet.OtherObjId, packet.Offset);
                factory.entityPool[packet.ObjId].pos = packet.PointPos.ToVector3();
                FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
            }
        }
    }
}
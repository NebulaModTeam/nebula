using NebulaAPI;
using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Inserter;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.Inserter
{
    [RegisterPacketProcessor]
    class NewSetInserterInsertTargetProcessor : PacketProcessor<NewSetInserterInsertTargetPacket>
    {
        public override void ProcessPacket(NewSetInserterInsertTargetPacket packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            if (factory != null)
            {
                Multiplayer.Session.Factories.TargetPlanet = factory.planetId;
                factory.WriteObjectConn(packet.ObjId, 1, false, packet.OtherObjId, -1);
                factory.factorySystem.SetInserterInsertTarget(packet.InserterId, packet.OtherObjId, packet.Offset);
                factory.factorySystem.inserterPool[packet.InserterId].pos2 = packet.PointPos.ToVector3();
                Multiplayer.Session.Factories.TargetPlanet = FactoryManager.PLANET_NONE;
            }
        }
    }
}

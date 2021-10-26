using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Inserter;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.Inserter
{
    [RegisterPacketProcessor]
    internal class NewSetInserterPickTargetProcessor : PacketProcessor<NewSetInserterPickTargetPacket>
    {
        public override void ProcessPacket(NewSetInserterPickTargetPacket packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            if (factory != null)
            {
                Multiplayer.Session.Factories.TargetPlanet = factory.planetId;
                factory.WriteObjectConn(packet.ObjId, 1, false, packet.OtherObjId, -1);

                // setting specifyPlanet here to avoid accessing a null object (see GPUInstancingManager activePlanet getter)
                PlanetData pData = GameMain.gpuiManager.specifyPlanet;

                GameMain.gpuiManager.specifyPlanet = GameMain.galaxy.PlanetById(packet.PlanetId);
                factory.factorySystem.SetInserterPickTarget(packet.InserterId, packet.OtherObjId, packet.Offset);
                GameMain.gpuiManager.specifyPlanet = pData;

                factory.entityPool[packet.ObjId].pos = packet.PointPos.ToVector3();
                Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
            }
        }
    }
}
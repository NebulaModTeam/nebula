﻿using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Inserter;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.Inserter
{
    [RegisterPacketProcessor]
    internal class NewSetInserterInsertTargetProcessor : PacketProcessor<NewSetInserterInsertTargetPacket>
    {
        public override void ProcessPacket(NewSetInserterInsertTargetPacket packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            if (factory != null)
            {
                Multiplayer.Session.Factories.TargetPlanet = factory.planetId;
                factory.WriteObjectConn(packet.ObjId, 0, true, packet.OtherObjId, -1);

                // setting specifyPlanet here to avoid accessing a null object (see GPUInstancingManager activePlanet getter)
                PlanetData pData = GameMain.gpuiManager.specifyPlanet;

                GameMain.gpuiManager.specifyPlanet = GameMain.galaxy.PlanetById(packet.PlanetId);
                factory.factorySystem.SetInserterInsertTarget(packet.InserterId, packet.OtherObjId, packet.Offset);
                GameMain.gpuiManager.specifyPlanet = pData;

                factory.factorySystem.inserterPool[packet.InserterId].pos2 = packet.PointPos.ToVector3();
                Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
            }
        }
    }
}

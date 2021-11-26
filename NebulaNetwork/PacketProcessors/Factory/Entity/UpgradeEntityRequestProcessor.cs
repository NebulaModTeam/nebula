﻿using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using UnityEngine;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class UpgradeEntityRequestProcessor : PacketProcessor<UpgradeEntityRequest>
    {
        public override void ProcessPacket(UpgradeEntityRequest packet, NebulaConnection conn)
        {
            using (Multiplayer.Session.Factories.IsIncomingRequest.On())
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

                // We only execute the code if the client has loaded the factory at least once.
                // Else they will get it once they go to the planet for the first time. 
                if (planet?.factory == null)
                {
                    return;
                }

                Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
                Multiplayer.Session.Factories.PacketAuthor = packet.AuthorId;

                Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);
                ItemProto itemProto = LDB.items.Select(packet.UpgradeProtoId);

                Quaternion qRot = new Quaternion(packet.rot.x, packet.rot.y, packet.rot.z, packet.rot.w);
                Vector3 vPos = new Vector3(packet.pos.x, packet.pos.y, packet.pos.z);

                if (packet.IsPrebuild)
                {
                    for (int i = 1; i < planet.factory.prebuildCursor; i++)
                    {
                        PrebuildData pbData = planet.factory.prebuildPool[i];
                        if (pbData.pos == vPos && pbData.rot == qRot)
                        {
                            DoUpgrade(planet, -i, itemProto);
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < planet.factory.entityCursor; i++)
                    {
                        EntityData eData = planet.factory.entityPool[i];
                        if (eData.pos == vPos && eData.rot == qRot)
                        {
                            DoUpgrade(planet, i, itemProto);
                            break;
                        }
                    }
                }

                Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
                Multiplayer.Session.Factories.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
            }
        }

        private static void DoUpgrade(PlanetData planet, int objId, ItemProto itemProto)
        {
            // setting specifyPlanet here to avoid accessing a null object (see GPUInstancingManager activePlanet getter)
            PlanetData pData = GameMain.gpuiManager.specifyPlanet;

            GameMain.gpuiManager.specifyPlanet = planet;
            planet.factory.UpgradeFinally(GameMain.mainPlayer, objId, itemProto);
            GameMain.gpuiManager.specifyPlanet = pData;
        }
    }
}

﻿using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereDataProcessor : PacketProcessor<DysonSphereData>
    {
        public override void ProcessPacket(DysonSphereData packet, NebulaConnection conn)
        {
            if (IsHost) return;

            //Failsafe, if client does not have instantiated sphere for the star, it will create dummy one that will be replaced during import
            if (GameMain.data.dysonSpheres[packet.StarIndex] == null)
            {
                GameMain.data.dysonSpheres[packet.StarIndex] = new DysonSphere();
                GameMain.data.statistics.production.Init(GameMain.data);
                //Another failsafe, DysonSphere import requires initialized factory statistics
                if (GameMain.data.statistics.production.factoryStatPool[0] == null)
                {
                    GameMain.data.statistics.production.factoryStatPool[0] = new FactoryProductionStat();
                    GameMain.data.statistics.production.factoryStatPool[0].Init();
                }
                GameMain.data.dysonSpheres[packet.StarIndex].Init(GameMain.data, GameMain.data.galaxy.stars[packet.StarIndex]);
            }

            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.BinaryData))
            {
                GameMain.data.dysonSpheres[packet.StarIndex].Import(reader.BinaryReader);
            }
        }
    }
}

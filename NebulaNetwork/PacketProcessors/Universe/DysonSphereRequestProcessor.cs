using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    public class DysonSphereRequestProcessor : PacketProcessor<DysonSphereLoadRequest>
    {
        public override void ProcessPacket(DysonSphereLoadRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }
            switch (packet.Event)
            {
                case DysonSphereRequestEvent.List:
                    using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                    {
                        List<int> list = new List<int>();
                        for (int i = 0; i < GameMain.data.dysonSpheres.Length; ++i)
                        {
                            if (GameMain.data.dysonSpheres[i] != null)
                            {
                                list.Add(i);
                            }
                        }
                        writer.BinaryWriter.Write(list.Count);
                        foreach(int starIndex in list)
                        {
                            writer.BinaryWriter.Write(starIndex);
                        }
                        conn.SendPacket(new DysonSphereData(packet.StarIndex, writer.CloseAndGetBytes(), DysonSphereRespondEvent.List));
                    }
                    break;
                case DysonSphereRequestEvent.Load:
                    DysonSphere dysonSphere = GameMain.data.CreateDysonSphere(packet.StarIndex);
                    using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                    {
                        dysonSphere.Export(writer.BinaryWriter);
                        byte[] data = writer.CloseAndGetBytes();
                        Log.Info($"Sent {data.Length} bytes of data for DysonSphereData (INDEX: {packet.StarIndex})");
                        conn.SendPacket(new FragmentInfo(data.Length));
                        conn.SendPacket(new DysonSphereData(packet.StarIndex, data, DysonSphereRespondEvent.Load));
                        Multiplayer.Session.DysonSpheres.RegisterPlayer(conn, packet.StarIndex);
                    }
                    break;
                case DysonSphereRequestEvent.Unload:
                    Multiplayer.Session.DysonSpheres.UnRegisterPlayer(conn, packet.StarIndex);
                    break;
            }
        }
    }
}

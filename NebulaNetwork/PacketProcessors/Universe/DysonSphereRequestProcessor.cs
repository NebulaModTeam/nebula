#region

using System;
using System.Collections.Generic;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe;

[RegisterPacketProcessor]
public class DysonSphereRequestProcessor : PacketProcessor<DysonSphereLoadRequest>
{
    protected override void ProcessPacket(DysonSphereLoadRequest packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }
        switch (packet.Event)
        {
            case DysonSphereRequestEvent.List:
                using (var writer = new BinaryUtils.Writer())
                {
                    var list = new List<int>();
                    for (var i = 0; i < GameMain.data.dysonSpheres.Length; ++i)
                    {
                        if (GameMain.data.dysonSpheres[i] != null)
                        {
                            list.Add(i);
                        }
                    }
                    writer.BinaryWriter.Write(list.Count);
                    foreach (var starIndex in list)
                    {
                        writer.BinaryWriter.Write(starIndex);
                    }
                    conn.SendPacket(new DysonSphereData(packet.StarIndex, writer.CloseAndGetBytes(),
                        DysonSphereRespondEvent.List));
                }
                break;
            case DysonSphereRequestEvent.Load:
                var dysonSphere = GameMain.data.CreateDysonSphere(packet.StarIndex);
                using (var writer = new BinaryUtils.Writer())
                {
                    dysonSphere.Export(writer.BinaryWriter);
                    var data = writer.CloseAndGetBytes();
                    Log.Info($"Sent {data.Length} bytes of data for DysonSphereData (INDEX: {packet.StarIndex})");
                    conn.SendPacket(new FragmentInfo(data.Length));
                    conn.SendPacket(new DysonSphereData(packet.StarIndex, data, DysonSphereRespondEvent.Load));
                    Multiplayer.Session.DysonSpheres.RegisterPlayer(conn, packet.StarIndex);
                }
                break;
            case DysonSphereRequestEvent.Unload:
                Multiplayer.Session.DysonSpheres.UnRegisterPlayer(conn, packet.StarIndex);
                break;

            case DysonSphereRequestEvent.Query:
                // Ignore query if dyson sphere doesn't exist on host
                if (packet.StarIndex < 0 || packet.StarIndex >= GameMain.data.galaxy.starCount)
                {
                    return;
                }
                dysonSphere = GameMain.data.dysonSpheres[packet.StarIndex];
                if (dysonSphere == null)
                {
                    return;
                }
                using (var writer = new BinaryUtils.Writer())
                {
                    dysonSphere.Export(writer.BinaryWriter);
                    var data = writer.CloseAndGetBytes();
                    Log.Info($"Sent {data.Length} bytes of data for DysonSphereData (INDEX: {packet.StarIndex})");
                    conn.SendPacket(new FragmentInfo(data.Length));
                    conn.SendPacket(new DysonSphereData(packet.StarIndex, data, DysonSphereRespondEvent.Load));
                    Multiplayer.Session.DysonSpheres.RegisterPlayer(conn, packet.StarIndex);
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(packet), "Unknown DysonSphereRequestEvent: " + packet.Event);
        }
    }
}

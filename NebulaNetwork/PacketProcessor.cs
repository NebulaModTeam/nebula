#region

using System.Collections.Generic;
using NebulaAPI.DataStructures;
using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaWorld;

#endregion

namespace NebulaModel.Packets;

public abstract class PacketProcessor<T> : BasePacketProcessor<T>
{
    protected ConcurrentPlayerCollection Players => Multiplayer.Session.Server.Players;
    protected IServer Server => Multiplayer.Session.Server;
    protected IClient Client => Multiplayer.Session.Client;

    public override void ProcessPacket(T packet, INebulaConnection conn)
    {
        ProcessPacket(packet, (NebulaConnection)conn);
    }

    protected abstract void ProcessPacket(T packet, NebulaConnection conn);
}

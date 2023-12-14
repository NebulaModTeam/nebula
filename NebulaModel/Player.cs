#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;

#endregion

namespace NebulaModel;

public class NebulaPlayer : INebulaPlayer
{
    public NebulaPlayer(INebulaConnection connection, IPlayerData data)
    {
        Connection = connection;
        Data = data;
    }

    public INebulaConnection Connection { get; }
    public IPlayerData Data { get; private set; }
    public ushort Id => Data.PlayerId;

    public void SendPacket<T>(T packet) where T : class, new()
    {
        Connection.SendPacket(packet);
    }

    public void LoadUserData(IPlayerData data)
    {
        var localId = Id;
        Data = data;
        Data.PlayerId = localId;
    }
}

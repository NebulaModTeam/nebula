#region

using NebulaAPI.GameState;
using NebulaAPI.Networking;
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

    public INebulaConnection Connection { get; set; }
    public IPlayerData Data { get; set; }
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

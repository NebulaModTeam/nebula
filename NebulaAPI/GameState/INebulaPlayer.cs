#region

using NebulaAPI.Networking;

#endregion

namespace NebulaAPI.GameState;

public interface INebulaPlayer
{
    INebulaConnection Connection { get; set; }
    IPlayerData Data { get; set; }
    ushort Id { get; }

    void SendPacket<T>(T packet) where T : class, new();

    void LoadUserData(IPlayerData data);
}

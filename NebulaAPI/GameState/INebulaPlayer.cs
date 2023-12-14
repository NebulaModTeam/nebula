namespace NebulaAPI;

public interface INebulaPlayer
{
    INebulaConnection Connection { get; }
    IPlayerData Data { get; }
    ushort Id { get; }

    void SendPacket<T>(T packet) where T : class, new();

    void LoadUserData(IPlayerData data);
}

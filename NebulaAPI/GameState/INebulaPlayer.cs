namespace NebulaAPI
{
    public interface INebulaPlayer
    {
        INebulaConnection Connection { get; }
        IPlayerData Data { get; }
        ushort Id { get; }
        int CurrentResearchId { get; }
        long TechProgressContributed { get; }
        void SendPacket<T>(T packet) where T : class, new();
        void LoadUserData(IPlayerData data);
    }
}
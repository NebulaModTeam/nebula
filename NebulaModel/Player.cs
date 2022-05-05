using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Networking;

namespace NebulaModel
{
    public class NebulaPlayer : INebulaPlayer
    {
        public INebulaConnection Connection { get; private set; }
        public IPlayerData Data { get; private set; }
        public ushort Id => Data.PlayerId;
        public NebulaPlayer(NebulaConnection connection, PlayerData data)
        {
            Connection = connection;
            Data = data;
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            Connection.SendPacket(packet);
        }

        public void SendRawPacket(byte[] packet)
        {
            ((NebulaConnection)Connection).SendRawPacket(packet);
        }

        public void LoadUserData(IPlayerData data)
        {
            ushort localId = Id;
            Data = data;
            Data.PlayerId = localId;
        }
    }
}

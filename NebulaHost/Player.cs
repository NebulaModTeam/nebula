using LiteNetLib;
using NebulaModel.DataStructures;
using NebulaModel.Networking;

namespace NebulaHost
{
    public class Player
    {
        public NebulaConnection Connection { get; private set; }
        public PlayerData Data { get; private set; }
        public ushort Id => Data.PlayerId;

        public Player(NebulaConnection connection, PlayerData data)
        {
            Connection = connection;
            Data = data;
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            Connection.SendPacket(packet);
        }
    }
}

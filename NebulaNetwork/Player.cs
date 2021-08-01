using Mirror;
using NebulaModel.DataStructures;
using NebulaModel.Networking.Serialization;
using static NebulaModel.Networking.NebulaConnection;

namespace NebulaNetwork
{
    public class Player
    {
        public NetworkConnection Connection { get; private set; }
        public PlayerData Data { get; private set; }
        public ushort Id => Data.PlayerId;
        public int CurrentResearchId { get; private set; }
        public long TechProgressContributed { get; private set; }


        public NetPacketProcessor PacketProcessor { get; private set; }

        public Player(NetworkConnection connection, PlayerData data, NetPacketProcessor netPacketProcessor)
        {
            Connection = connection;
            Data = data;
            PacketProcessor = netPacketProcessor;
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            Connection.SendPacket(packet);
        }

        public void LoadUserData(PlayerData data)
        {
            ushort localId = Id;
            Data = data;
            Data.PlayerId = localId;
        }

        public void UpdateResearchProgress(int techId, long techprogress)
        {
            //no research active or tech has changed, this is the inital packet
            if (CurrentResearchId == 0 || CurrentResearchId != techId)
            {
                CurrentResearchId = techId;
                TechProgressContributed = techprogress;
            }
            else
            {
                TechProgressContributed += techprogress;
            }
        }

        //resets the current research progress and returns contributed hashes
        public long ReleaseResearchProgress()
        {
            long holder = TechProgressContributed;
            CurrentResearchId = 0;
            TechProgressContributed = 0;
            return holder;
        }
    }
}

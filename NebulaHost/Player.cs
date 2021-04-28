using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Logger;

namespace NebulaHost
{
    public class Player
    {
        public NebulaConnection Connection { get; private set; }
        public PlayerData Data { get; private set; }
        public ushort Id => Data.PlayerId;
        public int CurrentResearchId { get; private set; }
        public long TechProgressContributed { get; private set; }

        public Player(NebulaConnection connection, PlayerData data)
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
            Connection.SendRawPacket(packet);
        }

        public void LoadUserData(PlayerData data)
        {
            ushort localId = Id;
            Data = data;
            Data.PlayerId = localId;
        }

        public void UpdateResearchProgress(int techId, long techprogress)
        {
            //no research active, inital state
            if (CurrentResearchId == 0)
            {
                CurrentResearchId = techId;
                TechProgressContributed = techprogress;
                Log.Info($"UpdateResearchProgress: started ResearchProgress for item {CurrentResearchId} with inital value {TechProgressContributed}");
            }
            else
            {
                Log.Info($"UpdateResearchProgress: updated progress for player by {techprogress}, now total {TechProgressContributed}");
                TechProgressContributed += techprogress;
            }
        }

        public long ReleaseResearchProgress()
        {
            Log.Info($"ReleaseResearchProgress: releasing {TechProgressContributed} hashes for playerid {this.Id}");
            long holder = TechProgressContributed;
            CurrentResearchId = 0;
            TechProgressContributed = 0;
            return holder;
        }
    }
}

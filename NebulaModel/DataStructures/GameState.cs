using LiteNetLib.Utils;
using NebulaModel.Attributes;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public struct GameState : INetSerializable
    {
        public long timestamp;
        public long gameTick;

        public GameState(long timestamp, long gameTick)
        {
            this.timestamp = timestamp;
            this.gameTick = gameTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(timestamp);
            writer.Put(gameTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            timestamp = reader.GetLong();
            gameTick = reader.GetLong();
        }
    }
}

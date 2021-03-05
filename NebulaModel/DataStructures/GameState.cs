using LiteNetLib.Utils;
using NebulaModel.Attributes;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public struct GameState : INetSerializable
    {
        public long gameTick;

        public GameState(long gameTick)
        {
            this.gameTick = gameTick;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(gameTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            gameTick = reader.GetLong();
        }
    }
}

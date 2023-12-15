#region

using NebulaAPI.Interfaces;
using NebulaAPI.Packets;

#endregion

namespace NebulaModel.DataStructures;

[RegisterNestedType]
public struct GameState : INetSerializable
{
    private long timestamp;
    private long gameTick;

    public GameState(long timestamp, long gameTick)
    {
        this.timestamp = timestamp;
        this.gameTick = gameTick;
    }

    public void Serialize(INetDataWriter writer)
    {
        writer.Put(timestamp);
        writer.Put(gameTick);
    }

    public void Deserialize(INetDataReader reader)
    {
        timestamp = reader.GetLong();
        gameTick = reader.GetLong();
    }
}

using LiteNetLib.Utils;

namespace NebulaModel.DataStructures
{
    public struct NebulaId : INetSerializable
    {
        public string id;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
        }

        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetString();
        }
    }
}

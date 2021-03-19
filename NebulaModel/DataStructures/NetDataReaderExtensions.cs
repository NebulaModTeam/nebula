using NebulaModel.Networking.Serialization;

namespace NebulaModel.DataStructures
{
    public static class NetDataReaderExtensions
    {
        public static Float3 GetFloat3(this NetDataReader reader)
        {
            Float3 value = new Float3();
            value.Deserialize(reader);
            return value;
        }

        public static Float4 GetFloat4(this NetDataReader reader)
        {
            Float4 value = new Float4();
            value.Deserialize(reader);
            return value;
        }

        public static Double3 GetDouble3(this NetDataReader reader)
        {
            Double3 value = new Double3();
            value.Deserialize(reader);
            return value;
        }
    }
}

using NebulaModel.Networking.Serialization;

namespace NebulaTests.NebulaModel.Networking.Serialization;

public class SampleDictionaryPacket
{
    public Dictionary<int, int> SampleInts { get; set; } = new();

    public Dictionary<SampleCustomClass, SampleCustomClass> SampleCustomTypes { get; set; } = new();
    public Dictionary<SampleNetSerializableClass, SampleNetSerializableStruct> SampleNetSerializables { get; set; } = new();


    // This is the one used by Assert.AreEqual, we don't need to implement the rest for now.
    public override bool Equals(object? obj)
    {
        var other = (SampleDictionaryPacket)obj!;

        foreach (var kvp in SampleInts)
        {
            if (!other.SampleInts.ContainsKey(kvp.Key))
                return false;
            if (other.SampleInts[kvp.Key] != SampleInts[kvp.Key])
                return false;
        }

        foreach (var kvp in SampleCustomTypes)
        {
            if (!other.SampleCustomTypes.ContainsKey(kvp.Key))
                return false;
            if (other.SampleCustomTypes[kvp.Key] != SampleCustomTypes[kvp.Key])
                return false;
        }

        return true;
    }
}

[TestClass]
public class DictionarySerializationTest
{
    private readonly SampleDictionaryPacket basePacket = new()
    {
        SampleInts = new() { { 1, 1 }, { 2, 2 }, { 3, 3 } },
        SampleCustomTypes =
            new()
            {
                { new SampleCustomClass { Value = 1 }, new SampleCustomClass() { Value = 2 } },
                { new SampleCustomClass { Value = 3 }, new SampleCustomClass() { Value = 4 } },
                { new SampleCustomClass { Value = 5 }, new SampleCustomClass() { Value = 6 } }
            },
        SampleNetSerializables =
            new()
            {
                { new SampleNetSerializableClass { value = 1 }, new SampleNetSerializableStruct { value = 1 } },
                { new SampleNetSerializableClass { value = 2 }, new SampleNetSerializableStruct { value = 2 } },
                { new SampleNetSerializableClass { value = 3 }, new SampleNetSerializableStruct { value = 3 } },
            }
    };

    [TestMethod, Timeout(1000)]
    public void SerializeDeserialize_DictionaryPackets_AreEqual()
    {
        var packetProcessor = new NebulaNetPacketProcessor();
        packetProcessor.RegisterNestedType<SampleNetSerializableClass>(() => new SampleNetSerializableClass());

        // Subscribe first to force a packet type register call
        SampleDictionaryPacket receivedPacket = null;
        packetProcessor.SubscribeReusable<SampleDictionaryPacket>(
            packet =>
            {
                receivedPacket = packet;
            });

        var writer = new NetDataWriter();
        packetProcessor.Write(writer, basePacket);

        var reader = new NetDataReader(writer);
        packetProcessor.ReadAllPackets(reader);

        UnitTesting.Assert.AreEqual(basePacket, receivedPacket);
    }
}

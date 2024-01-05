namespace NebulaAPI.Networking;

public interface INetPacketProcessor
{
    bool SimulateLatency { get; set; }

    /// <summary>
    /// Whether or not packet processing is enabled
    /// </summary>
    bool EnablePacketProcessing { get; set; }

    void EnqueuePacketForProcessing<T>(T packet, object userData) where T : class, new();
    void EnqueuePacketForProcessing(byte[] rawData, object userData);
    byte[] Write<T>(T packet) where T : class, new();
    void ProcessPacketQueue();
}

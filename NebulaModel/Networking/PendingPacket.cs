namespace NebulaModel.Networking;

public struct PendingPacket
{
    public byte[] Data { get; set; }
    public object UserData { get; set; }

    public PendingPacket(byte[] data, object userData)
    {
        Data = data;
        UserData = userData;
    }
}

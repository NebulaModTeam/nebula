namespace NebulaAPI.Packets;

public interface IDeferredPacket<TUpdateData>
{
    public void UpdatePacket(TUpdateData data);

    public void Reset();
}


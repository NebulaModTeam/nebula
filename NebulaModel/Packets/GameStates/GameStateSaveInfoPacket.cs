#region

using NebulaAPI.Packets;

#endregion

namespace NebulaModel.Packets.GameStates;

[HidePacketInDebugLogs]
public class GameStateSaveInfoPacket
{
    public GameStateSaveInfoPacket() { }

    public GameStateSaveInfoPacket(long lastSaveTime)
    {
        LastSaveTime = lastSaveTime;
    }

    public long LastSaveTime { get; set; }
}

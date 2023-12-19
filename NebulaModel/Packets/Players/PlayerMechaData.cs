#region

using NebulaAPI.Packets;
using NebulaModel.DataStructures;

#endregion

namespace NebulaModel.Packets.Players;

[HidePacketInDebugLogs]
public class PlayerMechaData
{
    public PlayerMechaData() { }

    public PlayerMechaData(Player player)
    {
        Data = new MechaData(player);
    }

    public MechaData Data { get; set; }
}

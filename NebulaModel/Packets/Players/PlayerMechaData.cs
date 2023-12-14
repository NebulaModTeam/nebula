#region

using NebulaAPI;
using NebulaModel.DataStructures;

#endregion

namespace NebulaModel.Packets.Players;

[HidePacketInDebugLogs]
public class PlayerMechaData
{
    public PlayerMechaData() { }

    public PlayerMechaData(Player player)
    {
        Data = new MechaData(player.sandCount, player.mecha.coreEnergy, player.mecha.reactorEnergy, player.package,
            player.deliveryPackage, player.mecha.reactorStorage, player.mecha.warpStorage, player.mecha.forge);
    }

    public MechaData Data { get; set; }
}

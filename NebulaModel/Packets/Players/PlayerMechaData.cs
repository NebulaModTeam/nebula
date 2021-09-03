using NebulaAPI;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Players
{
    [HidePacketInDebugLogs]
    public class PlayerMechaData
    {
        public MechaData Data { get; set; }

        public PlayerMechaData() { }
        public PlayerMechaData(Player player, int researchHashRate = 0)
        {
            Data = new MechaData(player.sandCount, player.mecha.coreEnergy, player.mecha.reactorEnergy, player.package, player.mecha.reactorStorage, player.mecha.warpStorage, player.mecha.forge, researchHashRate);
        }
    }
}

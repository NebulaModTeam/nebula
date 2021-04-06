using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Players
{
    public class PlayerMechaData
    {
        public MechaData Data { get; set; }

        public PlayerMechaData() { }
        public PlayerMechaData(Player player)
        {
            Data = new MechaData(player.sandCount, player.mecha.coreEnergy, player.mecha.reactorEnergy, player.package, player.mecha.reactorStorage, player.mecha.warpStorage, player.mecha.forge);
        }
    }
}

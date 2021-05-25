namespace NebulaModel.Packets.Players
{
    public class PlayerUseWarper
    {
        public bool WarpCommand { get; set; }
        public ushort PlayerId { get; set; }
        public PlayerUseWarper() { }
        public PlayerUseWarper(bool WarpCommand)
        {
            this.WarpCommand = WarpCommand;
        }
    }
}

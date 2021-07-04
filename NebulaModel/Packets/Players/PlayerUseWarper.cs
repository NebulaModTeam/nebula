namespace NebulaModel.Packets.Players
{
    // Packet sent when player starts or stops warping with the desired state
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
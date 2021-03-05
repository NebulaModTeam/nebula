using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Players
{
    public class PlayerColorChanged
    {
        public ushort PlayerId { get; set; }
        public Float3 Color { get; set; }

        public PlayerColorChanged() { }
        public PlayerColorChanged(ushort playerID, Float3 color)
        {
            PlayerId = playerID;
            Color = color;
        }
    }
}

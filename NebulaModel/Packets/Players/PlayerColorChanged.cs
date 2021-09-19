using NebulaAPI;

namespace NebulaModel.Packets.Players
{
    public class PlayerColorChanged
    {
        public ushort PlayerId { get; set; }
        public Float4[] Colors { get; set; }

        public PlayerColorChanged() { }
        public PlayerColorChanged(ushort playerID, Float4[] colors)
        {
            PlayerId = playerID;
            Colors = colors;
        }
    }
}

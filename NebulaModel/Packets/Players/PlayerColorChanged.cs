using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Players
{
	public class PlayerColorChanged
	{
		public ushort PlayerId { get; set; }
		public Float4 Color { get; set; }

		public PlayerColorChanged() { }
		public PlayerColorChanged(ushort playerID, Float4 color) 
		{
			this.PlayerId = playerID;
			this.Color = color;
		}
	}
}

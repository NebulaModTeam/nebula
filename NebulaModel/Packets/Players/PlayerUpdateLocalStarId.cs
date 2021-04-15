namespace NebulaModel.Packets.Players
{
    public class PlayerUpdateLocalStarId
    {
        public int StarId { get; set; }

        public PlayerUpdateLocalStarId() { }

        public PlayerUpdateLocalStarId(int starId)
        {
            StarId = starId;
        }
    }
}

namespace NebulaModel.Packets.Universe
{
    // Packet for name input for Planets and Stars
    public class NameInputPacket
    {
        public string Name { get; set; }
        public int PlanetId { get; set; }
        public int StarId { get; set; }
        public int AuthorId { get; set; }

        public NameInputPacket() { }
        public NameInputPacket(string name, int starId, int planetId, int authorId)
        {
            Name = name;
            StarId = starId;
            PlanetId = planetId;
            AuthorId = authorId;
        }
    }
}

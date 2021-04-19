namespace NebulaModel.Packets.Factory
{
    public class BuildEntityRequest
    {
        public int PlanetId { get; set; }
        public int PrebuildId { get; set; }
        public int AuthorId { get; set; }

        public BuildEntityRequest() { }
        public BuildEntityRequest(int planetId, int prebuildId, int authorId)
        {
            PlanetId = planetId;
            PrebuildId = prebuildId;
            AuthorId = authorId;
        }
    }
}

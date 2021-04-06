namespace NebulaModel.Packets.Factory
{
    public class BuildEntityRequest
    {
        public int PlanetId { get; set; }
        public int PrebuildId { get; set; }

        public BuildEntityRequest() { }
        public BuildEntityRequest(int planetId, int prebuildId)
        {
            PlanetId = planetId;
            PrebuildId = prebuildId;
        }
    }
}

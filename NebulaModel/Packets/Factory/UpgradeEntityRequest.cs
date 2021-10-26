using NebulaAPI;

namespace NebulaModel.Packets.Factory
{
    public class UpgradeEntityRequest
    {
        public int PlanetId { get; set; }
        public Float3 pos { get; set; }
        public Float4 rot { get; set; }
        public int UpgradeProtoId { get; set; }
        public int AuthorId { get; set; }

        public UpgradeEntityRequest() { }
        public UpgradeEntityRequest(int planetId, Float3 pos, Float4 rot, int upgradeProtoId, int authorId)
        {
            PlanetId = planetId;
            this.pos = pos;
            this.rot = rot;
            UpgradeProtoId = upgradeProtoId;
            AuthorId = authorId;
        }
    }
}

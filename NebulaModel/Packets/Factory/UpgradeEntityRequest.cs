namespace NebulaModel.Packets.Factory
{
    public class UpgradeEntityRequest
    {
        public int PlanetId { get; set; }
        public int ObjId { get; set; }
        public int Grade { get; set; }
        public int UpgradeProtoId { get; set; }
        public int AuthorId { get; set; }

        public UpgradeEntityRequest() { }
        public UpgradeEntityRequest(int planetId, int objId, int grade, int upgradeProtoId, int authorId)
        {
            PlanetId = planetId;
            ObjId = objId;
            Grade = grade;
            UpgradeProtoId = upgradeProtoId;
            AuthorId = authorId;
        }
    }
}

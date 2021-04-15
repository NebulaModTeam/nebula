namespace NebulaModel.Packets.Factory
{
    public class DestructEntityRequest
    {
        public int PlanetId { get; set; }
        public int ObjId { get; set; }
        public int AuthorId { get; set; }

        public DestructEntityRequest() { }
        public DestructEntityRequest(int planetId, int objId, int authorId)
        {
            AuthorId = authorId;
            PlanetId = planetId;
            ObjId = objId;
        }
    }
}

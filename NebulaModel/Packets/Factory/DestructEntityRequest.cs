namespace NebulaModel.Packets.Factory
{
    public class DestructEntityRequest
    {
        public int PlanetId { get; set; }
        public int ObjId { get; set; }

        public DestructEntityRequest() { }
        public DestructEntityRequest(int planetId, int objId)
        {
            PlanetId = planetId;
            ObjId = objId;
        }
    }
}

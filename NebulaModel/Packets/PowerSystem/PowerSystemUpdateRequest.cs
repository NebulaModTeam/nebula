namespace NebulaModel.Packets.PowerSystem
{
    public class PowerSystemUpdateRequest
    {
        public int[] PlanetIDs { get; set; }
        public PowerSystemUpdateRequest() { }
        public PowerSystemUpdateRequest(int[] planetIDs)
        {
            PlanetIDs = planetIDs;
        }
    }
}

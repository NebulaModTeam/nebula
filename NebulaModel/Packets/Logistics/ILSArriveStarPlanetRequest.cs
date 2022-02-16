/*
 * when a client arrives at a star he requests the current storages from all ILS in that system
 * this will also sync the belt filters
 */
namespace NebulaModel.Packets.Logistics
{
    public class ILSArriveStarPlanetRequest
    {
        public int StarId { get; set; }
        public ILSArriveStarPlanetRequest() { }
        public ILSArriveStarPlanetRequest(int starId)
        {
            StarId = starId;
        }
    }
}

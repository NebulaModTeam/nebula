using NebulaAPI;

namespace NebulaModel.Packets.Players
{
    [HidePacketInDebugLogs]
    [NetworkOptions(Reliable = false)]
    public class PlayerMovement
    {
        public ushort PlayerId { get; set; }
        public int LocalPlanetId { get; set; }
        public Float3 LocalPlanetPosition { get; set; }
        public Double3 UPosition { get; set; }
        public Float3 Rotation { get; set; }
        public Float3 BodyRotation { get; set; }

        public PlayerMovement() { }

        public PlayerMovement(ushort playerId, int localPlanetId, Float3 localPlanetPosition, Double3 uPosition, Float3 rotation, Float3 bodyRotation)
        {
            PlayerId = playerId;
            LocalPlanetId = localPlanetId;
            LocalPlanetPosition = localPlanetPosition;
            UPosition = uPosition;
            Rotation = rotation;
            BodyRotation = bodyRotation;
        }
    }
}

using NebulaModel.Attributes;

namespace NebulaModel
{
    public class MultiplayerOptions
    {
        public enum EGameMode
        {
            Coop,
            MMO,
        }

        [UIControl("Enabled")]
        public bool Enabled { get; set; } = true;

        [UIControl("Host Port")]
        public int HostPort { get; set; } = 8469;

        [UIRange(10, 80)]
        [UIControl("Drone Max Distance")]
        public float DroneMaxDistance { get; set; } = 80;

        [UIControl("Nickname")]
        public string Nickname { get; set; }

        [UIControl("Game Mode")]
        public EGameMode GameMode { get; set; }
    }
}

using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Session
{
    public class HandshakeRequest
    {
        public string Username { get; set; }
        public Float3 MechaColor { get; set; }
        public string ModVersion { get; set; }
        public bool HasGS2 { get; set; }
        public int GameVersionSig { get; set; }
        public byte[] ClientCert { get; set; }

        public HandshakeRequest() { }

        public HandshakeRequest(byte[] clientCert, string username, Float3 mechaColor, bool hasGS2 = false)
        {
            this.Username = username;
            this.MechaColor = mechaColor;
            this.ModVersion = Config.ModVersion;
            this.HasGS2 = hasGS2;
            this.GameVersionSig = GameConfig.gameVersion.sig;
            this.ClientCert = clientCert;
        }
    }
}

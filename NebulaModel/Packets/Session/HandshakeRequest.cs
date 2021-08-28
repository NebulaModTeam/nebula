using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Session
{
    public class HandshakeRequest
    {
        public string Username { get; set; }
        public Float3 MechaColor { get; set; }
        public string ModVersion { get; set; }
        public int GameVersionSig { get; set; }
        public byte[] ClientCert { get; set; }

        public HandshakeRequest() { }

        public HandshakeRequest(byte[] clientCert, string username, Float3 mechaColor)
        {
            this.Username = username;
            this.MechaColor = mechaColor;
            this.ModVersion = Config.ModVersion;
            this.GameVersionSig = GameConfig.gameVersion.sig;
            this.ClientCert = clientCert;
        }
    }
}

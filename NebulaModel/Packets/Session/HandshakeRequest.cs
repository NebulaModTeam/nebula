namespace NebulaModel.Packets.Session
{
    public class HandshakeRequest
    {
        public string ModVersion { get; set; }
        public int GameVersionSig { get; set; }
        public byte[] ClientCert { get; set; }

        public HandshakeRequest() { }

        public HandshakeRequest(byte[] clientCert)
        {
            this.ModVersion = Config.ModVersion.ToString();
            this.GameVersionSig = GameConfig.gameVersion.sig;
            this.ClientCert = clientCert;
        }
    }
}

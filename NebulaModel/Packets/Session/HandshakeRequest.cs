namespace NebulaModel.Packets.Session
{
    public class HandshakeRequest
    {
        public string ModVersion { get; set; }
        public byte[] ClientCert { get; set; }

        public HandshakeRequest() { }

        public HandshakeRequest(byte[] clientCert)
        {
            this.ModVersion = Config.ModInfo.Metadata.Version.ToString();
            this.ClientCert = clientCert;
        }
    }
}

namespace NebulaModel.Packets.Session
{
    public class HandshakeRequest
    {
        public readonly uint ProtocolVersion = 0;
        public byte[] ClientCert { get; set; }

        public HandshakeRequest() { }

        public HandshakeRequest(byte[] clientCert)
        {
            this.ClientCert = clientCert;
        }
    }
}

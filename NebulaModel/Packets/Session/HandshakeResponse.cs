namespace NebulaModel.Packets.Session
{
    public class HandshakeResponse
    {
        public ushort[] OtherPlayerIds { get; set; }

        public HandshakeResponse() { }

        public HandshakeResponse(ushort[] otherPlayerIds)
        {
            OtherPlayerIds = otherPlayerIds;
        }
    }
}

namespace NebulaModel.Packets.Session
{
    public class HandshakeResponse
    {
        public bool IsFirstPlayer { get; set; }

        public ushort[] OtherPlayerIds { get; set; }

        public HandshakeResponse() { }

        public HandshakeResponse(bool isFirstPlayer, ushort[] otherPlayerIds)
        {
            this.IsFirstPlayer = isFirstPlayer;
            this.OtherPlayerIds = otherPlayerIds;
        }
    }
}

namespace NebulaModel.Packets.Players
{
    public class ChatCommandWhoPacket
    {
        public bool IsRequest { get; set; }
        public string ResponsePayload { get; set; }

        public ChatCommandWhoPacket() { }

        public ChatCommandWhoPacket(bool isRequest, string responsePayload)
        {
            IsRequest = isRequest;
#if DEBUG
            if (!isRequest)
            {
                Assert.False(string.IsNullOrEmpty(responsePayload));
            }
#endif
            ResponsePayload = responsePayload;
        }
    }
}
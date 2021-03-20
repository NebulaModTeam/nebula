namespace NebulaModel.Networking
{
    public struct PendingPacket
    {
        public byte[] Data { get; }
        public object UserData { get; }

        public PendingPacket(byte[] data, object userData)
        {
            this.Data = data;
            this.UserData = userData;
        }
    }
}

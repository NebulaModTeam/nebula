namespace NebulaModel.Networking
{
    public class PendingPacket
    {
        public byte[] Data { get; }
        public NebulaConnection Connection { get; }

        public PendingPacket(byte[] data, NebulaConnection connection)
        {
            this.Data = data;
            this.Connection = connection;
        }
    }
}

using Mirror;
using NebulaModel.Networking.Serialization;
using System;

namespace NebulaModel.Networking
{
    public static class NebulaConnection
    {
        public struct NebulaMessage : NetworkMessage
        {
            public int Fragment;
            public bool MoreFragments;
            public int TotalLength;
            public byte[] Data;
        }
        public static NetPacketProcessor PacketProcessor { get; set; }

        private static byte[] CompletePayload;

        private static int MaxMessageSizeInBytes => Transport.activeTransport.GetMaxPacketSize() - sizeof(int) - sizeof(bool) - sizeof(int) - 14;

        public static void SendPacket<T>(this NetworkConnection connection, T packet) where T : class, new()
        {
            var processedPacket = PacketProcessor.Write(packet);

            Logger.Log.Debug($"Sending NebulaMessage of type {packet.GetType()} to client {connection.connectionId}");
            for (int i = 0; i < processedPacket.Length; i += MaxMessageSizeInBytes)
            {
                int fragment = i / MaxMessageSizeInBytes;
                NebulaMessage msg = new NebulaMessage()
                {
                    Fragment = fragment,
                    MoreFragments = (processedPacket.Length - i) > MaxMessageSizeInBytes,
                    TotalLength = processedPacket.Length,
                    Data = new byte[processedPacket.Length - i > MaxMessageSizeInBytes ? MaxMessageSizeInBytes : processedPacket.Length - i]
                };
                Array.Copy(processedPacket, i, msg.Data, 0, msg.Data.Length);
                if (connection == null) return;
                connection.Send(msg);
            }
        }

        public static void OnNebulaMessage(NebulaMessage nebulaMessage) => OnNebulaMessage(NetworkClient.connection, nebulaMessage);
        public static void OnNebulaMessage(NetworkConnection networkConnection, NebulaMessage nebulaMessage)
        {
            if (nebulaMessage.Fragment == 0 && nebulaMessage.MoreFragments == false)
            {
                PacketProcessor.EnqueuePacketForProcessing(nebulaMessage.Data, networkConnection);
                return;
            }

            if (nebulaMessage.Fragment == 0)
            {
                CompletePayload = new byte[nebulaMessage.TotalLength];
            }

            var index = nebulaMessage.MoreFragments ? ((nebulaMessage.Fragment) * MaxMessageSizeInBytes) : (CompletePayload.Length - nebulaMessage.Data.Length);
            Array.Copy(nebulaMessage.Data, 0, CompletePayload, index, nebulaMessage.Data.Length);

            if (nebulaMessage.MoreFragments == false)
            {
                PacketProcessor.EnqueuePacketForProcessing(CompletePayload, networkConnection);
            }
        }
    }
}

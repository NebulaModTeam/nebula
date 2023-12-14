#region

using System;
using System.IO;
using K4os.Compression.LZ4.Streams;
using NebulaAPI;

#endregion

namespace NebulaModel.Networking;

public static class BinaryUtils
{
    private const int BUFFER_SIZE = 8192;

    public class Writer : IWriterProvider
    {
        private readonly BufferedStream bs;
        private readonly LZ4EncoderStream ls;
        private readonly MemoryStream ms;

        public Writer()
        {
            ms = new MemoryStream();
            ls = LZ4Stream.Encode(ms);
            bs = new BufferedStream(ls, BUFFER_SIZE);
            BinaryWriter = new BinaryWriter(bs);
        }

        public BinaryWriter BinaryWriter { get; }

        public void Dispose()
        {
            BinaryWriter?.Close();
            bs?.Dispose();
            ls?.Dispose();
            ms?.Dispose();
            GC.SuppressFinalize(this);
        }

        public byte[] CloseAndGetBytes()
        {
            BinaryWriter?.Close();
            return ms?.ToArray() ?? Array.Empty<byte>();
        }
    }

    public class Reader : IReaderProvider
    {
        private readonly BufferedStream bs;
        private readonly LZ4DecoderStream ls;

        public Reader(byte[] bytes)
        {
            MemoryStream = new MemoryStream(bytes);
            ls = LZ4Stream.Decode(MemoryStream);
            bs = new BufferedStream(ls, BUFFER_SIZE);
            BinaryReader = new BinaryReader(bs);
        }

        public MemoryStream MemoryStream { get; }

        public BinaryReader BinaryReader { get; }

        public void Dispose()
        {
            BinaryReader?.Close();
            bs?.Dispose();
            ls?.Dispose();
            MemoryStream?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

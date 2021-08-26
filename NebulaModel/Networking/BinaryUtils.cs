using K4os.Compression.LZ4.Streams;
using NebulaAPI;
using System.IO;

namespace NebulaModel.Networking
{
    public static class BinaryUtils
    {
        const int BUFFER_SIZE = 8192;

        public class Writer : IWriterProvider
        {
            readonly MemoryStream ms;
            readonly LZ4EncoderStream ls;
            readonly BufferedStream bs;
            readonly BinaryWriter bw;

            public BinaryWriter BinaryWriter => bw;

            public Writer()
            {
                ms = new MemoryStream();
                ls = LZ4Stream.Encode(ms);
                bs = new BufferedStream(ls, BUFFER_SIZE);
                bw = new BinaryWriter(bs);
            }

            public void Dispose()
            {
                bw?.Close();
                bs?.Dispose();
                ls?.Dispose();
                ms?.Dispose();
                GC.SuppressFinalize(this);
            }

            public byte[] CloseAndGetBytes()
            {
                bw?.Close();
                return ms?.ToArray() ?? Array.Empty<byte>();
            }
        }

        public class Reader : IReaderProvider
        {
            readonly MemoryStream ms;
            readonly LZ4DecoderStream ls;
            readonly BufferedStream bs;
            readonly BinaryReader br;

            public MemoryStream MemoryStream => ms;
            public BinaryReader BinaryReader => br;

            public Reader(byte[] bytes)
            {
                ms = new MemoryStream(bytes);
                ls = LZ4Stream.Decode(ms);
                bs = new BufferedStream(ls, BUFFER_SIZE);
                br = new BinaryReader(bs);
            }

            public void Dispose()
            {
                br?.Close();
                bs?.Dispose();
                ls?.Dispose();
                ms?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}

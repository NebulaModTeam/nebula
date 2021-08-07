using K4os.Compression.LZ4.Streams;
using System;
using System.IO;

namespace NebulaModel.Networking
{
    public static class BinaryUtils
    {
        const int BUFFER_SIZE = 8192;

        public class Writer : IDisposable
        {
            MemoryStream ms;
            LZ4EncoderStream ls;
            BufferedStream bs;
            BinaryWriter bw;

            public BinaryWriter BinaryWriter => bw;

            public Writer()
            {
                ms = new MemoryStream();
                //ls = LZ4Stream.Encode(ms);
                bs = new BufferedStream(ms, BUFFER_SIZE);
                bw = new BinaryWriter(bs);
            }

            public void Dispose()
            {
                bw?.Close();
                bs?.Dispose();
                //ls?.Dispose();
                ms?.Dispose();
            }

            public byte[] CloseAndGetBytes()
            {
                bw?.Close();
                return ms?.ToArray() ?? new byte[0];
            }
        }

        public class Reader : IDisposable
        {
            MemoryStream ms;
            //LZ4DecoderStream ls;
            BufferedStream bs;
            BinaryReader br;

            public MemoryStream MemoryStream => ms;
            public BinaryReader BinaryReader => br;

            public Reader(byte[] bytes)
            {
                ms = new MemoryStream(bytes);
                //ls = LZ4Stream.Decode(ms);
                bs = new BufferedStream(ms, BUFFER_SIZE);
                br = new BinaryReader(bs);
            }

            public void Dispose()
            {
                br?.Close();
                bs?.Dispose();
                //ls?.Dispose();
                ms?.Dispose();
            }
        }
    }
}

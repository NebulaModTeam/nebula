#region

using System;
using System.Net;
using System.Text;
using NebulaAPI.Interfaces;

#endregion

namespace NebulaModel.Networking.Serialization;

public class NetDataReader : INetDataReader
{
    public NetDataReader()
    {
    }

    public NetDataReader(NetDataWriter writer)
    {
        SetSource(writer);
    }

    public NetDataReader(byte[] source)
    {
        SetSource(source);
    }

    public NetDataReader(byte[] source, int offset)
    {
        SetSource(source, offset);
    }

    public NetDataReader(byte[] source, int offset, int maxSize)
    {
        SetSource(source, offset, maxSize);
    }

    private byte[] RawData { get; set; }

    private int RawDataSize { get; set; }

    private int UserDataOffset { get; set; }

    public int UserDataSize => RawDataSize - UserDataOffset;

    public bool IsNull => RawData == null;

    private int Position { get; set; }

    public bool EndOfData => Position == RawDataSize;

    public int AvailableBytes => RawDataSize - Position;

    public void SkipBytes(int count)
    {
        Position += count;
    }

    private void SetSource(NetDataWriter dataWriter)
    {
        RawData = dataWriter.Data;
        Position = 0;
        UserDataOffset = 0;
        RawDataSize = dataWriter.Length;
    }

    private void SetSource(byte[] source)
    {
        RawData = source;
        Position = 0;
        UserDataOffset = 0;
        RawDataSize = source.Length;
    }

    private void SetSource(byte[] source, int offset)
    {
        RawData = source;
        Position = offset;
        UserDataOffset = offset;
        RawDataSize = source.Length;
    }

    private void SetSource(byte[] source, int offset, int maxSize)
    {
        RawData = source;
        Position = offset;
        UserDataOffset = offset;
        RawDataSize = maxSize;
    }

    public void Clear()
    {
        Position = 0;
        RawDataSize = 0;
        RawData = null;
    }

    #region GetMethods

    public IPEndPoint GetNetEndPoint()
    {
        var host = GetString(1000);
        var port = GetInt();
        return NetUtils.MakeEndPoint(host, port);
    }

    public byte GetByte()
    {
        var res = RawData[Position];
        Position += 1;
        return res;
    }

    public sbyte GetSByte()
    {
        var b = (sbyte)RawData[Position];
        Position++;
        return b;
    }

    public bool[] GetBoolArray()
    {
        var size = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        var arr = new bool[size];
        Buffer.BlockCopy(RawData, Position, arr, 0, size);
        Position += size;
        return arr;
    }

    public ushort[] GetUShortArray()
    {
        var size = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        var arr = new ushort[size];
        Buffer.BlockCopy(RawData, Position, arr, 0, size * 2);
        Position += size * 2;
        return arr;
    }

    public short[] GetShortArray()
    {
        var size = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        var arr = new short[size];
        Buffer.BlockCopy(RawData, Position, arr, 0, size * 2);
        Position += size * 2;
        return arr;
    }

    public long[] GetLongArray()
    {
        var size = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        var arr = new long[size];
        Buffer.BlockCopy(RawData, Position, arr, 0, size * 8);
        Position += size * 8;
        return arr;
    }

    public ulong[] GetULongArray()
    {
        var size = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        var arr = new ulong[size];
        Buffer.BlockCopy(RawData, Position, arr, 0, size * 8);
        Position += size * 8;
        return arr;
    }

    public int[] GetIntArray()
    {
        var size = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        var arr = new int[size];
        Buffer.BlockCopy(RawData, Position, arr, 0, size * 4);
        Position += size * 4;
        return arr;
    }

    public uint[] GetUIntArray()
    {
        var size = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        var arr = new uint[size];
        Buffer.BlockCopy(RawData, Position, arr, 0, size * 4);
        Position += size * 4;
        return arr;
    }

    public float[] GetFloatArray()
    {
        var size = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        var arr = new float[size];
        Buffer.BlockCopy(RawData, Position, arr, 0, size * 4);
        Position += size * 4;
        return arr;
    }

    public double[] GetDoubleArray()
    {
        var size = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        var arr = new double[size];
        Buffer.BlockCopy(RawData, Position, arr, 0, size * 8);
        Position += size * 8;
        return arr;
    }

    public string[] GetStringArray()
    {
        var size = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        var arr = new string[size];
        for (var i = 0; i < size; i++)
        {
            arr[i] = GetString();
        }
        return arr;
    }

    public string[] GetStringArray(int maxStringLength)
    {
        var size = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        var arr = new string[size];
        for (var i = 0; i < size; i++)
        {
            arr[i] = GetString(maxStringLength);
        }
        return arr;
    }

    public bool GetBool()
    {
        var res = RawData[Position] > 0;
        Position += 1;
        return res;
    }

    public char GetChar()
    {
        var result = BitConverter.ToChar(RawData, Position);
        Position += 2;
        return result;
    }

    public ushort GetUShort()
    {
        var result = BitConverter.ToUInt16(RawData, Position);
        Position += 2;
        return result;
    }

    public short GetShort()
    {
        var result = BitConverter.ToInt16(RawData, Position);
        Position += 2;
        return result;
    }

    public long GetLong()
    {
        var result = BitConverter.ToInt64(RawData, Position);
        Position += 8;
        return result;
    }

    public ulong GetULong()
    {
        var result = BitConverter.ToUInt64(RawData, Position);
        Position += 8;
        return result;
    }

    public int GetInt()
    {
        var result = BitConverter.ToInt32(RawData, Position);
        Position += 4;
        return result;
    }

    public uint GetUInt()
    {
        var result = BitConverter.ToUInt32(RawData, Position);
        Position += 4;
        return result;
    }

    public float GetFloat()
    {
        var result = BitConverter.ToSingle(RawData, Position);
        Position += 4;
        return result;
    }

    public double GetDouble()
    {
        var result = BitConverter.ToDouble(RawData, Position);
        Position += 8;
        return result;
    }

    public string GetString(int maxLength)
    {
        var bytesCount = GetInt();
        if (bytesCount <= 0 || bytesCount > maxLength * 2)
        {
            return string.Empty;
        }

        var charCount = Encoding.UTF8.GetCharCount(RawData, Position, bytesCount);
        if (charCount > maxLength)
        {
            return string.Empty;
        }

        var result = Encoding.UTF8.GetString(RawData, Position, bytesCount);
        Position += bytesCount;
        return result;
    }

    public string GetString()
    {
        var bytesCount = GetInt();
        if (bytesCount <= 0)
        {
            return string.Empty;
        }

        var result = Encoding.UTF8.GetString(RawData, Position, bytesCount);
        Position += bytesCount;
        return result;
    }

    public ArraySegment<byte> GetRemainingBytesSegment()
    {
        var segment = new ArraySegment<byte>(RawData, Position, AvailableBytes);
        Position = RawData.Length;
        return segment;
    }

    public T Get<T>() where T : INetSerializable, new()
    {
        var obj = new T();
        obj.Deserialize(this);
        return obj;
    }

    public byte[] GetRemainingBytes()
    {
        var outgoingData = new byte[AvailableBytes];
        Buffer.BlockCopy(RawData, Position, outgoingData, 0, AvailableBytes);
        Position = RawData.Length;
        return outgoingData;
    }

    public void GetBytes(byte[] destination, int start, int count)
    {
        Buffer.BlockCopy(RawData, Position, destination, start, count);
        Position += count;
    }

    public void GetBytes(byte[] destination, int count)
    {
        Buffer.BlockCopy(RawData, Position, destination, 0, count);
        Position += count;
    }

    public sbyte[] GetSBytesWithLength()
    {
        var length = GetInt();
        var outgoingData = new sbyte[length];
        Buffer.BlockCopy(RawData, Position, outgoingData, 0, length);
        Position += length;
        return outgoingData;
    }

    public byte[] GetBytesWithLength()
    {
        var length = GetInt();
        var outgoingData = new byte[length];
        Buffer.BlockCopy(RawData, Position, outgoingData, 0, length);
        Position += length;
        return outgoingData;
    }

    #endregion

    #region PeekMethods

    public byte PeekByte()
    {
        return RawData[Position];
    }

    public sbyte PeekSByte()
    {
        return (sbyte)RawData[Position];
    }

    public bool PeekBool()
    {
        return RawData[Position] > 0;
    }

    public char PeekChar()
    {
        return BitConverter.ToChar(RawData, Position);
    }

    public ushort PeekUShort()
    {
        return BitConverter.ToUInt16(RawData, Position);
    }

    public short PeekShort()
    {
        return BitConverter.ToInt16(RawData, Position);
    }

    public long PeekLong()
    {
        return BitConverter.ToInt64(RawData, Position);
    }

    public ulong PeekULong()
    {
        return BitConverter.ToUInt64(RawData, Position);
    }

    private int PeekInt()
    {
        return BitConverter.ToInt32(RawData, Position);
    }

    public uint PeekUInt()
    {
        return BitConverter.ToUInt32(RawData, Position);
    }

    public float PeekFloat()
    {
        return BitConverter.ToSingle(RawData, Position);
    }

    public double PeekDouble()
    {
        return BitConverter.ToDouble(RawData, Position);
    }

    public string PeekString(int maxLength)
    {
        var bytesCount = BitConverter.ToInt32(RawData, Position);
        if (bytesCount <= 0 || bytesCount > maxLength * 2)
        {
            return string.Empty;
        }

        var charCount = Encoding.UTF8.GetCharCount(RawData, Position + 4, bytesCount);
        if (charCount > maxLength)
        {
            return string.Empty;
        }

        var result = Encoding.UTF8.GetString(RawData, Position + 4, bytesCount);
        return result;
    }

    public string PeekString()
    {
        var bytesCount = BitConverter.ToInt32(RawData, Position);
        if (bytesCount <= 0)
        {
            return string.Empty;
        }

        var result = Encoding.UTF8.GetString(RawData, Position + 4, bytesCount);
        return result;
    }

    #endregion

    #region TryGetMethods

    public bool TryGetByte(out byte result)
    {
        if (AvailableBytes >= 1)
        {
            result = GetByte();
            return true;
        }
        result = 0;
        return false;
    }

    public bool TryGetSByte(out sbyte result)
    {
        if (AvailableBytes >= 1)
        {
            result = GetSByte();
            return true;
        }
        result = 0;
        return false;
    }

    public bool TryGetBool(out bool result)
    {
        if (AvailableBytes >= 1)
        {
            result = GetBool();
            return true;
        }
        result = false;
        return false;
    }

    public bool TryGetChar(out char result)
    {
        if (AvailableBytes >= 2)
        {
            result = GetChar();
            return true;
        }
        result = '\0';
        return false;
    }

    public bool TryGetShort(out short result)
    {
        if (AvailableBytes >= 2)
        {
            result = GetShort();
            return true;
        }
        result = 0;
        return false;
    }

    private bool TryGetUShort(out ushort result)
    {
        if (AvailableBytes >= 2)
        {
            result = GetUShort();
            return true;
        }
        result = 0;
        return false;
    }

    public bool TryGetInt(out int result)
    {
        if (AvailableBytes >= 4)
        {
            result = GetInt();
            return true;
        }
        result = 0;
        return false;
    }

    public bool TryGetUInt(out uint result)
    {
        if (AvailableBytes >= 4)
        {
            result = GetUInt();
            return true;
        }
        result = 0;
        return false;
    }

    public bool TryGetLong(out long result)
    {
        if (AvailableBytes >= 8)
        {
            result = GetLong();
            return true;
        }
        result = 0;
        return false;
    }

    public bool TryGetULong(out ulong result)
    {
        if (AvailableBytes >= 8)
        {
            result = GetULong();
            return true;
        }
        result = 0;
        return false;
    }

    public bool TryGetFloat(out float result)
    {
        if (AvailableBytes >= 4)
        {
            result = GetFloat();
            return true;
        }
        result = 0;
        return false;
    }

    public bool TryGetDouble(out double result)
    {
        if (AvailableBytes >= 8)
        {
            result = GetDouble();
            return true;
        }
        result = 0;
        return false;
    }

    private bool TryGetString(out string result)
    {
        if (AvailableBytes >= 4)
        {
            var bytesCount = PeekInt();
            if (AvailableBytes >= bytesCount + 4)
            {
                result = GetString();
                return true;
            }
        }
        result = null;
        return false;
    }

    public bool TryGetStringArray(out string[] result)
    {
        if (!TryGetUShort(out var size))
        {
            result = null;
            return false;
        }

        result = new string[size];
        for (var i = 0; i < size; i++)
        {
            if (TryGetString(out result[i]))
            {
                continue;
            }
            result = null;
            return false;
        }

        return true;
    }

    public bool TryGetBytesWithLength(out byte[] result)
    {
        if (AvailableBytes >= 4)
        {
            var length = PeekInt();
            if (length >= 0 && AvailableBytes >= length + 4)
            {
                result = GetBytesWithLength();
                return true;
            }
        }
        result = null;
        return false;
    }

    #endregion
}

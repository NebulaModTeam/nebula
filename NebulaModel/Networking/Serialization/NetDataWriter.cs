#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NebulaAPI.Interfaces;

#endregion

namespace NebulaModel.Networking.Serialization;

public class NetDataWriter : INetDataWriter
{
    private const int InitialSize = 64;
    private readonly bool _autoResize;
    private byte[] _data;

    public NetDataWriter() : this(true)
    {
    }

    private NetDataWriter(bool autoResize, int initialSize = InitialSize)
    {
        _data = new byte[initialSize];
        _autoResize = autoResize;
    }

    public int Capacity => _data.Length;

    public byte[] Data => _data;

    public int Length { get; set; }

    public void Put(float value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 4);
        }

        FastBitConverter.GetBytes(_data, Length, value);
        Length += 4;
    }

    public void Put(double value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 8);
        }

        FastBitConverter.GetBytes(_data, Length, value);
        Length += 8;
    }

    public void Put(long value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 8);
        }

        FastBitConverter.GetBytes(_data, Length, value);
        Length += 8;
    }

    public void Put(ulong value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 8);
        }

        FastBitConverter.GetBytes(_data, Length, value);
        Length += 8;
    }

    public void Put(int value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 4);
        }

        FastBitConverter.GetBytes(_data, Length, value);
        Length += 4;
    }

    public void Put(uint value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 4);
        }

        FastBitConverter.GetBytes(_data, Length, value);
        Length += 4;
    }

    public void Put(char value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 2);
        }

        FastBitConverter.GetBytes(_data as IEnumerable<byte>, Length, value);
        Length += 2;
    }

    public void Put(ushort value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 2);
        }

        FastBitConverter.GetBytes(_data as IEnumerable<byte>, Length, value);
        Length += 2;
    }

    public void Put(short value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 2);
        }

        FastBitConverter.GetBytes(_data as IEnumerable<byte>, Length, value);
        Length += 2;
    }

    public void Put(sbyte value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 1);
        }

        _data[Length] = (byte)value;
        Length++;
    }

    public void Put(byte value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 1);
        }

        _data[Length] = value;
        Length++;
    }

    public void Put(byte[] data, int offset, int length)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + length);
        }

        Buffer.BlockCopy(data, offset, _data, Length, length);
        Length += length;
    }

    public void Put(byte[] data)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + data.Length);
        }

        Buffer.BlockCopy(data, 0, _data, Length, data.Length);
        Length += data.Length;
    }

    public void PutSBytesWithLength(sbyte[] data, int offset, int length)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + length + 4);
        }

        FastBitConverter.GetBytes(_data, Length, length);
        Buffer.BlockCopy(data, offset, _data, Length + 4, length);
        Length += length + 4;
    }

    public void PutSBytesWithLength(sbyte[] data)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + data.Length + 4);
        }

        FastBitConverter.GetBytes(_data, Length, data.Length);
        Buffer.BlockCopy(data, 0, _data, Length + 4, data.Length);
        Length += data.Length + 4;
    }

    public void PutBytesWithLength(byte[] data, int offset, int length)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + length + 4);
        }

        FastBitConverter.GetBytes(_data, Length, length);
        Buffer.BlockCopy(data, offset, _data, Length + 4, length);
        Length += length + 4;
    }

    public void PutBytesWithLength(byte[] data)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + data.Length + 4);
        }

        FastBitConverter.GetBytes(_data, Length, data.Length);
        Buffer.BlockCopy(data, 0, _data, Length + 4, data.Length);
        Length += data.Length + 4;
    }

    public void Put(bool value)
    {
        if (_autoResize)
        {
            ResizeIfNeed(Length + 1);
        }

        _data[Length] = (byte)(value ? 1 : 0);
        Length++;
    }

    public void PutArray(float[] value)
    {
        PutArray(value, 4);
    }

    public void PutArray(double[] value)
    {
        PutArray(value, 8);
    }

    public void PutArray(long[] value)
    {
        PutArray(value, 8);
    }

    public void PutArray(ulong[] value)
    {
        PutArray(value, 8);
    }

    public void PutArray(int[] value)
    {
        PutArray(value, 4);
    }

    public void PutArray(uint[] value)
    {
        PutArray(value, 4);
    }

    public void PutArray(ushort[] value)
    {
        PutArray(value, 2);
    }

    public void PutArray(short[] value)
    {
        PutArray(value, 2);
    }

    public void PutArray(bool[] value)
    {
        PutArray(value, 1);
    }

    public void PutArray(string[] value)
    {
        var len = value == null ? (ushort)0 : (ushort)value.Length;
        Put(len);
        for (var i = 0; i < len; i++)
        {
            if (value != null)
            {
                Put(value[i]);
            }
        }
    }

    public void PutArray(string[] value, int maxLength)
    {
        var len = value == null ? (ushort)0 : (ushort)value.Length;
        Put(len);
        for (var i = 0; i < len; i++)
        {
            if (value != null)
            {
                Put(value[i], maxLength);
            }
        }
    }

    public void Put(IPEndPoint endPoint)
    {
        Put(endPoint.Address.ToString());
        Put(endPoint.Port);
    }

    public void Put(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Put(0);
            return;
        }

        //put bytes count
        var bytesCount = Encoding.UTF8.GetByteCount(value);
        if (_autoResize)
        {
            ResizeIfNeed(Length + bytesCount + 4);
        }

        Put(bytesCount);

        //put string
        Encoding.UTF8.GetBytes(value, 0, value.Length, _data, Length);
        Length += bytesCount;
    }

    public void Put(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            Put(0);
            return;
        }

        var length = value.Length > maxLength ? maxLength : value.Length;
        //calculate max count
        var bytesCount = Encoding.UTF8.GetByteCount(value);
        if (_autoResize)
        {
            ResizeIfNeed(Length + bytesCount + 4);
        }

        //put bytes count
        Put(bytesCount);

        //put string
        Encoding.UTF8.GetBytes(value, 0, length, _data, Length);

        Length += bytesCount;
    }

    public void Put<T>(T obj) where T : INetSerializable
    {
        obj.Serialize(this);
    }

    /// <summary>
    ///     Creates NetDataWriter from existing ByteArray
    /// </summary>
    /// <param name="bytes">Source byte array</param>
    /// <param name="copy">Copy array to new location or use existing</param>
    public static NetDataWriter FromBytes(byte[] bytes, bool copy)
    {
        if (!copy)
        {
            return new NetDataWriter(true, 0) { _data = bytes, Length = bytes.Length };
        }
        var netDataWriter = new NetDataWriter(true, bytes.Length);
        netDataWriter.Put(bytes);
        return netDataWriter;
    }

    /// <summary>
    ///     Creates NetDataWriter from existing ByteArray (always copied data)
    /// </summary>
    /// <param name="bytes">Source byte array</param>
    /// <param name="offset">Offset of array</param>
    /// <param name="length">Length of array</param>
    public static NetDataWriter FromBytes(byte[] bytes, int offset, int length)
    {
        var netDataWriter = new NetDataWriter(true, bytes.Length);
        netDataWriter.Put(bytes, offset, length);
        return netDataWriter;
    }

    public static NetDataWriter FromString(string value)
    {
        var netDataWriter = new NetDataWriter();
        netDataWriter.Put(value);
        return netDataWriter;
    }

    private void ResizeIfNeed(int newSize)
    {
        var len = _data.Length;
        if (len >= newSize)
        {
            return;
        }
        while (len < newSize)
        {
            len *= 2;
        }

        Array.Resize(ref _data, len);
    }

    public void Reset(int size)
    {
        ResizeIfNeed(size);
        Length = 0;
    }

    public void Reset()
    {
        Length = 0;
    }

    public byte[] CopyData()
    {
        var resultData = new byte[Length];
        Buffer.BlockCopy(_data, 0, resultData, 0, Length);
        return resultData;
    }

    /// <summary>
    ///     Sets position of NetDataWriter to rewrite previous values
    /// </summary>
    /// <param name="position">new byte position</param>
    /// <returns>previous position of data writer</returns>
    public int SetPosition(int position)
    {
        var prevPosition = Length;
        Length = position;
        return prevPosition;
    }

    private void PutArray(Array arr, int sz)
    {
        var length = arr == null ? (ushort)0 : (ushort)arr.Length;
        sz *= length;
        if (_autoResize)
        {
            ResizeIfNeed(Length + sz + 2);
        }

        FastBitConverter.GetBytes(_data as IEnumerable<byte>, Length, length);
        if (arr != null)
        {
            Buffer.BlockCopy(arr, 0, _data, Length + 2, sz);
        }

        Length += sz + 2;
    }
}

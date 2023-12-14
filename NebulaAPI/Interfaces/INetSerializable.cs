#region

using System;
using System.Net;

#endregion

namespace NebulaAPI;

public interface INetSerializable
{
    void Serialize(INetDataWriter writer);

    void Deserialize(INetDataReader reader);
}

public interface INetDataWriter
{
    void Put(float value);

    void Put(double value);

    void Put(long value);

    void Put(ulong value);

    void Put(int value);

    void Put(uint value);

    void Put(char value);

    void Put(ushort value);

    void Put(short value);

    void Put(sbyte value);

    void Put(byte value);

    void Put(byte[] data, int offset, int length);

    void Put(byte[] data);

    void PutSBytesWithLength(sbyte[] data, int offset, int length);

    void PutSBytesWithLength(sbyte[] data);

    void PutBytesWithLength(byte[] data, int offset, int length);

    void PutBytesWithLength(byte[] data);

    void Put(bool value);

    void PutArray(float[] value);

    void PutArray(double[] value);

    void PutArray(long[] value);

    void PutArray(ulong[] value);

    void PutArray(int[] value);

    void PutArray(uint[] value);

    void PutArray(ushort[] value);

    void PutArray(short[] value);

    void PutArray(bool[] value);

    void PutArray(string[] value);

    void PutArray(string[] value, int maxLength);

    void Put(IPEndPoint endPoint);

    void Put(string value);

    void Put(string value, int maxLength);

    void Put<T>(T obj) where T : INetSerializable;
}

public interface INetDataReader
{
    IPEndPoint GetNetEndPoint();

    byte GetByte();

    sbyte GetSByte();

    bool[] GetBoolArray();

    ushort[] GetUShortArray();

    short[] GetShortArray();

    long[] GetLongArray();

    ulong[] GetULongArray();

    int[] GetIntArray();

    uint[] GetUIntArray();

    float[] GetFloatArray();

    double[] GetDoubleArray();

    string[] GetStringArray();

    string[] GetStringArray(int maxStringLength);

    bool GetBool();

    char GetChar();

    ushort GetUShort();

    short GetShort();

    long GetLong();

    ulong GetULong();

    int GetInt();

    uint GetUInt();

    float GetFloat();

    double GetDouble();

    string GetString(int maxLength);

    string GetString();

    ArraySegment<byte> GetRemainingBytesSegment();

    T Get<T>() where T : INetSerializable, new();

    byte[] GetRemainingBytes();

    void GetBytes(byte[] destination, int start, int count);

    void GetBytes(byte[] destination, int count);

    sbyte[] GetSBytesWithLength();

    byte[] GetBytesWithLength();
}

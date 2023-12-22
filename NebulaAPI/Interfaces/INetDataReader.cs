// #pragma once
// #ifndef INetDataReader.cs_H_
// #define INetDataReader.cs_H_
// 
// #endif

using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace NebulaAPI.Interfaces;

public interface INetDataReader
{
    byte[] RawData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    int RawDataSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    int UserDataOffset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    int UserDataSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    bool IsNull
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    int Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    bool EndOfData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    int AvailableBytes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    void SkipBytes(int count);
    void SetPosition(int position);
    void SetSource(INetDataWriter dataWriter);
    void SetSource(byte[] source);
    void SetSource(byte[] source, int offset, int maxSize);
    IPEndPoint GetNetEndPoint();
    byte GetByte();
    sbyte GetSByte();
    T[] GetArray<T>(int size);
    bool[] GetBoolArray();
    ushort[] GetUShortArray();
    short[] GetShortArray();
    int[] GetIntArray();
    uint[] GetUIntArray();
    float[] GetFloatArray();
    double[] GetDoubleArray();
    long[] GetLongArray();
    ulong[] GetULongArray();
    string[] GetStringArray();

    /// <summary>
    /// Note that "maxStringLength" only limits the number of characters in a string, not its size in bytes.
    /// Strings that exceed this parameter are returned as empty
    /// </summary>
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

    /// <summary>
    /// Note that "maxLength" only limits the number of characters in a string, not its size in bytes.
    /// </summary>
    /// <returns>"string.Empty" if value > "maxLength"</returns>
    string GetString(int maxLength);

    string GetString();
    ArraySegment<byte> GetBytesSegment(int count);
    ArraySegment<byte> GetRemainingBytesSegment();
    T Get<T>() where T : struct, INetSerializable;
    T Get<T>(Func<T> constructor) where T : class, INetSerializable;
    byte[] GetRemainingBytes();
    void GetBytes(byte[] destination, int start, int count);
    void GetBytes(byte[] destination, int count);
    sbyte[] GetSBytesWithLength();
    byte[] GetBytesWithLength();
    byte PeekByte();
    sbyte PeekSByte();
    bool PeekBool();
    char PeekChar();
    ushort PeekUShort();
    short PeekShort();
    long PeekLong();
    ulong PeekULong();
    int PeekInt();
    uint PeekUInt();
    float PeekFloat();
    double PeekDouble();

    /// <summary>
    /// Note that "maxLength" only limits the number of characters in a string, not its size in bytes.
    /// </summary>
    string PeekString(int maxLength);

    string PeekString();
    bool TryGetByte(out byte result);
    bool TryGetSByte(out sbyte result);
    bool TryGetBool(out bool result);
    bool TryGetChar(out char result);
    bool TryGetShort(out short result);
    bool TryGetUShort(out ushort result);
    bool TryGetInt(out int result);
    bool TryGetUInt(out uint result);
    bool TryGetLong(out long result);
    bool TryGetULong(out ulong result);
    bool TryGetFloat(out float result);
    bool TryGetDouble(out double result);
    bool TryGetString(out string result);
    bool TryGetStringArray(out string[] result);
    bool TryGetBytesWithLength(out byte[] result);
    void Clear();
}

// #pragma once
// #ifndef INetDataWriter.cs_H_
// #define INetDataWriter.cs_H_
// 
// #endif

using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace NebulaAPI.Interfaces;

public interface INetDataWriter
{
    int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    byte[] Data
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    void ResizeIfNeed(int newSize);
    void EnsureFit(int additionalSize);
    void Reset(int size);
    void Reset();
    byte[] CopyData();

    /// <summary>
    /// Sets position of NetDataWriter to rewrite previous values
    /// </summary>
    /// <param name="position">new byte position</param>
    /// <returns>previous position of data writer</returns>
    int SetPosition(int position);

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
    void Put(bool value);
    void Put(IPEndPoint endPoint);
    void Put(string value);

    /// <summary>
    /// Note that "maxLength" only limits the number of characters in a string, not its size in bytes.
    /// </summary>
    void Put(string value, int maxLength);

    void Put<T>(T obj) where T : INetSerializable;
    void PutSBytesWithLength(sbyte[] data, int offset, int length);
    void PutSBytesWithLength(sbyte[] data);
    void PutBytesWithLength(byte[] data, int offset, int length);
    void PutBytesWithLength(byte[] data);
    void PutArray(Array arr, int sz);
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
    void PutArray(string[] value, int strMaxLength);
}

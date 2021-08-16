// unset

using System;
using System.IO;

namespace NebulaAPI.Network
{
    public interface IWriterProvider : IDisposable
    { 
        BinaryWriter BinaryWriter { get; }
        byte[] CloseAndGetBytes();
    }
    
    public interface IReaderProvider : IDisposable
    { 
        BinaryReader BinaryReader { get; }
    }
}
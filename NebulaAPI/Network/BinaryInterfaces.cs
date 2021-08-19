using System;
using System.IO;

namespace NebulaAPI
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
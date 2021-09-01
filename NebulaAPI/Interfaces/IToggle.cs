using System;

namespace NebulaAPI
{
    public interface IToggle
    {
        bool Value { get; }
        IDisposable On();
    }
}
#region

using System;

#endregion

namespace NebulaAPI.Interfaces;

public interface IToggle
{
    bool Value { get; }

    IDisposable On();
}

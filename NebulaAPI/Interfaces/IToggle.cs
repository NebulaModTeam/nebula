#region

using System;

#endregion

namespace NebulaAPI;

public interface IToggle
{
    bool Value { get; }

    IDisposable On();
}

#region

using System;
using NebulaModel.DataStructures;

#endregion

namespace NebulaWorld.Combat;

public class CombatManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();

    public static bool LockBuildHp { get; private set; }

    public CombatManager()
    {
        LockBuildHp = true;
    }

    public void Dispose()
    {
        LockBuildHp = false;
        GC.SuppressFinalize(this);
    }
}

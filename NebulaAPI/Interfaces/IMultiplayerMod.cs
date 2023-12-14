#region

using System.IO;

#endregion

namespace NebulaAPI.Interfaces;

/// <summary>
///     Implement this interface to make sure your mod will be the same version on the host
/// </summary>
public interface IMultiplayerMod
{
    string Version { get; }

    bool CheckVersion(string hostVersion, string clientVersion);
}

/// <summary>
///     Implement this interface if you have important settings that clients need to know
/// </summary>
public interface IMultiplayerModWithSettings : IMultiplayerMod
{
    void Export(BinaryWriter w);

    void Import(BinaryReader r);
}

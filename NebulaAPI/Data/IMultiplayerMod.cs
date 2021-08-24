using System.IO;

namespace NebulaAPI
{
    /// <summary>
    /// Implement this interface to make sure your mod will be the same version on the host
    /// </summary>
    public interface IMultiplayerMod
    {
        string Version { get; }
    }
    
    /// <summary>
    /// Implement this interface if you have important settings that clients need to know
    /// </summary>
    public interface IMultiplayerModWithSettings : IMultiplayerMod
    {
        void Export (BinaryWriter w);
        void Import (BinaryReader r);
    }
}

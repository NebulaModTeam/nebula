// unset

using System.IO;

namespace NebulaAPI
{
    public interface IMultiplayerMod
    {
        string Verson { get; }
        bool CheckVersion { get; }
    }
    
    public interface IMultiplayerModWithSettings : IMultiplayerMod
    {
        void Export (BinaryWriter w);
        void Import (BinaryReader r);
    }
}
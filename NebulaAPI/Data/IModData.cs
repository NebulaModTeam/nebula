using System.IO;

namespace NebulaAPI
{
    /// <summary>
    /// Use this class to sync your custom PlanetFactory Data
    /// </summary>
    public interface IModData<in T>
    {
        void Export (T inst, BinaryWriter w);
        void Import (T inst, BinaryReader r);
    }
}
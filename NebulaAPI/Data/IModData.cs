// unset

using System.IO;

namespace NebulaAPI
{
    public interface IModData<in T>
    {
        void Export (T inst, BinaryWriter w);
        void Import (T inst, BinaryReader r);
    }
}
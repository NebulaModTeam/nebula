#region

using System;
using System.Net;

#endregion

namespace NebulaAPI.Interfaces;

public interface INetSerializable
{
    void Serialize(INetDataWriter writer);

    void Deserialize(INetDataReader reader);
}

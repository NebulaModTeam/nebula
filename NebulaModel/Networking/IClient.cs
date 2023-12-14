#region

using System.Net;

#endregion

namespace NebulaModel.Networking;

public interface IClient
{
    IPEndPoint ServerEndpoint { get; }
}

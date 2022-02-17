using System.Net;

namespace NebulaModel.Networking
{
    public interface IClient
    {
        IPEndPoint ServerEndpoint { get; }
    }
}
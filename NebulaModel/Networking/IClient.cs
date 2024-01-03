#region

using System.Net;
using NebulaAPI.GameState;

#endregion

namespace NebulaModel.Networking;

public interface IClient : INetworkProvider
{
    IPEndPoint ServerEndpoint { get; set; }

    public void Update();

    public void Start();

    public void Stop();
}

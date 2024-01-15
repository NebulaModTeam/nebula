using System.Diagnostics;
using System.Net;
using System.Reflection;
using WebSocketSharp;
using WebSocketSharp.Server;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;
using WebSocket = WebSocketSharp.WebSocket;

namespace NebulaTests.Functional.NebulaNetworking;

internal class TestWebSocketService : WebSocketBehavior
{
    public const string Path = "/socket";
    public WebSocket ClientConnection { get; private set; }

    public int TotalReceivedBytes { get; private set; }

    public TaskCompletionSource<bool> ConnectionReady = new();
    protected override void OnError(ErrorEventArgs e) => throw e.Exception;

    protected override void OnOpen()
    {
        ClientConnection = Context.WebSocket;

        Console.WriteLine($"Server accepted a new client connection on {Context.UserEndPoint}");
        ConnectionReady.SetResult(true);
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        TotalReceivedBytes += e.RawData.Length;
    }
}

internal class WebSocketMock
{
    public long TotalClientReceivedBytes { get; private set; } = 0;
    public int TotalClientReceives { get; private set; }

    private WebSocketServer server;
    public TestWebSocketService Service { get; private set; }
    private WebSocket client;

    private int port = BandwidthTestSettings.NextPort;

    public WebSocketMock()
    {
        server = new WebSocketServer(IPAddress.Any, port);
        server.AddWebSocketService<TestWebSocketService>(TestWebSocketService.Path, (s) => Service = s);

        // Disable Nagle's on server
        var listener =
            typeof(WebSocketServer).GetField("_listener", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(server) as
                System.Net.Sockets.TcpListener;
        listener!.Server.NoDelay = true;


        client = new WebSocket($"ws://127.0.0.1:{port}{TestWebSocketService.Path}");


        client.OnMessage += (sender, args) =>
        {
            TotalClientReceivedBytes += args.RawData.Length;
            TotalClientReceives++;
        };
        client.OnError += (sender, args) => throw args.Exception;
    }

    ~WebSocketMock()
    {
        client.Close();
        server.Stop();
    }

    internal async Task StartConnections()
    {
        if (server.IsListening && client.IsAlive)
            return;

        server.Start();


        var clientConnectedCts = new TaskCompletionSource<bool>();


        client.OnOpen += (sender, args) =>
        {
            var tcpClient =
                typeof(WebSocket).GetField("_tcpClient", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(client) as
                    System.Net.Sockets.TcpClient;
            tcpClient!.NoDelay = true;
            Console.WriteLine("Client successfully connected to server.");
            clientConnectedCts.SetResult(true);
        };

        client.ConnectAsync();

        await clientConnectedCts.Task;
        await Service.ConnectionReady.Task;
    }
}

// dotnet test --filter TestCategory!=Bandwidth to exclude these tests.
// We could add bandwidth targets to the parameters but websock's bandwidth is very low for small sends & we're unsure of our requirements.
[TestCategory("Bandwidth")]
[TestCategory("Functional")]
[TestClass]
public class WebSocketsBandwidthTest
{
    /// <summary>
    /// Saturates the server -> client channel with messages of a specific size for a specific duration.
    /// </summary>
    /// <param name="packetSize">Packet size in bytes</param>
    /// <param name="testDuration">Test duration in ms</param>
    private async Task ChallengeServerSendOverDuration(int packetSize,
        int testDuration = BandwidthTestSettings.BandwidthChallengeDurationMs)
    {
        var mock = new WebSocketMock();
        await mock.StartConnections();

        var stopWatch = new Stopwatch();
        var message = new byte[packetSize];

        stopWatch.Start();

        bool isReadyToSend = true;
        int totalSends = 0;
        do
        {
            if (!isReadyToSend)
            {
                continue;
            }

            isReadyToSend = false;
            mock.Service.ClientConnection.SendAsync(message, (success) =>
            {
                isReadyToSend = success;
                totalSends++;
            });
        } while (stopWatch.ElapsedMilliseconds < testDuration);

        while (mock.TotalClientReceives < totalSends)
        {
            // Wait for the buffer to fully flush on the client side
            await Task.Delay(1);
        }

        stopWatch.Stop();

        var bytesPerSecond = (double)mock.TotalClientReceivedBytes / stopWatch.ElapsedMilliseconds * 1000;
        Console.WriteLine($"Ran throughput test for packet size: {packetSize} bytes");
        Console.WriteLine($"-- Server sent {BandwidthFormatter.FormatString(mock.TotalClientReceivedBytes, false)} " +
                          $"over {stopWatch.Elapsed.Seconds}.{stopWatch.Elapsed.Milliseconds} seconds.");
        Console.WriteLine($"---- Bandwidth per channel: {BandwidthFormatter.FormatString(bytesPerSecond)} " +
                          $"| {BandwidthFormatter.FormatString(bytesPerSecond, false)}/s");
        Console.WriteLine($"---- Send rate per channel: {(double)totalSends / stopWatch.ElapsedMilliseconds * 1000} Messages/second");

        var bandwidth = BandwidthFormatter.ToBandwidth(bytesPerSecond);
        // Currently just making sure we're sending something - as websock is rather slow for small messages
        // and we're currently unsure of our exact requirements.
        UnitTesting.Assert.IsTrue(bandwidth > new Bandwidth(0, 1));
    }

    [TestCategory("Bandwidth")]
    [TestCategory("Functional")]
    [TestMethod]
    [DataTestMethod]
    [DataRow(16)]
    [DataRow(64)]
    [DataRow(128)]
    [DataRow(256)]
    [DataRow(1024)]
    [DataRow(2048)]
    [DataRow(4096)]
    [DataRow(1024 * 1000 * 5)]
    [DataRow(1024 * 1000 * 10)]
    public async Task ServerSend_VariablePacketSize_BandwidthChallenge(int packetSize)
    {
        await ChallengeServerSendOverDuration(packetSize);
    }
}

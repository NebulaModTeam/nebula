using System.Collections.Generic;
using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using NebulaModel.Utils;
using NebulaWorld;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace NebulaNetwork.Messaging;

public class WebSocketService : WebSocketBehavior
{
    public static IPlayerManager PlayerManager;
    public static NebulaNetPacketProcessor PacketProcessor;
    private static readonly Dictionary<int, NebulaConnection> ConnectionDictionary = new();

    public WebSocketService() { }

    public WebSocketService(IPlayerManager playerManager, NebulaNetPacketProcessor packetProcessor)
    {
        PlayerManager = playerManager;
        PacketProcessor = packetProcessor;
        ConnectionDictionary.Clear();
    }

    protected override void OnOpen()
    {
        if (Multiplayer.Session.IsGameLoaded == false && Multiplayer.Session.IsInLobby == false)
        {
            // Reject any connection that occurs while the host's game is loading.
            Context.WebSocket.Close((ushort)DisconnectionReason.HostStillLoading,
                "Host still loading, please try again later.".Translate());
            return;
        }

        Log.Info($"Client connected ID: {ID}");
        var conn = new NebulaConnection(Context.WebSocket, Context.UserEndPoint, PacketProcessor);
        PlayerManager.PlayerConnected(conn);
        ConnectionDictionary.Add(Context.UserEndPoint.GetHashCode(), conn);
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        // Find created NebulaConnection
        if (ConnectionDictionary.TryGetValue(Context.UserEndPoint.GetHashCode(), out var conn))
        {
            PacketProcessor.EnqueuePacketForProcessing(e.RawData, conn);
        }
        else
        {
            Log.Warn($"Unregister socket {Context.UserEndPoint.GetHashCode()}");
        }
    }

    protected override void OnClose(CloseEventArgs e)
    {
        ConnectionDictionary.Remove(Context.UserEndPoint.GetHashCode());

        // If the reason of a client disconnect is because we are still loading the game,
        // we don't need to inform the other clients since the disconnected client never
        // joined the game in the first place.
        if (e.Code == (short)DisconnectionReason.HostStillLoading)
        {
            return;
        }

        Log.Info($"Client disconnected: {ID}, reason: {e.Reason}");
        UnityDispatchQueue.RunOnMainThread(() =>
        {
            // This is to make sure that we don't try to deal with player disconnection
            // if it is because we have stopped the server and are not in a multiplayer game anymore.
            if (Multiplayer.IsActive)
            {
                PlayerManager.PlayerDisconnected(new NebulaConnection(Context.WebSocket, Context.UserEndPoint,
                    PacketProcessor));
            }
        });
    }

    protected override void OnError(ErrorEventArgs e)
    {
        ConnectionDictionary.Remove(Context.UserEndPoint.GetHashCode());

        // TODO: seems like clients erroring out in the sync process can lock the host with the joining player message, maybe this fixes it
        Log.Info($"Client disconnected because of an error: {ID}, reason: {e.Exception}");
        UnityDispatchQueue.RunOnMainThread(() =>
        {
            // This is to make sure that we don't try to deal with player disconnection
            // if it is because we have stopped the server and are not in a multiplayer game anymore.
            if (Multiplayer.IsActive)
            {
                PlayerManager.PlayerDisconnected(new NebulaConnection(Context.WebSocket, Context.UserEndPoint,
                    PacketProcessor));
            }
        });
    }
}

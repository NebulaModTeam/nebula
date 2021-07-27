using HarmonyLib;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Routers;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace NebulaNetwork
{
    public class MultiplayerClientSession : MonoBehaviour, INetworkProvider
    {
        public static MultiplayerClientSession Instance { get; protected set; }

        private WebSocket clientSocket;
        private EndPoint serverEndpoint;
        private NebulaConnection serverConnection;
        private float mechaSynchonizationTimer = 0f;

        private float pingTimer = 0f;
        private float pingTimestamp = 0f;
        private Text pingIndicator;
        private int previousDelay = 0;

        public NetPacketProcessor PacketProcessor { get; protected set; }
        public bool IsConnected { get; protected set; }

        private const int MECHA_SYNCHONIZATION_INTERVAL = 5;
        private string socketAddress;

        private void Awake()
        {
            Instance = this;
        }

        public void ConnectToIp(IPEndPoint ip)
        {
            serverEndpoint = ip;
            socketAddress = $"ws://{ip}/socket";
            Log.Info($"Connecting to IP...");
            ConnectInternal();
        }

        public void ConnectToUrl(string url, int port)
        {
            IPHostEntry host = Dns.GetHostEntry(url);
            serverEndpoint = new IPEndPoint(host.AddressList[0], port);
            socketAddress = $"ws://{url}:{port}/socket";
            Log.Info($"Connecting to URL...");
            ConnectInternal();
        }

        private void ConnectInternal()
        {
            LocalPlayer.TryLoadGalacticScale2();

            clientSocket = new WebSocket(socketAddress);
            clientSocket.OnOpen += ClientSocket_OnOpen;
            clientSocket.OnClose += ClientSocket_OnClose;
            clientSocket.OnMessage += ClientSocket_OnMessage;

            PacketProcessor = new NetPacketProcessor();
#if DEBUG
            PacketProcessor.SimulateLatency = true;
#endif

            PacketUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor, false);

            clientSocket.Connect();

            SimulatedWorld.Initialize();

            LocalPlayer.IsMasterClient = false;
            LocalPlayer.SetNetworkProvider(this);

            if (Config.Options.RememberLastIP)
            {
                // We've successfully connected, set connection as last ip, cutting out "ws://" and "/socket"
                Config.Options.LastIP = socketAddress.Substring(5, socketAddress.Length - 12);
                Config.SaveOptions();
            }
        }

        public void DisplayPingIndicator()
        {
            GameObject previousObject = GameObject.Find("Ping Indicator");
            if (previousObject == null)
            {
                GameObject targetObject = GameObject.Find("label");
                pingIndicator = GameObject.Instantiate(targetObject, UIRoot.instance.uiGame.gameObject.transform).GetComponent<Text>();
                pingIndicator.gameObject.name = "Ping Indicator";
                pingIndicator.alignment = TextAnchor.UpperLeft;
                pingIndicator.enabled = true;
                RectTransform rect = pingIndicator.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.offsetMax = new Vector2(-68f, -40f);
                rect.offsetMin = new Vector2(10f, -100f);
                pingIndicator.text = "";
                pingIndicator.fontSize = 14;
            }
            else
            {
                pingIndicator = previousObject.GetComponent<Text>();
                pingIndicator.enabled = true;
            }
        }

        void Disconnect()
        {
            IsConnected = false;
            clientSocket?.Close((ushort)DisconnectionReason.ClientRequestedDisconnect, "Player left the game");
        }

        public void DestroySession()
        {
            Disconnect();
            if (pingIndicator != null) pingIndicator.enabled = false;
            Destroy(gameObject);
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            serverConnection?.SendPacket(packet);
        }
        public void SendPacketToLocalStar<T>(T packet) where T : class, new()
        {
            serverConnection?.SendPacket(new StarBroadcastPacket(PacketProcessor.Write(packet), GameMain.data.localStar?.id ?? -1));
        }
        public void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
        {
            serverConnection?.SendPacket(new PlanetBroadcastPacket(PacketProcessor.Write(packet), GameMain.mainPlayer.planetId));
        }
        public void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new()
        {
            //Should send packet to particular planet
            //Not needed at the moment, used only on the host side
            throw new NotImplementedException();
        }
        public void SendPacketToStar<T>(T packet, int starId) where T : class, new()
        {
            //Should send packet to particular planet
            //Not needed at the moment, used only on the host side
            throw new NotImplementedException();
        }

        public void SendPacketToStarExclude<T>(T packet, int starId, NebulaConnection exclude) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Reconnect()
        {
            SimulatedWorld.Clear();
            Disconnect();
            ConnectInternal();
        }

        public void UpdatePingIndicator()
        {
            int newDelay = (int)((Time.time - pingTimestamp) * 1000);
            if (newDelay != previousDelay)
            {
                pingIndicator.text = $"Ping: {newDelay}ms";
                previousDelay = newDelay;
            }
        }

        private void ClientSocket_OnMessage(object sender, MessageEventArgs e)
        {
            PacketProcessor.EnqueuePacketForProcessing(e.RawData, new NebulaConnection(clientSocket, serverEndpoint, PacketProcessor));
        }

        private void ClientSocket_OnOpen(object sender, System.EventArgs e)
        {
            DisableNagleAlgorithm(clientSocket);

            Log.Info($"Server connection established: {clientSocket.Url}");
            serverConnection = new NebulaConnection(clientSocket, serverEndpoint, PacketProcessor);
            IsConnected = true;
            //TODO: Maybe some challenge-response authentication mechanism?
            SendPacket(new HandshakeRequest(
                CryptoUtils.GetPublicKey(CryptoUtils.GetOrCreateUserCert()),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName,
                new Float3(Config.Options.MechaColorR / 255, Config.Options.MechaColorG / 255, Config.Options.MechaColorB / 255),
                LocalPlayer.GS2_GSSettings != null));
        }

        static void DisableNagleAlgorithm(WebSocket socket)
        {
            var tcpClient = AccessTools.FieldRefAccess<WebSocket, TcpClient>("_tcpClient")(socket);
            tcpClient.NoDelay = true;
        }

        private void ClientSocket_OnClose(object sender, CloseEventArgs e)
        {
            IsConnected = false;
            serverConnection = null;

            UnityDispatchQueue.RunOnMainThread(() =>
            {
                // If the client is Quitting by himself, we don't have to inform him of his disconnection.
                if (e.Code == (ushort)DisconnectionReason.ClientRequestedDisconnect)
                    return;

                if (e.Code == (ushort)DisconnectionReason.ModVersionMismatch)
                {
                    string[] versions = e.Reason.Split(';');
                    InGamePopup.ShowWarning(
                        "Mod Version Mismatch",
                        $"Your Nebula Multiplayer Mod is not the same as the Host version.\nYou:{versions[0]} - Remote:{versions[1]}",
                        "OK",
                        OnDisconnectPopupCloseBeforeGameLoad);
                    return;
                }

                if (e.Code == (ushort)DisconnectionReason.GameVersionMismatch)
                {
                    string[] versions = e.Reason.Split(';');
                    InGamePopup.ShowWarning(
                        "Game Version Mismatch",
                        $"Your version of the game is not the same as the one used by the Host.\nYou:{versions[0]} - Remote:{versions[1]}",
                        "OK",
                        OnDisconnectPopupCloseBeforeGameLoad);
                    return;
                }

                if (SimulatedWorld.IsGameLoaded)
                {
                    InGamePopup.ShowWarning(
                        "Connection Lost",
                        $"You have been disconnected from the server.\n{e.Reason}",
                        "Quit",
                        () => LocalPlayer.LeaveGame());
                }
                else
                {
                    InGamePopup.ShowWarning(
                        "Server Unavailable",
                        $"Could not reach the server, please try again later.",
                        "OK",
                        () =>
                        {
                            LocalPlayer.IsMasterClient = false;
                            SimulatedWorld.Clear();
                            DestroySession();
                            OnDisconnectPopupCloseBeforeGameLoad();
                        });
                }
            });
        }

        private void OnDisconnectPopupCloseBeforeGameLoad()
        {
            GameObject overlayCanvasGo = GameObject.Find("Overlay Canvas");
            Transform multiplayerMenu = overlayCanvasGo?.transform?.Find("Nebula - Multiplayer Menu");
            multiplayerMenu?.gameObject?.SetActive(true);
        }

        private void Update()
        {
            PacketProcessor.ProcessPacketQueue();

            if (SimulatedWorld.IsGameLoaded)
            {
                mechaSynchonizationTimer += Time.deltaTime;
                if (mechaSynchonizationTimer > MECHA_SYNCHONIZATION_INTERVAL)
                {
                    SendPacket(new PlayerMechaData(GameMain.mainPlayer));
                    mechaSynchonizationTimer = 0f;
                }

                pingTimer += Time.deltaTime;
                if (pingTimer >= 1f)
                {
                    SendPacket(new PingPacket());
                    pingTimestamp = Time.time;
                    pingTimer = 0f;
                }
            }
        }
    }
}

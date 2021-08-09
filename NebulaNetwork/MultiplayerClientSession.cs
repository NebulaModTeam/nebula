using BepInEx;
using Mirror;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Routers;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static NebulaModel.Networking.NebulaConnection;

namespace NebulaNetwork
{
    public class MultiplayerClientSession : MonoBehaviour, INetworkProvider
    {
        public static MultiplayerClientSession Instance { get; protected set; }
        public static Uri LastConnectedUri { get; private set; }  = null;

        private NetworkManager NetworkManager;
        private NetPacketProcessor PacketProcessor;

        private float mechaSynchonizationTimer = 0f;

        private float pingTimer = 0f;
        private float pingTimestamp = 0f;
        private Text pingIndicator;
        private int previousDelay = 0;
        private const int MECHA_SYNCHONIZATION_INTERVAL = 5;

        private void Awake()
        {
            Instance = this;
        }

        public void Connect(Uri uri)
        {
            LocalPlayer.TryLoadGalacticScale2();

            PacketProcessor = new NetPacketProcessor();

            PacketUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor, false);

            NebulaConnection.PacketProcessor = PacketProcessor;

            NetworkManager = MirrorManager.SetupMirror(typeof(ClientManager), uri);

            NetworkClient.RegisterHandler<NebulaMessage>((nebulaMessage) => OnNebulaMessage(nebulaMessage));
            NetworkClient.RegisterHandler<PacketProcessors.Planet.FactoryData>(PacketProcessors.Planet.FactoryData.ProcessPacket);
            NetworkClient.RegisterHandler<PacketProcessors.Planet.PlanetDataResponse>(PacketProcessors.Planet.PlanetDataResponse.ProcessPacket);
            NetworkClient.RegisterHandler<PacketProcessors.Universe.DysonSphereData>(PacketProcessors.Universe.DysonSphereData.ProcessPacket);

            NetworkManager.StartClient(uri);

            LastConnectedUri = uri;
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

        public void UpdatePingIndicator()
        {
            int newDelay = (int)((Time.time - pingTimestamp) * 1000);
            if (newDelay != previousDelay)
            {
                pingIndicator.text = $"Ping: {newDelay}ms";
                previousDelay = newDelay;
            }
        }

        private void Update()
        {
            PacketProcessor.ProcessPacketQueue();

            if (SimulatedWorld.IsGameLoaded)
            {
                mechaSynchonizationTimer += Time.deltaTime;
                if (mechaSynchonizationTimer > MECHA_SYNCHONIZATION_INTERVAL)
                {
                    NetworkClient.connection.SendPacket(new PlayerMechaData(GameMain.mainPlayer));
                    mechaSynchonizationTimer = 0f;
                }

                pingTimer += Time.deltaTime;
                if (pingTimer >= 1f)
                {
                    NetworkClient.connection.SendPacket(new PingPacket());
                    pingTimestamp = Time.time;
                    pingTimer = 0f;
                }
            }
        }

        static void Disconnect()
        {
            NetworkClient.connection?.Disconnect();
        }

        public void DestroySession()
        {
            NetworkManager.StopClient();
            if (pingIndicator != null) pingIndicator.enabled = false;
            Destroy(gameObject);
            Destroy(GameObject.Find("Mirror Networking"));
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            NetworkClient.connection.SendPacket(packet);
        }
        public void SendPacketToLocalStar<T>(T packet) where T : class, new()
        {
            NetworkClient.connection.SendPacket(new StarBroadcastPacket(PacketProcessor.Write(packet), GameMain.data.localStar?.id ?? -1));
        }
        public void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
        {
            NetworkClient.connection.SendPacket(new PlanetBroadcastPacket(PacketProcessor.Write(packet), GameMain.mainPlayer.planetId));
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

        public void SendPacketToStarExclude<T>(T packet, int starId, NetworkConnection exclude) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Reconnect()
        {
            SimulatedWorld.Clear();
            Disconnect();
            Connect(LastConnectedUri);
        }

    }

    public class ClientManager : NetworkManager
    {
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            NebulaModel.Logger.Log.Info($"Server connection established");

            SimulatedWorld.Initialize();

            LocalPlayer.IsMasterClient = false;
            LocalPlayer.SetNetworkProvider(MultiplayerClientSession.Instance);

            if (Config.Options.RememberLastIP)
            {
                // We've successfully connected, set connection URI as last connected IP
                Config.Options.LastIP = MultiplayerClientSession.LastConnectedUri.ToString();
                Config.SaveOptions();
            }

            //TODO: Maybe some challenge-response authentication mechanism?
            conn.SendPacket(new HandshakeRequest(
                CryptoUtils.GetPublicKey(CryptoUtils.GetOrCreateUserCert()),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName,
                new Float3(Config.Options.MechaColorR / 255, Config.Options.MechaColorG / 255, Config.Options.MechaColorB / 255),
                LocalPlayer.GS2_GSSettings != null));
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            ThreadingHelper.Instance.StartSyncInvoke(() =>
            {
                if (SimulatedWorld.IsGameLoaded)
                {
                    InGamePopup.ShowWarning(
                        "Connection Lost",
                        $"You have been disconnected from the server.\n",
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
                            MultiplayerClientSession.Instance.DestroySession();
                            OnDisconnectPopupCloseBeforeGameLoad();
                        });
                }
            });
        }

        private static void OnDisconnectPopupCloseBeforeGameLoad()
        {
            GameObject overlayCanvasGo = GameObject.Find("Overlay Canvas");
            Transform multiplayerMenu = overlayCanvasGo.transform.Find("Nebula - Multiplayer Menu");
            multiplayerMenu.gameObject.SetActive(true);
        }
    }
}

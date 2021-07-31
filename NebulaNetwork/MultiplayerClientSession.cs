using Mirror;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaModel;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using NebulaModel.Networking;
using static NebulaModel.Networking.NebulaConnection;
using NebulaModel.Networking.Serialization;
using System.Net;
using kcp2k;

namespace NebulaNetwork
{
    public class MultiplayerClientSession : MonoBehaviour
    {
        public static MultiplayerClientSession Instance { get; protected set; }

        private NetworkManager NetworkManager;
        private NetworkConnection NetworkConnection;
        private NetPacketProcessor PacketProcessor;

        private float mechaSynchonizationTimer = 0f;

        private float pingTimer = 0f;
        private float pingTimestamp = 0f;
        private Text pingIndicator;
        private int previousDelay = 0;
        private const int MECHA_SYNCHONIZATION_INTERVAL = 5;
        public bool IsConnected { get; protected set; }

        private void Awake()
        {
            Instance = this;
        }

        public void Connect(string ip, int port)
        {
            LocalPlayer.TryLoadGalacticScale2();

            NetworkManager = (NetworkManager)gameObject.AddComponent(typeof(NetworkManager));
            NetworkManager.transport = (TelepathyTransport)gameObject.AddComponent(typeof(TelepathyTransport));
            gameObject.AddComponent(typeof(NetworkManagerHUD));
            NetworkManager.OnValidate();
            Transport.activeTransport = NetworkManager.transport;

            PacketProcessor = new NetPacketProcessor();
#if DEBUG
            PacketProcessor.SimulateLatency = true;
#endif

            PacketUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor, false);

            NetworkClient.OnConnectedEvent += OnConnected;

            NetworkManager.StartClient();

            SimulatedWorld.Initialize();

            LocalPlayer.IsMasterClient = false;

            if (Config.Options.RememberLastIP)
            {
                // We've successfully connected, set connection as last ip, cutting out "ws://" and "/socket"
                Config.Options.LastIP = NetworkClient.connection.address;
                Config.SaveOptions();
            }
}

        private void OnConnected()
        {
            NebulaModel.Logger.Log.Info($"Server connection established");
            NetworkConnection = NetworkClient.connection;
            IsConnected = true;
            //TODO: Maybe some challenge-response authentication mechanism?
            NetworkConnection.SendPacket(new HandshakeRequest(
                CryptoUtils.GetPublicKey(CryptoUtils.GetOrCreateUserCert()),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName,
                new Float3(Config.Options.MechaColorR / 255, Config.Options.MechaColorG / 255, Config.Options.MechaColorB / 255),
                LocalPlayer.GS2_GSSettings != null));
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

        public void SetupClient()
        {
            NetworkClient.RegisterHandler<NebulaMessage>(OnNebulaMessage);
        }

        private void OnNebulaMessage(NebulaMessage arg1)
        {
            PacketProcessor.EnqueuePacketForProcessing(arg1.Payload.ToArray(), NetworkClient.connection);
        }
    }
}

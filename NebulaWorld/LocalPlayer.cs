using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaWorld.MonoBehaviours;
using NebulaWorld.MonoBehaviours.Local;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NebulaWorld
{
    public class LocalPlayer : INebulaPlayer
    {
        public static LocalPlayer Instance = new LocalPlayer();
        
        public bool IsMasterClient { get; set; }
        public ushort PlayerId => Data.PlayerId;
        public PlayerData Data { get; private set; }
        public Dictionary<int, byte[]> PendingFactories { get; set; } = new Dictionary<int, byte[]>();

        private INetworkProvider networkProvider;

        public void SetNetworkProvider(INetworkProvider provider)
        {
            networkProvider = provider;
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            networkProvider?.SendPacket(packet);
        }

        public void SendPacketToLocalStar<T>(T packet) where T : class, new()
        {
            networkProvider?.SendPacketToLocalStar(packet);
        }

        public void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
        {
            networkProvider?.SendPacketToLocalPlanet(packet);
        }

        public void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new()
        {
            networkProvider?.SendPacketToPlanet(packet, planetId);
        }

        public void SendPacketToStar<T>(T packet, int starId) where T : class, new()
        {
            networkProvider?.SendPacketToStar(packet, starId);
        }

        public void SendPacketToStarExclude<T>(T packet, int starId, INebulaConnection exclude) where T : class, new()
        {
            networkProvider?.SendPacketToStarExclude(packet, starId, (NebulaConnection)exclude);
        }

        public void SetReady()
        {

            if (!IsMasterClient)
            {
                // Notify the server that we are done with loading the game
                networkProvider.SendPacket(new SyncComplete());
                InGamePopup.FadeOut();
            }

            // Finally we add the local player components to the player character
            GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerMovement>();
            GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerAnimation>();

            if (!IsMasterClient)
            {
                //Subscribe for the local star events
                SendPacket(new PlayerUpdateLocalStarId(GameMain.data.localStar.id));
            }
        }

        public void SetPlayerData(PlayerData data)
        {
            Data = data;
        }

        public void LeaveGame()
        {
            networkProvider.DestroySession();
            PendingFactories.Clear();
            IsMasterClient = false;
            SimulatedWorld.Instance.Clear();
            SimulatedWorld.Instance.ExitingMultiplayerSession = true;

            if (!UIRoot.instance.backToMainMenu)
            {
                UIRoot.instance.backToMainMenu = true;
                DSPGame.EndGame();
            }
        }
    }
}

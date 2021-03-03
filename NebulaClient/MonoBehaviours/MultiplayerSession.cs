using NebulaClient.GameLogic;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using UnityEngine;

namespace NebulaClient.MonoBehaviours
{
    public class MultiplayerSession : MonoBehaviour
    {
        public static MultiplayerSession instance;

        public Client Client { get; private set; }
        public PlayerManager PlayerManager { get; private set; }

        private string serverIp;
        private int serverPort;

        private UIMessageBox statusBox;

        void Awake()
        {
            instance = this;

            Client = new Client();
            Client.PacketProcessor.SubscribeReusable<JoinSessionConfirmed>(OnJoinSessionConfirmed);
            Client.PacketProcessor.SubscribeReusable<RemotePlayerJoined>(OnRemotePlayerJoined);
            Client.PacketProcessor.SubscribeReusable<PlayerDisconnected>(OnRemotePlayerDisconnect);
            Client.PacketProcessor.SubscribeReusable<Movement>(OnPlayerMovement);
            Client.PacketProcessor.SubscribeReusable<PlayerAnimationUpdate>(OnPlayerAnimationUpdate);
            Client.PacketProcessor.SubscribeReusable<VegeMined>(OnVegeMined);
            Client.PacketProcessor.SubscribeReusable<PlayerColorChanged>(OnPlayerColorChanged);
            Client.PacketProcessor.SubscribeReusable<HandshakeResponse>(OnHandshakeResponse);
            Client.PacketProcessor.SubscribeReusable<InitialState>(OnInitialState);
            Client.PacketProcessor.SubscribeReusable<SyncComplete>(OnSyncComplete);
        }

        public void Connect(string ip, int port)
        {
            serverIp = ip;
            serverPort = port;
            Client.Connect(ip, port);

            PlayerManager = new PlayerManager();

            statusBox = UIMessageBox.Show("Loading", "Loading from the server, please wait", null, 0);
        }

        public void TryToReconnect()
        {
            Disconnect();
            Connect(serverIp, serverPort);
            // TODO: Should freeze game and add a spinner or something during the reconnection.
            // Else the player can still move around during the reconnection procedure which is weird
        }

        public void Disconnect()
        {
            if (Client.IsConnected)
            {
                Client.Disconnect();
            }

            CleanupSession();

        }

        public void LeaveGame()
        {
            Disconnect();

            // Go back to the main menu
            if (!UIRoot.instance.backToMainMenu)
            {
                UIRoot.instance.backToMainMenu = true;
                DSPGame.EndGame();
            }
        }

        void OnDestroy()
        {
            // This make sure to disconnect if you force close the game.
            Disconnect();
        }

        void CleanupSession()
        {
            PlayerManager.RemoveAll();
        }

        void Update()
        {
            Client.Update();
        }

        private void OnJoinSessionConfirmed(JoinSessionConfirmed packet)
        {
            PlayerManager.SetLocalPlayer(packet.LocalPlayerId);
            statusBox?.FadeOut();
            statusBox = null;
        }

        private void OnRemotePlayerJoined(RemotePlayerJoined packet)
        {
            PlayerManager.AddRemotePlayer(packet.PlayerId);
            statusBox = UIMessageBox.Show("A new player is joining!", "A new player is joining, please wait while they load in", null, 0);
            GameMain.Pause();

            if (PlayerManager.IsMasterClient)
            {
                Client.SendPacket(new InitialState(UniverseGen.algoVersion, 1, 64, 1f));
            }
        }

        private void OnRemotePlayerDisconnect(PlayerDisconnected packet)
        {
            PlayerManager.RemovePlayer(packet.PlayerId);
        }

        private void OnPlayerMovement(Movement packet)
        {
            PlayerManager.GetPlayerModelById(packet.PlayerId)?.Movement.UpdatePosition(packet);
        }

        private void OnPlayerAnimationUpdate(PlayerAnimationUpdate packet)
        {
            PlayerManager.GetPlayerModelById(packet.PlayerId)?.Animator.UpdateState(packet);
        }

        private void OnPlayerColorChanged(PlayerColorChanged packet)
        {
            PlayerManager.GetPlayerById(packet.PlayerId).UpdateColor(packet.Color);
        }

        private void OnVegeMined(VegeMined packet)
        {
            PlanetData planet = GameMain.galaxy?.PlanetById(packet.PlanetID);
            if (planet == null)
            {
                return;
            }

            if (packet.isVegetable) // Trees, rocks, leaves, etc
            {
                VegeData vData = (VegeData)planet.factory?.GetVegeData(packet.MiningID);
                VegeProto vProto = LDB.veges.Select((int)vData.protoId);
                if (vProto != null && planet.id == GameMain.localPlanet?.id)
                {
                    VFEffectEmitter.Emit(vProto.MiningEffect, vData.pos, vData.rot);
                    VFAudio.Create(vProto.MiningAudio, null, vData.pos, true);
                }
                planet.factory?.RemoveVegeWithComponents(vData.id);
            }
            else // veins
            {
                VeinData vData = (VeinData)planet.factory?.GetVeinData(packet.MiningID);
                VeinProto vProto = LDB.veins.Select((int)vData.type);
                if (vProto != null)
                {
                    if (planet.factory?.veinPool[packet.MiningID].amount > 0)
                    {
                        VeinData[] vPool = planet.factory?.veinPool;
                        PlanetData.VeinGroup[] vGroups = planet.factory?.planet.veinGroups;
                        long[] vAmounts = planet.veinAmounts;
                        vPool[packet.MiningID].amount -= 1;
                        vGroups[(int)vData.groupIndex].amount -= 1;
                        vAmounts[(int)vData.type] -= 1;

                        if (planet.id == GameMain.localPlanet?.id)
                        {
                            VFEffectEmitter.Emit(vProto.MiningEffect, vData.pos, Maths.SphericalRotation(vData.pos, 0f));
                            VFAudio.Create(vProto.MiningAudio, null, vData.pos, true);
                        }
                    }
                    else
                    {
                        PlanetData.VeinGroup[] vGroups = planet.factory?.planet.veinGroups;
                        vGroups[vData.groupIndex].count -= 1;

                        if (planet.id == GameMain.localPlanet?.id)
                        {
                            VFEffectEmitter.Emit(vProto.MiningEffect, vData.pos, Maths.SphericalRotation(vData.pos, 0f));
                            VFAudio.Create(vProto.MiningAudio, null, vData.pos, true);
                        }

                        planet.factory?.RemoveVeinWithComponents(vData.id);
                    }
                }
            }
        }

        private void OnHandshakeResponse(HandshakeResponse packet)
        {
            if (packet.IsFirstPlayer)
            {
                // We want to host the world here
                GameDesc gameDesc = new GameDesc();
                gameDesc.SetForNewGame(UniverseGen.algoVersion, 1, 64, 1, 1f);
                DSPGame.StartGameSkipPrologue(gameDesc);
                PlayerManager.IsMasterClient = true;
                statusBox.FadeOut();
            }
            else
            {
                statusBox.FadeOut();
                statusBox = null;
                statusBox = UIMessageBox.Show("Loading", "Loading state from server, please wait", null, UIMessageBox.INFO);

                foreach (var playerId in packet.OtherPlayerIds)
                {
                    PlayerManager.AddRemotePlayer(playerId);
                }
            }
        }

        private void OnInitialState(InitialState s)
        {
            GameDesc gameDesc = new GameDesc();
            gameDesc.SetForNewGame(s.AlgoVersion, s.GalaxySeed, s.StarCount, 1, s.ResourceMultiplier);
            DSPGame.StartGameSkipPrologue(gameDesc);
            statusBox?.FadeOut();
            statusBox = null;

            Client.SendPacket(new SyncComplete());
        }

        private void OnSyncComplete(SyncComplete packet)
        {
            GameMain.Resume();
            statusBox?.FadeOut();
            statusBox = null;
        }
    }
}


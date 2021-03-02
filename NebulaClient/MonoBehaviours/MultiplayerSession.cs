using NebulaClient.GameLogic;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

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
            if (statusBox != null)
            {
                statusBox.FadeOut();
                statusBox = null;
            }
        }

        private void OnRemotePlayerJoined(RemotePlayerJoined packet)
        {
            PlayerManager.AddRemotePlayer(packet.PlayerId);
            statusBox = UIMessageBox.Show("A new player is joining!", "A new player is joining, please wait while they load in", null, 0);
            GameMain.Pause();

            if(PlayerManager.WeAreMainPlayer)
            {
                GameSave.SaveCurrentGame("MPSYNCSTATE");
                Client.SendPacket(new InitialState(GameConfig.gameSaveFolder + "/MPSYNCSTATE.dsv"));
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

        private void OnVegeMined(VegeMined packet)
	    {
            GameMain.localPlanet?.factory?.RemoveVegeWithComponents(packet.VegeID);
	    }

        private void OnHandshakeResponse(HandshakeResponse packet)
        {
            if(packet.IsFirstPlayer)
            {
                // We want to host the world here
                GameDesc gameDesc = new GameDesc();
                gameDesc.SetForNewGame(UniverseGen.algoVersion, 1, 64, 1, 1f);
                DSPGame.StartGameSkipPrologue(gameDesc);
                PlayerManager.WeAreMainPlayer = true;
                statusBox.FadeOut();
            }
            else
            {
                statusBox.FadeOut();
                statusBox = null;
                statusBox = UIMessageBox.Show("Loading", "Loading state from server, please wait", null, UIMessageBox.INFO);

                foreach(var playerId in packet.OtherPlayerIds)
                {
                    PlayerManager.AddRemotePlayer(playerId);
                }
            }
        }

        private void OnInitialState(InitialState packet)
        {
            StartCoroutine(InitialStateSyncUtils.DownloadInitialState(packet.URI, statusBox, () => 
            {
                Debug.Log("Loading from: " + GameConfig.gameSaveFolder + "INITIALMPSTATE.dsv");
                DSPGame.StartGame("INITIALMPSTATE");
                statusBox.FadeOut();
                statusBox = null;

                Client.SendPacket(new SyncComplete());
            }));
        }


        private void OnSyncComplete(SyncComplete packet)
        {
            GameMain.Resume();
            if(statusBox != null)
            {
                statusBox.FadeOut();
                statusBox = null;
            }
        }

        private class InitialStateSyncUtils
        {
            private static bool isRunning = false;

            public static IEnumerator DownloadInitialState(string uri, UIMessageBox messageBox, Action callback)
            {
                {
                    using (UnityWebRequest request = UnityWebRequest.Get(uri))
                    {
                        isRunning = true;

                        //TODO: Make the message box display a progress bar, will require harmony to be added as a reference
                        messageBox.StartCoroutine(DownloadProgress(request));
                        yield return request.SendWebRequest();
                        isRunning = false;

                        if (request.isNetworkError || request.isHttpError)
                        {
                            Debug.LogError("Failed to download!");
                        }
                        else
                        {
                            byte[] receivedBytes = request.downloadHandler.data;
                            string path = GameConfig.gameSaveFolder + "INITIALMPSTATE.dsv";
                            Debug.Log("Saving to: " + path + " - Length: " + receivedBytes.Length);
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }

                            using (FileStream fs = new FileStream(path, FileMode.Create))
                            {
                                fs.Write(receivedBytes, 0, receivedBytes.Length);
                                fs.Flush();
                            }
                        }
                    }
                }

                callback();
            }

            private static IEnumerator DownloadProgress(UnityWebRequest request)
            {
                while (isRunning)
                {
                    Debug.Log($"Download progress: { request.downloadProgress * 100 }%");
                    yield return new WaitForSeconds(0.1f);
                }

                yield return null;
            }
        }
    }
}

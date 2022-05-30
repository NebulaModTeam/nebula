using NebulaAPI;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaModel.Packets.Trash;
using NebulaModel.Packets.Universe;
using NebulaModel.Packets.Warning;
using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours;
using NebulaWorld.MonoBehaviours.Local;
using NebulaWorld.MonoBehaviours.Remote;
using NebulaWorld.SocialIntegration;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NebulaWorld
{
    /// <summary>
    /// This class keeps track of our simulated world. It holds all temporary entities like remote player models 
    /// and also helps to execute some remote player actions that you would want to replicate on the local client.
    /// </summary>
    public class SimulatedWorld : IDisposable
    {
        private sealed class ThreadSafe
        {
            internal readonly Dictionary<ushort, RemotePlayerModel> RemotePlayersModels = new Dictionary<ushort, RemotePlayerModel>();
        }

        private readonly ThreadSafe threadSafe = new ThreadSafe();

        private Text pingIndicator;
        private LocalPlayerMovement localPlayerMovement;
        private LocalPlayerAnimation localPlayerAnimation;

        public Locker GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels)
        {
            return threadSafe.RemotePlayersModels.GetLocked(out remotePlayersModels);
        }

        public bool IsPlayerJoining { get; set; }

        public SimulatedWorld()
        {
            threadSafe = new ThreadSafe();
        }

        public void Dispose()
        {
            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                foreach (RemotePlayerModel model in remotePlayersModels.Values)
                {
                    model.Destroy();
                }

                remotePlayersModels.Clear();
            }

            UnityEngine.Object.Destroy(localPlayerMovement);
            UnityEngine.Object.Destroy(localPlayerAnimation);
            SetPauseIndicator(true);
        }

        public void SetupInitialPlayerState()
        {
            if (!Multiplayer.Session.IsGameLoaded)
            {
                Log.Warn("Trying to setup initial player state before the game is loaded!");
                return;
            }

            if (!Multiplayer.Session.LocalPlayer.IsInitialDataReceived)
            {
                Log.Warn("Trying to setup initial player state before the player data was received!");
                return;
            }

            LocalPlayer player = Multiplayer.Session.LocalPlayer as LocalPlayer;

            // If not a new client, we need to update the player position to put him where he was previously
            if (player.IsClient && !player.IsNewPlayer)
            {
                GameMain.mainPlayer.planetId = player.Data.LocalPlanetId;
                if (player.Data.LocalPlanetId == -1)
                {
                    GameMain.mainPlayer.uPosition = new VectorLF3(player.Data.UPosition.x, player.Data.UPosition.y, player.Data.UPosition.z);
                }
                else
                {
                    GameMain.mainPlayer.position = player.Data.LocalPlanetPosition.ToVector3();
                    GameMain.mainPlayer.uPosition = new VectorLF3(GameMain.localPlanet.uPosition.x + GameMain.mainPlayer.position.x, GameMain.localPlanet.uPosition.y + GameMain.mainPlayer.position.y, GameMain.localPlanet.uPosition.z + GameMain.mainPlayer.position.z);
                }
                GameMain.mainPlayer.uRotation = Quaternion.Euler(player.Data.Rotation.ToVector3());

                // Load client's saved data from the last session.
                GameMain.mainPlayer.package = player.Data.Mecha.Inventory;
                GameMain.mainPlayer.mecha.forge = player.Data.Mecha.Forge;
                GameMain.mainPlayer.mecha.coreEnergy = player.Data.Mecha.CoreEnergy;
                GameMain.mainPlayer.mecha.reactorEnergy = player.Data.Mecha.ReactorEnergy;
                GameMain.mainPlayer.mecha.reactorStorage = player.Data.Mecha.ReactorStorage;
                GameMain.mainPlayer.mecha.warpStorage = player.Data.Mecha.WarpStorage;
                GameMain.mainPlayer.SetSandCount(player.Data.Mecha.SandCount);

                // Fix references that broke during import
                GameMain.mainPlayer.mecha.forge.mecha = GameMain.mainPlayer.mecha;
                GameMain.mainPlayer.mecha.forge.player = GameMain.mainPlayer;
                GameMain.mainPlayer.mecha.forge.gameHistory = GameMain.data.history;
                GameMain.mainPlayer.mecha.forge.gameHistory = GameMain.data.history;
            }

            // Initialization on the host side after game is loaded
            Multiplayer.Session.Factories.InitializePrebuildRequests();

            if (player.IsClient)
            {
                // Update player's Mecha tech bonuses
                ((MechaData)player.Data.Mecha).TechBonuses.UpdateMech(GameMain.mainPlayer.mecha);

                if (player.IsNewPlayer)
                {
                    // Set mecha to full energy so new client won't have low energy when starting
                    GameMain.mainPlayer.mecha.coreEnergy = GameMain.mainPlayer.mecha.coreEnergyCap;
                    if (GameMain.history.logisticShipWarpDrive)
                    {
                        // If warp has unlocked, give new client few warpers
                        GameMain.mainPlayer.TryAddItemToPackage(1210, 5, 0, false);
                    }
                }

                // Enable Ping Indicator for Clients
                DisplayPingIndicator();

                // Notify the server that we are done loading the game
                byte[] clientCert = CryptoUtils.GetPublicKey(CryptoUtils.GetOrCreateUserCert());
                Multiplayer.Session.Network.SendPacket(new SyncComplete(clientCert));

                // Subscribe for the local star events
                Multiplayer.Session.Network.SendPacket(new PlayerUpdateLocalStarId(GameMain.data.localStar.id));

                // Request latest warning signal
                Multiplayer.Session.Network.SendPacket(new WarningDataRequest(WarningRequestEvent.Signal));

                // Hide the "Joining Game" popup
                InGamePopup.FadeOut();
            }

            // store original sand count of host if we are syncing it to preserve it when saving the game
            if (Config.Options.SyncSoil)
            {
                player.Data.Mecha.SandCount = GameMain.mainPlayer.sandCount;
            }

            // Finally we need add the local player components to the player character
            localPlayerMovement = GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerMovement>();
            localPlayerAnimation = GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerAnimation>();
            // ChatManager should continuous exsit until the game is closed
            GameMain.mainPlayer.gameObject.AddComponentIfMissing<ChatManager>();
        }

        public void OnPlayerJoining(string Username)
        {
            if (!IsPlayerJoining)
            {
                IsPlayerJoining = true;
                Multiplayer.Session.CanPause = true;
                GameMain.isFullscreenPaused = true;
                InGamePopup.ShowInfo("Loading", Username + " joining the game, please wait\n(Use BulletTime mod to unfreeze the game)", null);
            }
        }

        public void OnPlayerJoinedGame(INebulaPlayer player)
        {
            Multiplayer.Session.World.SpawnRemotePlayerModel(player.Data);

            // Load overriden Planet and Star names
            player.SendPacket(new NameInputPacket(GameMain.galaxy, Multiplayer.Session.LocalPlayer.Id));

            // add together player sand count and tell others if we are syncing soil
            if (Config.Options.SyncSoil)
            {
                GameMain.mainPlayer.sandCount += player.Data.Mecha.SandCount;
                Multiplayer.Session.Network.SendPacket(new PlayerSandCount(GameMain.mainPlayer.sandCount));
            }

            // (Host only) Trigger when a new client added to connected players
            Log.Info($"Client{player.Data.PlayerId} - {player.Data.Username} joined");
            NebulaModAPI.OnPlayerJoinedGame?.Invoke(player.Data);
        }

        public void OnPlayerLeftGame(INebulaPlayer player)
        {
            Multiplayer.Session.World.DestroyRemotePlayerModel(player.Id);

            if (Config.Options.SyncSoil)
            {
                GameMain.mainPlayer.sandCount -= player.Data.Mecha.SandCount;
                UIRoot.instance.uiGame.OnSandCountChanged(GameMain.mainPlayer.sandCount, -player.Data.Mecha.SandCount);
                Multiplayer.Session.Network.SendPacket(new PlayerSandCount(GameMain.mainPlayer.sandCount));
            }

            // (Host only) Trigger when a connected client leave the game
            Log.Info($"Client{player.Data.PlayerId} - {player.Data.Username} left");
            NebulaModAPI.OnPlayerLeftGame?.Invoke(player.Data);
        }

        public void OnAllPlayersSyncCompleted()
        {
            IsPlayerJoining = false;
            InGamePopup.FadeOut();
            GameMain.isFullscreenPaused = false;
            Multiplayer.Session.CanPause = false;
        }

        public void SpawnRemotePlayerModel(IPlayerData playerData)
        {
            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                if (!remotePlayersModels.ContainsKey(playerData.PlayerId))
                {
                    Log.Info($"Spawn player model {playerData.PlayerId} {playerData.Username}");
                    RemotePlayerModel model = new RemotePlayerModel(playerData.PlayerId, playerData.Username);
                    remotePlayersModels.Add(playerData.PlayerId, model);

                    // Show conneted message
                    string planetname = GameMain.galaxy.PlanetById(playerData.LocalPlanetId)?.displayName ?? "In space";
                    string message = string.Format("[{0:HH:mm}] {1} connected ({2})", DateTime.Now, playerData.Username, planetname);
                    SendChatMessage(message, ChatMessageType.SystemInfoMessage);
                }
            }
        }

        public void DestroyRemotePlayerModel(ushort playerId)
        {
            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                if (remotePlayersModels.TryGetValue(playerId, out RemotePlayerModel player))
                {
                    // Show disconnected message
                    string message = string.Format("[{0:HH:mm}] {1} disconnected", DateTime.Now, player.Username);
                    SendChatMessage(message, ChatMessageType.SystemInfoMessage);

                    player.Destroy();
                    remotePlayersModels.Remove(playerId);
                    if (remotePlayersModels.Count == 0 && Config.Options.AutoPauseEnabled)
                    {
                        Multiplayer.Session.CanPause = true;
                    }
                }
            }
        }

        public void UpdateRemotePlayerPosition(PlayerMovement packet)
        {
            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
                {
                    player.Movement.UpdatePosition(packet);
                }
            }
        }

        public void UpdateRemotePlayerAnimation(PlayerAnimationUpdate packet)
        {
            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
                {
                    player.Animator.UpdateState(packet);
                }
            }
        }

        public void UpdateRemotePlayerWarpState(PlayerUseWarper packet)
        {
            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                if (packet.PlayerId == 0)
                {
                    packet.PlayerId = 1; // host sends himself as PlayerId 0 but clients see him as id 1
                }

                if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
                {
                    if (packet.WarpCommand)
                    {
                        player.Effects.StartWarp();
                    }
                    else
                    {
                        player.Effects.StopWarp();
                    }
                }
            }
        }

        public void UpdateRemotePlayerDrone(NewDroneOrderPacket packet)
        {
            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
                {
                    //Setup drone of remote player based on the drone data
                    ref MechaDrone drone = ref player.PlayerInstance.mecha.drones[packet.DroneId];
                    MechaDroneLogic droneLogic = player.PlayerInstance.mecha.droneLogic;
                    PlanetFactory tmpFactory = droneLogic.factory;

                    droneLogic.factory = GameMain.galaxy.PlanetById(packet.PlanetId).factory;

                    // factory can sometimes be null when transitioning to or from a planet, in this case we do not want to continue
                    if (droneLogic.factory == null)
                    {
                        droneLogic.factory = tmpFactory;
                        return;
                    }

                    drone.stage = packet.Stage;
                    drone.targetObject = packet.Stage < 3 ? packet.EntityId : 0;
                    drone.movement = droneLogic.player.mecha.droneMovement;
                    if (packet.Stage == 1)
                    {
                        drone.position = player.Movement.GetLastPosition().LocalPlanetPosition.ToVector3();
                    }
                    drone.target = droneLogic._obj_hpos(packet.EntityId);
                    drone.initialVector = drone.position + drone.position.normalized * 4.5f + ((drone.target - drone.position).normalized + UnityEngine.Random.insideUnitSphere) * 1.5f;
                    drone.forward = drone.initialVector;
                    drone.progress = 0f;
                    player.MechaInstance.droneCount = GameMain.mainPlayer.mecha.droneCount;
                    player.MechaInstance.droneSpeed = GameMain.mainPlayer.mecha.droneSpeed;
                    if (packet.Stage == 3)
                    {
                        GameMain.mainPlayer.mecha.droneLogic.serving.Remove(packet.EntityId);
                    }
                    droneLogic.factory = tmpFactory;
                }
            }
        }

        public int GenerateTrashOnPlayer(TrashSystemNewTrashCreatedPacket packet)
        {
            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
                {
                    TrashData trashData = packet.GetTrashData();
                    //Calculate trash position based on the current player's model position
                    RemotePlayerMovement.Snapshot lastPosition = player.Movement.GetLastPosition();
                    if (lastPosition.LocalPlanetId < 1)
                    {
                        trashData.uPos = new VectorLF3(lastPosition.UPosition.x, lastPosition.UPosition.y, lastPosition.UPosition.z);
                    }
                    else
                    {
                        trashData.lPos = lastPosition.LocalPlanetPosition.ToVector3();
                        PlanetData planet = GameMain.galaxy.PlanetById(lastPosition.LocalPlanetId);
                        trashData.uPos = planet.uPosition + (VectorLF3)Maths.QRotate(planet.runtimeRotation, trashData.lPos);
                    }

                    using (Multiplayer.Session.Trashes.NewTrashFromOtherPlayers.On())
                    {
                        int myId = GameMain.data.trashSystem.container.NewTrash(packet.GetTrashObject(), trashData);
                        return myId;
                    }
                }
            }

            return 0;
        }

        public void OnDronesDraw()
        {
            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                foreach (KeyValuePair<ushort, RemotePlayerModel> remoteModel in remotePlayersModels)
                {
                    //Render drones of players only on the local planet
                    if (GameMain.mainPlayer.planetId == remoteModel.Value.Movement.localPlanetId)
                    {
                        remoteModel.Value.MechaInstance.droneRenderer.Draw();
                    }
                }
            }
        }

        public void OnDronesGameTick(float dt)
        {
            double tmp = 1e10; //fake energy of remote player, needed to do the Update()
            double tmp2 = 1;

            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                //Update drones positions based on their targets
                PrebuildData[] prebuildPool = GameMain.localPlanet?.factory?.prebuildPool;

                foreach (KeyValuePair<ushort, RemotePlayerModel> remoteModel in remotePlayersModels)
                {
                    Mecha remoteMecha = remoteModel.Value.MechaInstance;
                    MechaDrone[] drones = remoteMecha.drones;
                    int droneCount = remoteMecha.droneCount;
                    Vector3 remotePosition = remoteModel.Value.Movement.GetLastPosition().LocalPlanetPosition.ToVector3();

                    for (int i = 0; i < droneCount; i++)
                    {
                        //Update only moving drones of players on the same planet
                        if (drones[i].stage != 0 && GameMain.mainPlayer.planetId == remoteModel.Value.Movement.localPlanetId)
                        {
                            if (drones[i].Update(prebuildPool, remotePosition, dt, ref tmp, ref tmp2, 0) != 0)
                            {
                                //Reset drone and release lock
                                drones[i].stage = 3;
                                GameMain.mainPlayer.mecha.droneLogic.serving.Remove(drones[i].targetObject);
                                drones[i].targetObject = 0;
                            }
                        }
                    }
                    remoteMecha.droneRenderer.Update();
                }
            }
        }

        public void RenderPlayerNameTagsOnStarmap(UIStarmap starmap)
        {
            // Make a copy of the "Icarus" text from the starmap
            Text starmap_playerNameText = starmap.playerNameText;
            Transform starmap_playerTrack = starmap.playerTrack;

            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                foreach (KeyValuePair<ushort, RemotePlayerModel> player in remotePlayersModels)
                {
                    RemotePlayerModel playerModel = player.Value;

                    Text nameText;
                    Transform starmapTracker;
                    if (playerModel.StarmapNameText != null && playerModel.StarmapTracker != null)
                    {
                        nameText = playerModel.StarmapNameText;
                        starmapTracker = playerModel.StarmapTracker;
                    }
                    else
                    {
                        // Make an instance of the "Icarus" text to represent the other player name
                        nameText = playerModel.StarmapNameText = GameObject.Instantiate(starmap_playerNameText, starmap_playerNameText.transform.parent);
                        nameText.text = $"{ playerModel.Username }";
                        nameText.gameObject.SetActive(true);

                        // Make an instance the player tracker object
                        starmapTracker = playerModel.StarmapTracker = GameObject.Instantiate(starmap_playerTrack, starmap_playerTrack.parent);
                        starmapTracker.gameObject.SetActive(true);
                    }

                    VectorLF3 adjustedVector;
                    if (playerModel.Movement.localPlanetId > 0)
                    {
                        // Get the position of the planet
                        PlanetData planet = GameMain.galaxy.PlanetById(playerModel.Movement.localPlanetId);
                        adjustedVector = planet.uPosition;

                        // Add the local position of the player
                        Vector3 localPlanetPosition = playerModel.Movement.GetLastPosition().LocalPlanetPosition.ToVector3();
                        adjustedVector += (VectorLF3)Maths.QRotate(planet.runtimeRotation, localPlanetPosition);
                    }
                    else
                    {
                        // Just use the raw uPos as we don't care too much about precise locations
                        adjustedVector = playerModel.Movement.absolutePosition;
                    }

                    // Scale as required
                    adjustedVector = (adjustedVector - starmap.viewTargetUPos) * 0.00025;

                    // Get the point on the screen that represents the world position
                    if (!starmap.WorldPointIntoScreen(adjustedVector, out Vector2 rectPoint))
                    {
                        continue;
                    }

                    // Put the marker directly on the location of the player
                    starmapTracker.position = adjustedVector;

                    if (playerModel.Movement.localPlanetId > 0)
                    {
                        PlanetData planet = GameMain.galaxy.PlanetById(playerModel.Movement.localPlanetId);
                        Quaternion rotation = planet.runtimeRotation *
                            Quaternion.LookRotation(playerModel.PlayerModelTransform.forward, playerModel.Movement.GetLastPosition().LocalPlanetPosition.ToVector3());
                        starmapTracker.rotation = rotation;
                    }
                    else
                    {
                        Quaternion rotation = Quaternion.LookRotation(playerModel.PlayerModelTransform.forward, playerModel.PlayerTransform.localPosition);
                        starmapTracker.rotation = rotation;
                    }

                    starmapTracker.localScale = UIStarmap.isChangingToMilkyWay ? Vector3.zero :
                        Vector3.one * (starmap.screenCamera.transform.position - starmapTracker.position).magnitude;

                    // Put their name above or below it
                    nameText.rectTransform.anchoredPosition = new Vector2(rectPoint.x + (rectPoint.x > 600f ? -35 : 35), rectPoint.y + (rectPoint.y > -350.0 ? -19f : 19f));
                    nameText.gameObject.SetActive(!UIStarmap.isChangingToMilkyWay);
                }
            }
        }

        public void ClearPlayerNameTagsOnStarmap()
        {
            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                foreach (KeyValuePair<ushort, RemotePlayerModel> player in remotePlayersModels)
                {
                    // Destroy the marker and name so they don't linger and cause problems
                    GameObject.Destroy(player.Value.StarmapNameText.gameObject);
                    GameObject.Destroy(player.Value.StarmapTracker.gameObject);

                    // Null them out so they can be recreated next time the map is opened
                    player.Value.StarmapNameText = null;
                    player.Value.StarmapTracker = null;
                }
            }
        }

        public void RenderPlayerNameTagsInGame()
        {
            TextMesh uiSailIndicator_targetText = null;

            using (GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                foreach (KeyValuePair<ushort, RemotePlayerModel> player in remotePlayersModels)
                {
                    RemotePlayerModel playerModel = player.Value;

                    GameObject playerNameText;
                    if (playerModel.InGameNameText != null)
                    {
                        playerNameText = playerModel.InGameNameText;
                    }
                    else
                    {
                        // Only get the field required if we actually need to, no point getting it every time
                        if (uiSailIndicator_targetText == null)
                        {
                            uiSailIndicator_targetText = UIRoot.instance.uiGame.sailIndicator.targetText;
                        }

                        // Initialise a new game object to contain the text
                        playerModel.InGameNameText = playerNameText = new GameObject();
                        // Make it follow the player transform
                        playerNameText.transform.SetParent(playerModel.PlayerTransform, false);
                        // Add a meshrenderer and textmesh component to show the text with a different font
                        MeshRenderer meshRenderer = playerNameText.AddComponent<MeshRenderer>();
                        TextMesh textMesh = playerNameText.AddComponent<TextMesh>();

                        // Set the text to be their name
                        textMesh.text = $"{ playerModel.Username }";
                        // Align it to be centered below them
                        textMesh.anchor = TextAnchor.UpperCenter;
                        // Copy the font over from the sail indicator
                        textMesh.font = uiSailIndicator_targetText.font;
                        meshRenderer.sharedMaterial = uiSailIndicator_targetText.gameObject.GetComponent<MeshRenderer>().sharedMaterial;

                        playerNameText.SetActive(true);
                    }

                    // If the player is not on the same planet or is in space, then do not render their in-world tag
                    if (playerModel.Movement.localPlanetId != Multiplayer.Session.LocalPlayer.Data.LocalPlanetId && playerModel.Movement.localPlanetId <= 0)
                    {
                        playerNameText.SetActive(false);
                    }
                    else if (!playerNameText.activeSelf)
                    {
                        playerNameText.SetActive(true);
                    }

                    // Make sure the text is pointing at the camera
                    playerNameText.transform.rotation = GameCamera.main.transform.rotation;

                    // Resizes the text based on distance from camera for better visual quality
                    float distanceFromCamera = Vector3.Distance(playerNameText.transform.position, GameCamera.main.transform.position);
                    TextMesh nameTextMesh = playerNameText.GetComponent<TextMesh>();

                    if (distanceFromCamera > 100f)
                    {
                        nameTextMesh.characterSize = 0.2f;
                        nameTextMesh.fontSize = 60;
                    }
                    else if (distanceFromCamera > 50f)
                    {
                        nameTextMesh.characterSize = 0.15f;
                        nameTextMesh.fontSize = 48;
                    }
                    else
                    {
                        nameTextMesh.characterSize = 0.1f;
                        nameTextMesh.fontSize = 36;
                    }
                }
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

        public void HidePingIndicator()
        {
            if (pingIndicator != null)
            {
                pingIndicator.enabled = false;
            }
        }

        public void UpdatePingIndicator(string text)
        {
            if (pingIndicator != null)
            {
                pingIndicator.text = text;
            }
        }

        public void SetPauseIndicator(bool canPause)
        {
            //Tell the user if the game is paused or not
            GameObject targetObject = GameObject.Find("UI Root/Overlay Canvas/In Game/Esc Menu/pause-text");
            Text pauseText = targetObject?.GetComponent<Text>();
            Localizer pauseLocalizer = targetObject?.GetComponent<Localizer>();
            if (pauseText && pauseLocalizer)
            {
                if (!canPause)
                {
                    pauseText.text = "--  Nebula Multiplayer  --".Translate();
                    pauseLocalizer.stringKey = "--  Nebula Multiplayer  --".Translate();
                }
                else
                {
                    pauseText.text = "游戏已暂停".Translate();
                    pauseLocalizer.stringKey = "游戏已暂停".Translate();
                }
            }

        }

        public static int GetUniverseObserveLevel()
        {
            int level = 0;
            // the tech ids of the 4 tiers of Universe Exploration from https://dsp-wiki.com/Upgrades
            for (int i = 4104; i >= 4101; i--)
            {
                if (GameMain.history.TechUnlocked(i))
                {
                    // set level to last digit of tech id
                    level = (i % 10);
                    break;
                }
            }
            return level;
        }

        public void SendChatMessage(string text, ChatMessageType messageType)
        {
            ChatManager.Instance?.SendChatMessage(text, messageType);
        }
    }
}

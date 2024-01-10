#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NebulaAPI;
using NebulaAPI.DataStructures;
using NebulaAPI.GameState;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaModel.Packets.Trash;
using NebulaModel.Packets.Universe;
using NebulaModel.Packets.Warning;
using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours;
using NebulaWorld.MonoBehaviours.Local;
using NebulaWorld.MonoBehaviours.Local.Chat;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

#endregion

namespace NebulaWorld;

/// <summary>
///     This class keeps track of our simulated world. It holds all temporary entities like remote player models
///     and also helps to execute some remote player actions that you would want to replicate on the local client.
/// </summary>
public class SimulatedWorld : IDisposable
{
    private readonly ThreadSafe threadSafe = new();
    private LocalPlayerMovement localPlayerMovement;

    private Text pingIndicator;

    private bool IsPlayerJoining { get; set; }

    public void Dispose()
    {
        using (GetRemotePlayersModels(out var remotePlayersModels))
        {
            foreach (var model in remotePlayersModels.Values)
            {
                model.Destroy();
            }

            remotePlayersModels.Clear();
        }

        Object.Destroy(localPlayerMovement);
        SetPauseIndicator(true);
        GC.SuppressFinalize(this);
    }

    public Locker GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels)
    {
        return threadSafe.RemotePlayersModels.GetLocked(out remotePlayersModels);
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

        var player = Multiplayer.Session.LocalPlayer as LocalPlayer;

        // If not a new client, we need to update the player position to put him where he was previously
        if (player is { IsClient: true, IsNewPlayer: false })
        {
            GameMain.mainPlayer.planetId = player.Data.LocalPlanetId;
            if (player.Data.LocalPlanetId == -1)
            {
                GameMain.mainPlayer.uPosition =
                    new VectorLF3(player.Data.UPosition.x, player.Data.UPosition.y, player.Data.UPosition.z);
            }
            else
            {
                GameMain.mainPlayer.position = player.Data.LocalPlanetPosition.ToVector3();
                GameMain.mainPlayer.uPosition = new VectorLF3(GameMain.localPlanet.uPosition.x + GameMain.mainPlayer.position.x,
                    GameMain.localPlanet.uPosition.y + GameMain.mainPlayer.position.y,
                    GameMain.localPlanet.uPosition.z + GameMain.mainPlayer.position.z);
            }
            GameMain.mainPlayer.uRotation = Quaternion.Euler(player.Data.Rotation.ToVector3());

            // Load client's saved data from the last session.
            player.Data.Mecha.UpdateMech(GameMain.mainPlayer);

            // Fix references that broke during import
            GameMain.mainPlayer.mecha.forge.mecha = GameMain.mainPlayer.mecha;
            GameMain.mainPlayer.mecha.forge.player = GameMain.mainPlayer;
            GameMain.mainPlayer.mecha.forge.gameHistory = GameMain.data.history;
            GameMain.mainPlayer.mecha.forge.gameHistory = GameMain.data.history;
            GameMain.mainPlayer.mecha.groundCombatModule.AfterImport(GameMain.data); // do we need to do something about the spaceSector?
            GameMain.mainPlayer.mecha.spaceCombatModule.AfterImport(GameMain.data); // do we need to do something about the spaceSector?
        }

        // Initialization on the host side after game is loaded
        Multiplayer.Session.Factories.InitializePrebuildRequests();

        if (player is { IsClient: true })
        {
            // Update player's Mecha tech bonuses
            ((MechaData)player.Data.Mecha).TechBonuses.UpdateMech(GameMain.mainPlayer.mecha);

            if (player.IsNewPlayer)
            {
                // Set mecha to full energy, shield and hp so new client won't have low stats when starting
                GameMain.mainPlayer.mecha.coreEnergy = GameMain.mainPlayer.mecha.coreEnergyCap;
                GameMain.mainPlayer.mecha.energyShieldEnergy = GameMain.mainPlayer.mecha.energyShieldCapacity;
                GameMain.mainPlayer.mecha.hp = GameMain.mainPlayer.mecha.hpMaxApplied;
                if (GameMain.history.logisticShipWarpDrive)
                {
                    // If warp has unlocked, give new client few warpers
                    GameMain.mainPlayer.TryAddItemToPackage(1210, 5, 0, false);
                }
                // Make new client spawn higher to avoid collision
                var magnitude = GameMain.mainPlayer.transform.localPosition.magnitude;
                if (magnitude > 0)
                {
                    GameMain.mainPlayer.transform.localPosition *= (magnitude + 20f) / magnitude;
                }
            }
            else
            {
                // Prevent old client from dropping into gas gaint
                var planet = GameMain.galaxy.PlanetById(player.Data.LocalPlanetId);
                if (planet != null)
                {
                    var altitude = GameMain.mainPlayer.transform.localPosition.magnitude - planet.realRadius;
                    if (altitude > 5f || planet.type == EPlanetType.Gas)
                    {
                        GameMain.mainPlayer.movementState = EMovementState.Fly;
                    }
                }
            }

            // Refresh Logistics Distributor traffic for player delivery package changes
            GameMain.mainPlayer.factory?.transport.RefreshDispenserTraffic();

            // Enable Ping Indicator for Clients
            DisplayPingIndicator();

            // Notify the server that we are done loading the game
            var clientCert = CryptoUtils.GetPublicKey(CryptoUtils.GetOrCreateUserCert());
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
            if (player != null)
            {
                player.Data.Mecha.SandCount = GameMain.mainPlayer.sandCount;
            }
        }
        // Set the name of local player in starmap from Icarus to user name
        if (player != null)
        {
            GameMain.mainPlayer.mecha.appearance.overrideName = " " + player.Data.Username + " ";
        }
        // Finally we need add the local player components to the player character
        localPlayerMovement = GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerMovement>();
        // ChatManager should continuous exsit until the game is closed
        GameMain.mainPlayer.gameObject.AddComponentIfMissing<ChatManager>();
    }

    public void OnPlayerJoining(string username)
    {
        if (IsPlayerJoining)
        {
            return;
        }
        IsPlayerJoining = true;
        Multiplayer.Session.CanPause = true;
        GameMain.isFullscreenPaused = true;
        InGamePopup.ShowInfo("Loading".Translate(),
            string.Format("{0} joining the game, please wait\n(Use BulletTime mod to unfreeze the game)".Translate(),
                username), null);
    }

    public static void OnPlayerJoinedGame(INebulaPlayer player)
    {
        Multiplayer.Session.World.SpawnRemotePlayerModel(player.Data);

        // Load overriden Planet and Star names
        player.SendPacket(new NameInputPacket(GameMain.galaxy));

        // add together player sand count and tell others if we are syncing soil
        if (Config.Options.SyncSoil)
        {
            GameMain.mainPlayer.sandCount += player.Data.Mecha.SandCount;
            Multiplayer.Session.Network.SendPacket(new PlayerSandCount(GameMain.mainPlayer.sandCount));
        }
        // Reset local and remote chargers ids to recalculate and broadcast the current ids to new player
        Multiplayer.Session.PowerTowers.ResetAndBroadcast();

        // (Host only) Trigger when a new client added to connected players
        Log.Info($"Client{player.Data.PlayerId} - {player.Data.Username} joined");
        try
        {
            NebulaModAPI.OnPlayerJoinedGame?.Invoke(player.Data);
        }
        catch (Exception e)
        {
            Log.Error("NebulaModAPI.OnPlayerJoinedGame error:\n" + e);
        }
    }

    public static void OnPlayerLeftGame(INebulaPlayer player)
    {
        Multiplayer.Session.World.DestroyRemotePlayerModel(player.Id);

        if (Config.Options.SyncSoil)
        {
            GameMain.mainPlayer.sandCount -= player.Data.Mecha.SandCount;
            UIRoot.instance.uiGame.OnSandCountChanged(GameMain.mainPlayer.sandCount, -player.Data.Mecha.SandCount);
            Multiplayer.Session.Network.SendPacket(new PlayerSandCount(GameMain.mainPlayer.sandCount));
        }
        // Reset local and remote chargers ids to remove the ids used by the disconnected player
        Multiplayer.Session.PowerTowers.ResetAndBroadcast();

        // (Host only) Trigger when a connected client leave the game
        Log.Info($"Client{player.Data.PlayerId} - {player.Data.Username} left");
        try
        {
            NebulaModAPI.OnPlayerLeftGame?.Invoke(player.Data);
        }
        catch (Exception e)
        {
            Log.Error("NebulaModAPI.OnPlayerLeftGame error:\n" + e);
        }
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
        using (GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (remotePlayersModels.ContainsKey(playerData.PlayerId))
            {
                return;
            }
            Log.Info($"Spawn player model {playerData.PlayerId} {playerData.Username}");
            var model = new RemotePlayerModel(playerData.PlayerId, playerData.Username);
            remotePlayersModels.Add(playerData.PlayerId, model);

            // Show conneted message
            var planetname = GameMain.galaxy.PlanetById(playerData.LocalPlanetId)?.displayName ?? "In space";
            var message = string.Format("[{0:HH:mm}] {1} connected ({2})".Translate(), DateTime.Now, playerData.Username,
                planetname);
            SendChatMessage(message);
        }
    }

    public void DestroyRemotePlayerModel(ushort playerId)
    {
        using (GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (!remotePlayersModels.TryGetValue(playerId, out var player))
            {
                return;
            }
            // Show disconnected message
            var message = string.Format("[{0:HH:mm}] {1} disconnected".Translate(), DateTime.Now, player.Username);
            SendChatMessage(message);

            player.Destroy();
            remotePlayersModels.Remove(playerId);
            if (remotePlayersModels.Count == 0 && Config.Options.AutoPauseEnabled)
            {
                Multiplayer.Session.CanPause = true;
            }
        }
    }

    public void UpdateRemotePlayerRealtimeState(PlayerMovement packet)
    {
        using (GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (!remotePlayersModels.TryGetValue(packet.PlayerId, out var player))
            {
                return;
            }
            player.Movement.UpdatePosition(packet);
            player.Animator.UpdateState(packet);
        }
    }

    public int GenerateTrashOnPlayer(TrashSystemNewTrashCreatedPacket packet)
    {
        using (GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (!remotePlayersModels.TryGetValue(packet.PlayerId, out var player))
            {
                return 0;
            }
            var trashData = packet.GetTrashData();
            //Calculate trash position based on the current player's model position
            var lastPosition = player.Movement.GetLastPosition();
            if (lastPosition.LocalPlanetId < 1)
            {
                trashData.uPos = new VectorLF3(lastPosition.UPosition.x, lastPosition.UPosition.y,
                    lastPosition.UPosition.z);
            }
            else
            {
                trashData.lPos = lastPosition.LocalPlanetPosition.ToVector3();
                var planet = GameMain.galaxy.PlanetById(lastPosition.LocalPlanetId);
                trashData.uPos = planet.uPosition + (VectorLF3)Maths.QRotate(planet.runtimeRotation, trashData.lPos);
            }

            using (Multiplayer.Session.Trashes.NewTrashFromOtherPlayers.On())
            {
                var myId = GameMain.data.trashSystem.container.NewTrash(packet.GetTrashObject(), trashData);
                return myId;
            }
        }
    }

    public void OnDronesDraw()
    {
        using (GetRemotePlayersModels(out var remotePlayersModels))
        {
            //todo:replace
            //foreach (var remoteModel in remotePlayersModels.Where(remoteModel =>
            //             GameMain.mainPlayer.planetId == remoteModel.Value.Movement.localPlanetId))
            //{
            //    //remoteModel.Value.MechaInstance.droneRenderer.Draw();
            //}
        }
    }

    public void OnDronesGameTick(float dt)
    {
        var tmp = 1e10; //fake energy of remote player, needed to do the Update()
        double tmp2 = 1;

        using (GetRemotePlayersModels(out var remotePlayersModels))
        {
            //Update drones positions based on their targets
            var prebuildPool = GameMain.localPlanet?.factory?.prebuildPool;

            foreach (var remoteModel in remotePlayersModels)
            {
                var remoteMecha = remoteModel.Value.MechaInstance;
                //todo:replace
                /*var drones = remoteMecha.drones;
                var droneCount = remoteMecha.droneCount;
                var remotePosition = remoteModel.Value.Movement.GetLastPosition().LocalPlanetPosition.ToVector3();

                for (var i = 0; i < droneCount; i++)
                {
                    //Update only moving drones of players on the same planet
                    if (drones[i].stage == 0 || GameMain.mainPlayer.planetId != remoteModel.Value.Movement.localPlanetId)
                    {
                        continue;
                    }
                    if (drones[i].Update(prebuildPool, remotePosition, dt, ref tmp, ref tmp2, 0) == 0)
                    {
                        continue;
                    }
                    //Reset drone and release lock
                    drones[i].stage = 3;
                    GameMain.mainPlayer.mecha.droneLogic.serving.Remove(drones[i].targetObject);
                    drones[i].targetObject = 0;
                }
                remoteMecha.droneRenderer.Update();*/
            }
        }
    }

    public void RenderPlayerNameTagsOnStarmap(UIStarmap starmap)
    {
        // Make a copy of the "Icarus" text from the starmap
        var starmap_playerNameText = starmap.playerNameText;
        var starmap_playerTrack = starmap.playerTrack;

        using (GetRemotePlayersModels(out var remotePlayersModels))
        {
            foreach (var playerModel in remotePlayersModels.Select(player => player.Value))
            {
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
                    nameText = playerModel.StarmapNameText =
                        Object.Instantiate(starmap_playerNameText, starmap_playerNameText.transform.parent);
                    nameText.text = $"{playerModel.Username}";
                    nameText.gameObject.SetActive(true);

                    // Make an instance the player tracker object
                    starmapTracker = playerModel.StarmapTracker =
                        Object.Instantiate(starmap_playerTrack, starmap_playerTrack.parent);
                    starmapTracker.gameObject.SetActive(true);
                }

                VectorLF3 adjustedVector;
                if (playerModel.Movement.localPlanetId > 0)
                {
                    // Get the position of the planet
                    var planet = GameMain.galaxy.PlanetById(playerModel.Movement.localPlanetId);
                    adjustedVector = planet.uPosition;

                    // Add the local position of the player
                    var localPlanetPosition = playerModel.Movement.GetLastPosition().LocalPlanetPosition.ToVector3();
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
                if (!starmap.WorldPointIntoScreen(adjustedVector, out var rectPoint))
                {
                    continue;
                }

                // Put the marker directly on the location of the player
                starmapTracker.position = adjustedVector;

                if (playerModel.Movement.localPlanetId > 0)
                {
                    var planet = GameMain.galaxy.PlanetById(playerModel.Movement.localPlanetId);
                    var rotation = planet.runtimeRotation *
                                   Quaternion.LookRotation(playerModel.PlayerModelTransform.forward,
                                       playerModel.Movement.GetLastPosition().LocalPlanetPosition.ToVector3());
                    starmapTracker.rotation = rotation;
                }
                else
                {
                    var rotation = Quaternion.LookRotation(playerModel.PlayerModelTransform.forward,
                        playerModel.PlayerTransform.localPosition);
                    starmapTracker.rotation = rotation;
                }

                starmapTracker.localScale = UIStarmap.isChangingToMilkyWay
                    ? Vector3.zero
                    : Vector3.one * (starmap.screenCamera.transform.position - starmapTracker.position).magnitude;

                // Put their name above or below it
                nameText.rectTransform.anchoredPosition = new Vector2(rectPoint.x + (rectPoint.x > 600f ? -35 : 35),
                    rectPoint.y + (rectPoint.y > -350.0 ? -19f : 19f));
                nameText.gameObject.SetActive(!UIStarmap.isChangingToMilkyWay);
            }
        }
    }

    public void ClearPlayerNameTagsOnStarmap()
    {
        using (GetRemotePlayersModels(out var remotePlayersModels))
        {
            foreach (var player in remotePlayersModels)
            {
                // Destroy the marker and name so they don't linger and cause problems
                Object.Destroy(player.Value.StarmapNameText.gameObject);
                Object.Destroy(player.Value.StarmapTracker.gameObject);

                // Null them out so they can be recreated next time the map is opened
                player.Value.StarmapNameText = null;
                player.Value.StarmapTracker = null;
            }
        }
    }

    public void RenderPlayerNameTagsInGame()
    {
        TextMesh uiSailIndicator_targetText = null;

        using (GetRemotePlayersModels(out var remotePlayersModels))
        {
            foreach (var playerModel in remotePlayersModels.Select(player => player.Value))
            {
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
                    var meshRenderer = playerNameText.AddComponent<MeshRenderer>();
                    var textMesh = playerNameText.AddComponent<TextMesh>();

                    // Set the text to be their name
                    textMesh.text = $"{playerModel.Username}";
                    // Align it to be centered below them
                    textMesh.anchor = TextAnchor.UpperCenter;
                    // Copy the font over from the sail indicator
                    textMesh.font = uiSailIndicator_targetText.font;
                    meshRenderer.sharedMaterial =
                        uiSailIndicator_targetText.gameObject.GetComponent<MeshRenderer>().sharedMaterial;

                    playerNameText.SetActive(true);
                }

                // If the player is not on the same planet or is in space, then do not render their in-world tag
                if (playerModel.Movement.localPlanetId != Multiplayer.Session.LocalPlayer.Data.LocalPlanetId &&
                    playerModel.Movement.localPlanetId <= 0)
                {
                    playerNameText.SetActive(false);
                }
                else if (!playerNameText.activeSelf)
                {
                    playerNameText.SetActive(true);
                }

                // Make sure the text is pointing at the camera
                var transform = GameCamera.main.transform;
                playerNameText.transform.rotation = transform.rotation;

                // Resizes the text based on distance from camera for better visual quality
                var distanceFromCamera =
                    Vector3.Distance(playerNameText.transform.position, transform.position);
                var nameTextMesh = playerNameText.GetComponent<TextMesh>();

                switch (distanceFromCamera)
                {
                    case > 100f:
                        nameTextMesh.characterSize = 0.2f;
                        nameTextMesh.fontSize = 60;
                        break;
                    case > 50f:
                        nameTextMesh.characterSize = 0.15f;
                        nameTextMesh.fontSize = 48;
                        break;
                    default:
                        nameTextMesh.characterSize = 0.1f;
                        nameTextMesh.fontSize = 36;
                        break;
                }
            }
        }
    }

    private void DisplayPingIndicator()
    {
        var previousObject = GameObject.Find("Ping Indicator");
        if (previousObject == null)
        {
            var targetObject = GameObject.Find("label");
            pingIndicator = Object.Instantiate(targetObject, UIRoot.instance.uiGame.gameObject.transform).GetComponent<Text>();
            pingIndicator.gameObject.name = "Ping Indicator";
            pingIndicator.alignment = TextAnchor.UpperLeft;
            pingIndicator.enabled = true;
            var rect = pingIndicator.GetComponent<RectTransform>();
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

    public static void SetPauseIndicator(bool canPause)
    {
        //Tell the user if the game is paused or not
        var targetObject = GameObject.Find("UI Root/Overlay Canvas/In Game/Esc Menu/pause-text");
        if (targetObject == null)
        {
            return;
        }
        var pauseText = targetObject.GetComponent<Text>();
        var pauseLocalizer = targetObject.GetComponent<Localizer>();
        if (!pauseText || !pauseLocalizer)
        {
            return;
        }
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

    public static int GetUniverseObserveLevel()
    {
        var level = 0;
        // the tech ids of the 4 tiers of Universe Exploration from https://dsp-wiki.com/Upgrades
        for (var i = 4104; i >= 4101; i--)
        {
            if (!GameMain.history.TechUnlocked(i))
            {
                continue;
            }
            // set level to last digit of tech id
            level = i % 10;
            break;
        }
        return level;
    }

    private static void SendChatMessage(string text, ChatMessageType messageType = ChatMessageType.SystemInfoMessage)
    {
        ChatManager.Instance.SendChatMessage(text, messageType);
    }

    private sealed class ThreadSafe
    {
        internal readonly Dictionary<ushort, RemotePlayerModel> RemotePlayersModels = new();
    }
}

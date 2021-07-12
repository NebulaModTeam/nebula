using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Trash;
using NebulaWorld.Factory;
using NebulaWorld.Logistics;
using NebulaWorld.MonoBehaviours.Remote;
using NebulaWorld.Planet;
using NebulaWorld.Player;
using NebulaWorld.Trash;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NebulaWorld
{
    /// <summary>
    /// This class keeps track of our simulated world. It holds all temporary entities like remote player models 
    /// and also helps to execute some remote player actions that you would want to replicate on the local client.
    /// </summary>
    public static class SimulatedWorld
    {
        sealed class ThreadSafe
        {
            internal readonly Dictionary<ushort, RemotePlayerModel> remotePlayersModels = new Dictionary<ushort, RemotePlayerModel>();
        }

        private static readonly ThreadSafe threadSafe = new ThreadSafe();

        public static Locker GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels) =>
            threadSafe.remotePlayersModels.GetLocked(out remotePlayersModels);

        public static bool Initialized { get; private set; }
        public static bool IsGameLoaded { get; private set; }
        public static bool IsPlayerJoining { get; set; }
        public static bool ExitingMultiplayerSession { get; set; }

        public static void Initialize()
        {
            StationUIManager.Initialize();
            ILSShipManager.Initialize();
            DroneManager.Initialize();
            FactoryManager.Initialize();
            PlanetManager.Initialize();
            Initialized = true;
            ExitingMultiplayerSession = false;

            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                remotePlayersModels.Clear();
            }
        }

        /// <summary>
        /// Removes any simulated entities that was added to the scene for a game.
        /// </summary>
        public static void Clear()
        {
            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                foreach (var model in remotePlayersModels.Values)
                {
                    model.Destroy();
                }

                remotePlayersModels.Clear();
            }

            Initialized = false;
            IsGameLoaded = false;
            IsPlayerJoining = false;
        }

        public static void OnPlayerJoining()
        {
            if (!IsPlayerJoining)
            {
                IsPlayerJoining = true;
                GameMain.isFullscreenPaused = true;
                InGamePopup.ShowInfo("Loading", "Player joining the game, please wait", null);
            }
        }

        public static void OnAllPlayersSyncCompleted()
        {
            IsPlayerJoining = false;
            InGamePopup.FadeOut();
            GameMain.isFullscreenPaused = false;
        }

        public static void UpdateGameState(GameState state)
        {
            // We allow for a small drift of 5 ticks since the tick offset using the ping is only an approximation
            if (GameMain.gameTick > 0 && Mathf.Abs(state.gameTick - GameMain.gameTick) > 5)
            {
                Log.Info($"Game Tick got updated since it was desynced, was {GameMain.gameTick}, received {state.gameTick}");
                GameMain.gameTick = state.gameTick;
            }
        }

        public static void SpawnRemotePlayerModel(PlayerData playerData)
        {
            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                if (!remotePlayersModels.ContainsKey(playerData.PlayerId))
                {
                    RemotePlayerModel model = new RemotePlayerModel(playerData.PlayerId, playerData.Username);
                    remotePlayersModels.Add(playerData.PlayerId, model);
                }
            }

            UpdatePlayerColor(playerData.PlayerId, playerData.MechaColor);
        }

        public static void DestroyRemotePlayerModel(ushort playerId)
        {
            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                if (remotePlayersModels.TryGetValue(playerId, out RemotePlayerModel player))
                {
                    player.Destroy();
                    remotePlayersModels.Remove(playerId);
                }
            }
        }

        public static void UpdateRemotePlayerPosition(PlayerMovement packet)
        {
            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
                {
                    player.Movement.UpdatePosition(packet);
                }
            }
        }

        public static void UpdateRemotePlayerAnimation(PlayerAnimationUpdate packet)
        {
            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
                {
                    player.Animator.UpdateState(packet);
                    player.Effects.UpdateState(packet);
                }
            }
        }

        public static void UpdateRemotePlayerWarpState(PlayerUseWarper packet)
        {
            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                if (packet.PlayerId == 0) packet.PlayerId = 1; // host sends himself as PlayerId 0 but clients see him as id 1
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

        public static void UpdateRemotePlayerDrone(NewDroneOrderPacket packet)
        {
            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
                {
                    //Setup drone of remote player based on the drone data
                    ref MechaDrone drone = ref player.PlayerInstance.mecha.drones[packet.DroneId];
                    MechaDroneLogic droneLogic = player.PlayerInstance.mecha.droneLogic;
                    var tmpFactory = droneLogic.factory;

                    droneLogic.factory = GameMain.galaxy.PlanetById(packet.PlanetId).factory;

                    // factory can sometimes be null when transitioning to or from a planet, in this case we do not want to continue
                    if(droneLogic.factory == null)
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
                    drone.target = (Vector3)MethodInvoker.GetHandler(AccessTools.Method(typeof(MechaDroneLogic), "_obj_hpos", new System.Type[] { typeof(int) })).Invoke(GameMain.mainPlayer.mecha.droneLogic, packet.EntityId);
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

        public static void UpdatePlayerColor(ushort playerId, Float3 color)
        {
            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                Transform transform;
                RemotePlayerModel remotePlayerModel;
                if (playerId == LocalPlayer.PlayerId)
                {
                    transform = GameMain.data.mainPlayer.transform;
                }
                else if (remotePlayersModels.TryGetValue(playerId, out remotePlayerModel))
                {
                    transform = remotePlayerModel.PlayerTransform;
                }
                else
                {
                    Log.Error("Could not find the transform for player with ID " + playerId);
                    return;
                }

                Log.Info($"Changing color of player {playerId} to {color}");

                // Apply new color to each part of the mecha
                SkinnedMeshRenderer[] componentsInChildren = transform.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (Renderer r in componentsInChildren)
                {
                    if (r.material?.name.StartsWith("icarus-armor", System.StringComparison.Ordinal) ?? false)
                    {
                        r.material.SetColor("_Color", color.ToColor());
                    }
                }

                // We changed our own color, so we have to let others know
                if (LocalPlayer.PlayerId == playerId)
                {
                    LocalPlayer.SendPacket(new PlayerColorChanged(playerId, color));
                }
            }
        }

        public static void OnILSShipUpdate(ILSShipData packet)
        {
            if (packet.idleToWork)
            {
                ILSShipManager.IdleShipGetToWork(packet);
            }
            else
            {
                ILSShipManager.WorkShipBackToIdle(packet);
            }
        }

        public static void OnILSShipItemsUpdate(ILSShipItems packet)
        {
            ILSShipManager.AddTakeItem(packet);
        }

        public static void OnStationUIChange(StationUI packet)
        {
            StationUIManager.UpdateUI(packet);
        }

        public static void OnILSRemoteOrderUpdate(ILSRemoteOrderData packet)
        {
            ILSShipManager.UpdateRemoteOrder(packet);
        }

        public static void OnVegetationMined(VegeMinedPacket packet)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            if (packet.Amount == 0 && factory != null)
            {
                if (packet.IsVein)
                {
                    VeinData veinData = factory.GetVeinData(packet.VegeId);
                    VeinProto veinProto = LDB.veins.Select((int)veinData.type);

                    factory.RemoveVeinWithComponents(packet.VegeId);

                    if (veinProto != null)
                    {
                        VFEffectEmitter.Emit(veinProto.MiningEffect, veinData.pos, Maths.SphericalRotation(veinData.pos, 0f));
                        VFAudio.Create(veinProto.MiningAudio, null, veinData.pos, true);
                    }
                }
                else
                {
                    VegeData vegeData = factory.GetVegeData(packet.VegeId);
                    VegeProto vegeProto = LDB.veges.Select((int)vegeData.protoId);

                    factory.RemoveVegeWithComponents(packet.VegeId);

                    if (vegeProto != null)
                    {
                        VFEffectEmitter.Emit(vegeProto.MiningEffect, vegeData.pos, Maths.SphericalRotation(vegeData.pos, 0f));
                        VFAudio.Create(vegeProto.MiningAudio, null, vegeData.pos, true);
                    }
                }
            }
            else if (factory != null)
            {
                VeinData veinData = factory.GetVeinData(packet.VegeId);
                PlanetData.VeinGroup[] veinGroups = factory.planet.veinGroups;
                short groupIndex = veinData.groupIndex;

                // must be a vein/oil patch (i think the game treats them same now as oil patches can run out too)
                factory.veinPool[packet.VegeId].amount = packet.Amount;
                factory.planet.veinAmounts[(int)veinData.type] -= 1L;
                veinGroups[(int)groupIndex].amount = veinGroups[(int)groupIndex].amount - 1L;
            }
            else
            {
                Debug.Log("Received VegeMinedPacket but could not do as i was told :C");
            }
        }

        public static int GenerateTrashOnPlayer(TrashSystemNewTrashCreatedPacket packet)
        {
            using (GetRemotePlayersModels(out var remotePlayersModels))
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

                    using (TrashManager.NewTrashFromOtherPlayers.On())
                    {
                        int myId = GameMain.data.trashSystem.container.NewTrash(packet.GetTrashObject(), trashData);

                        return myId;
                    }
                }
            }

            return 0;
        }

        public static void OnGameLoadCompleted()
        {
            if (Initialized == false)
                return;

            Log.Info("Game has finished loading");

            // Assign our own color
            UpdatePlayerColor(LocalPlayer.PlayerId, LocalPlayer.Data.MechaColor);

            // Change player location from spawn to the last known
            VectorLF3 UPosition = new VectorLF3(LocalPlayer.Data.UPosition.x, LocalPlayer.Data.UPosition.y, LocalPlayer.Data.UPosition.z);
            if (UPosition != VectorLF3.zero)
            {
                GameMain.mainPlayer.planetId = LocalPlayer.Data.LocalPlanetId;
                if (LocalPlayer.Data.LocalPlanetId == -1)
                {
                    GameMain.mainPlayer.uPosition = UPosition;
                }
                else
                {
                    GameMain.mainPlayer.position = LocalPlayer.Data.LocalPlanetPosition.ToVector3();
                    GameMain.mainPlayer.uPosition = new VectorLF3(GameMain.localPlanet.uPosition.x + GameMain.mainPlayer.position.x, GameMain.localPlanet.uPosition.y + GameMain.mainPlayer.position.y, GameMain.localPlanet.uPosition.z + GameMain.mainPlayer.position.z);
                }
                GameMain.mainPlayer.uRotation = Quaternion.Euler(LocalPlayer.Data.Rotation.ToVector3());

                //Load player's saved data from the last session.
                AccessTools.Property(typeof(global::Player), "package").SetValue(GameMain.mainPlayer, LocalPlayer.Data.Mecha.Inventory, null);
                GameMain.mainPlayer.mecha.forge = LocalPlayer.Data.Mecha.Forge;
                GameMain.mainPlayer.mecha.coreEnergy = LocalPlayer.Data.Mecha.CoreEnergy;
                GameMain.mainPlayer.mecha.reactorEnergy = LocalPlayer.Data.Mecha.ReactorEnergy;
                GameMain.mainPlayer.mecha.reactorStorage = LocalPlayer.Data.Mecha.ReactorStorage;
                GameMain.mainPlayer.mecha.warpStorage = LocalPlayer.Data.Mecha.WarpStorage;
                GameMain.mainPlayer.SetSandCount(LocalPlayer.Data.Mecha.SandCount);

                //Fix references that brokes during import
                AccessTools.Property(typeof(MechaForge), "mecha").SetValue(GameMain.mainPlayer.mecha.forge, GameMain.mainPlayer.mecha, null);
                AccessTools.Property(typeof(MechaForge), "player").SetValue(GameMain.mainPlayer.mecha.forge, GameMain.mainPlayer, null);
                GameMain.mainPlayer.mecha.forge.gameHistory = GameMain.data.history;
            }

            //Update player's Mecha tech bonuses
            if (!LocalPlayer.IsMasterClient)
            {
                LocalPlayer.Data.Mecha.TechBonuses.UpdateMech(GameMain.mainPlayer.mecha);
            }

            //Initialization on the host side after game is loaded
            FactoryManager.InitializePrebuildRequests();

            LocalPlayer.SetReady();

            IsGameLoaded = true;
        }

        public static void OnDronesDraw()
        {
            using (GetRemotePlayersModels(out var remotePlayersModels))
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

        public static void OnDronesGameTick(long time, float dt)
        {
            double tmp = 1e10; //fake energy of remote player, needed to do the Update()
            double tmp2 = 1;

            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                //Update drones positions based on their targets
                var prebuildPool = GameMain.localPlanet?.factory?.prebuildPool;

                foreach (KeyValuePair<ushort, RemotePlayerModel> remoteModel in remotePlayersModels)
                {
                    Mecha remoteMecha = remoteModel.Value.MechaInstance;
                    MechaDrone[] drones = remoteMecha.drones;
                    int droneCount = remoteMecha.droneCount;
                    var remotePosition = remoteModel.Value.Movement.GetLastPosition().LocalPlanetPosition.ToVector3();

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

        public static void RenderPlayerNameTagsOnStarmap(UIStarmap starmap)
        {
            // Make a copy of the "Icarus" text from the starmap
            Text starmap_playerNameText = (Text)AccessTools.Field(typeof(UIStarmap), "playerNameText").GetValue(starmap);
            Transform starmap_playerTrack = (Transform)AccessTools.Field(typeof(UIStarmap), "playerTrack").GetValue(starmap);

            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                foreach (var player in remotePlayersModels)
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
                        var rotation = planet.runtimeRotation * 
                            Quaternion.LookRotation(playerModel.PlayerModelTransform.forward, playerModel.Movement.GetLastPosition().LocalPlanetPosition.ToVector3());
                        starmapTracker.rotation = rotation;
                    }
                    else
                    {
                        var rotation = Quaternion.LookRotation(playerModel.PlayerModelTransform.forward, playerModel.PlayerTransform.localPosition);
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

        public static void ClearPlayerNameTagsOnStarmap()
        {
            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                foreach (var player in remotePlayersModels)
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

        public static void RenderPlayerNameTagsInGame()
        {
            TextMesh uiSailIndicator_targetText = null;

            using (GetRemotePlayersModels(out var remotePlayersModels))
            {
                foreach (var player in remotePlayersModels)
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
                            uiSailIndicator_targetText = (TextMesh)AccessTools.Field(typeof(UISailIndicator), "targetText").GetValue(UIRoot.instance.uiGame.sailIndicator);
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
                    if (playerModel.Movement.localPlanetId != LocalPlayer.Data.LocalPlanetId && playerModel.Movement.localPlanetId <= 0)
                    {
                        playerNameText.gameObject.SetActive(false);
                    }
                    else if (!playerNameText.gameObject.activeSelf)
                    {
                        playerNameText.gameObject.SetActive(true);
                    }

                    // Make sure the text is pointing at the camera
                    playerNameText.transform.rotation = GameCamera.main.transform.rotation;

                    // Resizes the text based on distance from camera for better visual quality
                    var distanceFromCamera = Vector3.Distance(playerNameText.transform.position, GameCamera.main.transform.position);
                    var nameTextMesh = playerNameText.GetComponent<TextMesh>();

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
    }
}

using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Universe;
using System;
using System.Collections.Generic;

namespace NebulaWorld.Universe
{
    public class DysonSphereManager : IDisposable
    {
        private sealed class ThreadSafe
        {
            internal readonly Dictionary<int, List<INebulaConnection>> Subscribers = new Dictionary<int, List<INebulaConnection>>();
        }
        private readonly ThreadSafe threadSafe = new ThreadSafe();

        public Locker GetSubscribers(out Dictionary<int, List<INebulaConnection>> subscribers)
        {
            return threadSafe.Subscribers.GetLocked(out subscribers);
        }

        public readonly ToggleSwitch IsIncomingRequest = new ToggleSwitch();
        public readonly ToggleSwitch IncomingDysonSwarmPacket = new ToggleSwitch();
        public bool IsNormal { get; set; } = true; //Client side: is the spheres data normal or desynced
        public bool InBlueprint { get; set; } = false; //In the processing of importing blueprint
        public int RequestingIndex { get; set; } = -1; //StarIndex of the dyson sphere requesting

        private readonly List<DysonSphereStatusPacket> statusPackets = new List<DysonSphereStatusPacket>(); //Server side

        public DysonSphereManager()
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Send packet to all Clients that subscribe to target Dyson Sphere 
        /// </summary>
        public void SendPacketToDysonSphere<T>(T packet, int starIndex) where T : class, new()
        {
            using (GetSubscribers(out Dictionary<int, List<INebulaConnection>> subscribers))
            {
                if (subscribers.ContainsKey(starIndex))
                {
                    foreach (INebulaConnection conn in subscribers[starIndex])
                    {
                        conn.SendPacket(packet);
                    }
                }
            }
        }

        public void SendPacketToDysonSphereExcept<T>(T packet, int starIndex, INebulaConnection exception) where T : class, new()
        {
            using (GetSubscribers(out Dictionary<int, List<INebulaConnection>> subscribers))
            {
                if (subscribers.ContainsKey(starIndex))
                {
                    foreach (INebulaConnection conn in subscribers[starIndex])
                    {
                        if (!conn.Equals(exception))
                        {
                            conn.SendPacket(packet);
                        }
                    }
                }
            }
        }

        public void RegisterPlayer(INebulaConnection conn, int starIndex)
        {
            using (GetSubscribers(out Dictionary<int, List<INebulaConnection>> subscribers))
            {
                if (!subscribers.ContainsKey(starIndex))
                {
                    subscribers.Add(starIndex, new List<INebulaConnection>());
                    statusPackets.Add(new DysonSphereStatusPacket(GameMain.data.dysonSpheres[starIndex]));
                    Multiplayer.Session.Launch.Register(starIndex);
                }
                if (!subscribers[starIndex].Contains(conn))
                {
                    subscribers[starIndex].Add(conn);
                    conn.SendPacket(statusPackets.Find(x => x.StarIndex == starIndex));
                }
            }
        }

        public void UnRegisterPlayer(INebulaConnection conn, int starIndex)
        {
            using (GetSubscribers(out Dictionary<int, List<INebulaConnection>> subscribers))
            {
                if (subscribers.ContainsKey(starIndex))
                {
                    subscribers[starIndex].Remove(conn);
                    if (subscribers[starIndex].Count == 0)
                    {
                        subscribers.Remove(starIndex);
                        statusPackets.Remove(statusPackets.Find(x => x.StarIndex == starIndex));
                        Multiplayer.Session.Launch.Unregister(starIndex);
                    }
                }
            }
        }

        public void UnRegisterPlayer(INebulaConnection conn)
        {
            using (GetSubscribers(out Dictionary<int, List<INebulaConnection>> subscribers))
            {
                List<int> removals = new List<int>();
                foreach (KeyValuePair<int, List<INebulaConnection>> item in subscribers)
                {
                    item.Value.Remove(conn);
                    if (item.Value.Count == 0)
                    {
                        removals.Add(item.Key);
                    }
                }
                foreach (int starIndex in removals)
                {
                    subscribers.Remove(starIndex);
                    statusPackets.Remove(statusPackets.Find(x => x.StarIndex == starIndex));
                    Multiplayer.Session.Launch.Unregister(starIndex);
                }
            }
        }

        public void UpdateSphereStatusIfNeeded()
        {                            
            foreach (DysonSphereStatusPacket packet in statusPackets)
            {
                DysonSphere dysonSphere = GameMain.data.dysonSpheres[packet.StarIndex];
                //Update dyson sphere when the status changes
                if (packet.GrossRadius != dysonSphere.grossRadius || packet.EnergyReqCurrentTick != dysonSphere.energyReqCurrentTick || packet.EnergyGenCurrentTick != dysonSphere.energyGenCurrentTick)
                {
                    packet.GrossRadius = dysonSphere.grossRadius;
                    packet.EnergyReqCurrentTick = dysonSphere.energyReqCurrentTick;
                    packet.EnergyGenCurrentTick = dysonSphere.energyGenCurrentTick;
                    SendPacketToDysonSphere(packet, packet.StarIndex);
                }
            }
        }

        public void RequestDysonSphere(int starIndex, bool showInfo = true)
        {
            StarData starData = GameMain.galaxy.stars[starIndex];
            RequestingIndex = starIndex;
            Log.Info($"Requesting DysonSphere for system {starData.displayName} (Index: {starData.index})");
            Multiplayer.Session.Network.SendPacket(new DysonSphereLoadRequest(starData.index, DysonSphereRequestEvent.Load));
            ClearSelection(starIndex);
            if (showInfo)
            {
                InGamePopup.ShowInfo("Loading", $"Loading Dyson sphere {starData.displayName}, please wait...", null);
            }
        }

        public void UnloadRemoteDysonSpheres()
        {
            //The editor will throw errors if there are no dyson spheres available
            int currentId = GameMain.localStar?.index ?? UIRoot.instance.uiGame.dysonEditor.selection.viewStar?.index ?? -1;
            for (int i = 0; i < GameMain.data.dysonSpheres.Length; i++)
            {
                if (GameMain.data.dysonSpheres[i] != null && i != currentId)
                {
                    Log.Info($"Unload DysonSphere at system {GameMain.galaxy.stars[i].displayName} (Index: {i})");
                    Multiplayer.Session.Network.SendPacket(new DysonSphereLoadRequest(i, DysonSphereRequestEvent.Unload));
                    GameMain.data.dysonSpheres[i] = null;
                }
            }
            IsNormal = true;
        }

        public void HandleDesync(int starIndex, INebulaConnection conn)
        {
            if (Multiplayer.Session.LocalPlayer.IsHost)
            {
                //Notify packet author that its building request did not success
                conn.SendPacket(new DysonSphereData(starIndex, Array.Empty<byte>(), DysonSphereRespondEvent.Desync));
            }
            else
            {
                //Notify client that desync happened
                if (IsNormal)
                {
                    IsNormal = false;
                    InGamePopup.ShowWarning("Desync", $"Dyson sphere id[{starIndex}] {GameMain.galaxy.stars[starIndex].displayName} is desynced.",
                        "Reload", () => RequestDysonSphere(starIndex));
                }
            }
        }

        public static int QueryOrbitId(DysonSwarm swarm)
        {
            //Return the next available orbit Id
            int orbitId = swarm.orbitCursor <= 20 ? swarm.orbitCursor : -1;
            for (int i = 1; i < swarm.orbitCursor; i++)
            {
                if (swarm.orbits[i].id == 0)
                {
                    orbitId = i;
                    break;
                }
            }
            return orbitId;
        }

        public static void ClearSelection(int starIndex, int layerId = -1)
        {
            DESelection selection = UIRoot.instance.uiGame.dysonEditor.selection;
            if (selection.viewStar!= null && selection.viewStar.index == starIndex)
            {
                if (layerId == -1)
                {
                    selection.ClearAllSelection();
                }
                else if (selection.IsLayerSelected(layerId))
                {
                    selection.ClearComponentSelection();
                }
            }
        }
    }
}

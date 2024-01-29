#region

using System;
using System.Collections.Generic;
using System.Linq;
using NebulaAPI.DataStructures;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Universe;

#endregion

namespace NebulaWorld.Universe;

public class DysonSphereManager : IDisposable
{
    public readonly ToggleSwitch IncomingDysonSwarmPacket = new();

    public readonly ToggleSwitch IsIncomingRequest = new();

    private readonly List<DysonSphereStatusPacket> statusPackets = []; //Server side
    private readonly ThreadSafe threadSafe = new();

    public bool IsNormal { get; set; } = true; //Client side: is the spheres data normal or desynced
    public bool InBlueprint { get; set; } //In the processing of importing blueprint
    public int RequestingIndex { get; set; } = -1; //StarIndex of the dyson sphere requesting

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private Locker GetSubscribers(out Dictionary<int, List<INebulaConnection>> subscribers)
    {
        return threadSafe.Subscribers.GetLocked(out subscribers);
    }

    /// <summary>
    ///     Send packet to all Clients that subscribe to target Dyson Sphere
    /// </summary>
    public void SendPacketToDysonSphere<T>(T packet, int starIndex) where T : class, new()
    {
        using (GetSubscribers(out var subscribers))
        {
            if (!subscribers.TryGetValue(starIndex, out var value))
            {
                return;
            }
            foreach (var conn in value)
            {
                conn.SendPacket(packet);
            }
        }
    }

    public void SendPacketToDysonSphereExcept<T>(T packet, int starIndex, INebulaConnection exception) where T : class, new()
    {
        using (GetSubscribers(out var subscribers))
        {
            if (!subscribers.TryGetValue(starIndex, out var value))
            {
                return;
            }
            foreach (var conn in value.Where(conn => !conn.Equals(exception)))
            {
                conn.SendPacket(packet);
            }
        }
    }

    public void RegisterPlayer(INebulaConnection conn, int starIndex)
    {
        using var locker = GetSubscribers(out var subscribers);
        if (!subscribers.TryGetValue(starIndex, out var value))
        {
            value = [];
            subscribers.Add(starIndex, value);
            statusPackets.Add(new DysonSphereStatusPacket(GameMain.data.dysonSpheres[starIndex]));
            Multiplayer.Session.Launch.Register(starIndex);
        }
        if (value.Contains(conn))
        {
            return;
        }
        value.Add(conn);
        conn.SendPacket(statusPackets.Find(x => x.StarIndex == starIndex));
    }

    public void UnRegisterPlayer(INebulaConnection conn, int starIndex)
    {
        using var locker = GetSubscribers(out var subscribers);
        if (!subscribers.TryGetValue(starIndex, out var value))
        {
            return;
        }

        value.Remove(conn);
        if (value.Count != 0)
        {
            return;
        }
        subscribers.Remove(starIndex);
        statusPackets.Remove(statusPackets.Find(x => x.StarIndex == starIndex));
        Multiplayer.Session.Launch.Unregister(starIndex);
    }

    public void UnRegisterPlayer(INebulaConnection conn)
    {
        using var locker = GetSubscribers(out var subscribers);
        var removals = new List<int>();
        foreach (var item in subscribers)
        {
            item.Value.Remove(conn);
            if (item.Value.Count == 0)
            {
                removals.Add(item.Key);
            }
        }
        foreach (var starIndex in removals)
        {
            subscribers.Remove(starIndex);
            statusPackets.Remove(statusPackets.Find(x => x.StarIndex == starIndex));
            Multiplayer.Session.Launch.Unregister(starIndex);
        }
    }

    public void UpdateSphereStatusIfNeeded()
    {
        foreach (var packet in statusPackets)
        {
            var dysonSphere = GameMain.data.dysonSpheres[packet.StarIndex];
            //Update dyson sphere when the status changes
            if (Math.Abs(packet.GrossRadius - dysonSphere.grossRadius) < 0.000000001 &&
                packet.EnergyReqCurrentTick == dysonSphere.energyReqCurrentTick &&
                packet.EnergyGenCurrentTick == dysonSphere.energyGenCurrentTick)
            {
                continue;
            }
            packet.GrossRadius = dysonSphere.grossRadius;
            packet.EnergyReqCurrentTick = dysonSphere.energyReqCurrentTick;
            packet.EnergyGenCurrentTick = dysonSphere.energyGenCurrentTick;
            SendPacketToDysonSphere(packet, packet.StarIndex);
        }
    }

    public void RequestDysonSphere(int starIndex, bool showInfo = true)
    {
        var starData = GameMain.galaxy.stars[starIndex];
        RequestingIndex = starIndex;
        Log.Info($"Requesting DysonSphere for system {starData.displayName} (Index: {starData.index})");
        Multiplayer.Session.Network.SendPacket(new DysonSphereLoadRequest(starData.index, DysonSphereRequestEvent.Load));
        ClearSelection(starIndex);
        if (showInfo)
        {
            InGamePopup.ShowInfo("Loading".Translate(),
                string.Format("Loading Dyson sphere {0}, please wait".Translate(), starData.displayName), null);
        }
    }

    public void UnloadRemoteDysonSpheres()
    {
        //The editor will throw errors if there are no dyson spheres available
        var currentId = GameMain.localStar?.index ?? UIRoot.instance.uiGame.dysonEditor.selection.viewStar?.index ?? -1;
        for (var i = 0; i < GameMain.data.dysonSpheres.Length; i++)
        {
            if (GameMain.data.dysonSpheres[i] == null || i == currentId)
            {
                continue;
            }
            Log.Info($"Unload DysonSphere at system {GameMain.galaxy.stars[i].displayName} (Index: {i})");
            Multiplayer.Session.Network.SendPacket(new DysonSphereLoadRequest(i, DysonSphereRequestEvent.Unload));
            GameMain.data.dysonSpheres[i] = null;
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
            if (!IsNormal)
            {
                return;
            }
            IsNormal = false;
            InGamePopup.ShowWarning("Desync".Translate(),
                string.Format("Dyson sphere id[{0}] {1} is desynced.".Translate(), starIndex,
                    GameMain.galaxy.stars[starIndex].displayName),
                "Reload".Translate(), () => RequestDysonSphere(starIndex));
        }
    }

    public static int QueryOrbitId(DysonSwarm swarm)
    {
        //Return the next available orbit Id
        var orbitId = swarm.orbitCursor <= 20 ? swarm.orbitCursor : -1;
        for (var i = 1; i < swarm.orbitCursor; i++)
        {
            if (swarm.orbits[i].id != 0)
            {
                continue;
            }
            orbitId = i;
            break;
        }
        return orbitId;
    }

    public static void ClearSelection(int starIndex, int layerId = -1)
    {
        var selection = UIRoot.instance.uiGame.dysonEditor.selection;
        if (selection.viewStar == null || selection.viewStar.index != starIndex)
        {
            return;
        }
        if (layerId == -1)
        {
            selection.ClearAllSelection();
        }
        else if (selection.IsLayerSelected(layerId))
        {
            selection.ClearComponentSelection();
        }
    }

    private sealed class ThreadSafe
    {
        internal readonly Dictionary<int, List<INebulaConnection>> Subscribers = new();
    }
}

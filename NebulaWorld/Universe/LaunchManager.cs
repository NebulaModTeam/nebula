#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Universe;
using UnityEngine;

#endregion

namespace NebulaWorld.Universe;

public class LaunchManager
{
    private HashSet<int> planetIds = []; //Client side

    public ConcurrentBag<DysonLaunchData.Projectile> ProjectileBag { get; set; } = [];
    public ConcurrentDictionary<int, DysonLaunchData> Snapshots { get; set; } = new();

    public void Dispose()
    {
        ProjectileBag = null;
        Snapshots = null;
        planetIds = null;
    }

    public void Register(int starIndex)
    {
        if (Snapshots.IsEmpty)
        {
            // Clear remaining projectile data in bag
            while (ProjectileBag.TryTake(out _)) { }
        }
        Snapshots.TryAdd(starIndex, new DysonLaunchData(starIndex));
    }

    public void Unregister(int starIndex)
    {
        Snapshots.TryRemove(starIndex, out _);
    }

    public void CollectProjectile()
    {
        if (Snapshots.IsEmpty)
        {
            return;
        }
        // Try to take out all projectiles produced in this tick
        while (ProjectileBag.TryTake(out var data))
        {
            var starId = data.PlanetId / 100 - 1;
            if (!Snapshots.TryGetValue(starId, out var snapshot))
            {
                // If the dyson sphere has no subscribers anymore, skip this data
                continue;
            }
            // bullet: targetId is orbitId, range 1~40
            if (data.TargetId <= 40)
            {
                data.Interval = snapshot.BulletTick;
                snapshot.BulletList.Add(data);
                snapshot.BulletTick = 0;
            }
            // rocket: targetId is layerId(4 bits) + nodeId(12 bits)
            else
            {
                data.Interval = snapshot.RocketTick;
                snapshot.RocketList.Add(data);
                snapshot.RocketTick = 0;
            }
        }

        // Increase tick counter
        foreach (var snapshot in Snapshots.Values)
        {
            snapshot.BulletTick++;
            snapshot.RocketTick++;
        }
    }

    public void SendBroadcastIfNeeded()
    {
        if (Snapshots.IsEmpty)
        {
            return;
        }

        //Broadcast the update packet to subscribers
        foreach (var snapshot in Snapshots.Values)
        {
            if (snapshot.BulletList.Count > 0 || snapshot.RocketList.Count > 0)
            {
                Multiplayer.Session.DysonSpheres.SendPacketToDysonSphere(new DysonLaunchDataPacket(snapshot),
                    snapshot.StarIndex);
            }
        }

        // Clear snapshots
        foreach (var snapshot in Snapshots.Values)
        {
            snapshot.BulletList.Clear();
            snapshot.RocketList.Clear();
            snapshot.BulletTick = 0;
            snapshot.RocketTick = 0;
        }
    }

    public void ImportPacket(DysonLaunchDataPacket packet)
    {
        Snapshots[packet.Data.StarIndex] = packet.Data;

        // Update planetId set of loaded factories
        planetIds.Clear();
        for (var i = 0; i < GameMain.data.factoryCount; i++)
        {
            planetIds.Add(GameMain.data.factories[i].planetId);
        }
    }

    public void LaunchProjectile()
    {
        var discardTarget = -1;
        foreach (var snapshot in Snapshots.Values)
        {
            var sphere = GameMain.data.dysonSpheres[snapshot.StarIndex];
            if (sphere == null)
            {
                discardTarget = snapshot.StarIndex;
                continue;
            }
            while (snapshot.BulletCursor < snapshot.BulletList.Count)
            {
                if (snapshot.BulletList[snapshot.BulletCursor].Interval <= snapshot.BulletTick)
                {
                    // Only fire when the planet factory isn't loaded 
                    if (!planetIds.Contains(snapshot.BulletList[snapshot.BulletCursor].PlanetId))
                    {
                        AddBullet(sphere.swarm, snapshot.BulletList[snapshot.BulletCursor]);
                    }
                    snapshot.BulletCursor++;
                    snapshot.BulletTick = 0;
                }
                else
                {
                    snapshot.BulletTick++;
                    break;
                }
            }
            while (snapshot.RocketCursor < snapshot.RocketList.Count)
            {
                if (snapshot.RocketList[snapshot.RocketCursor].Interval <= snapshot.RocketTick)
                {
                    // Only fire when the planet factory isn't loaded
                    if (!planetIds.Contains(snapshot.RocketList[snapshot.RocketCursor].PlanetId))
                    {
                        AddRocket(sphere, snapshot.RocketList[snapshot.RocketCursor]);
                    }
                    snapshot.RocketCursor++;
                    snapshot.RocketTick = 0;
                }
                else
                {
                    snapshot.RocketTick++;
                    break;
                }
            }
            if (snapshot.BulletCursor == snapshot.BulletList.Count && snapshot.RocketCursor == snapshot.RocketList.Count)
            {
                // snapshot is empty, can be removed now
                discardTarget = snapshot.StarIndex;
            }
        }
        if (discardTarget >= 0)
        {
            Snapshots.TryRemove(discardTarget, out _);
        }
    }

    private static void AddBullet(DysonSwarm swarm, DysonLaunchData.Projectile projectile)
    {
        ref var astroPoses = ref GameMain.data.galaxy.astrosData;
        int orbitId = projectile.TargetId;
        if (!swarm.OrbitExist(orbitId))
        {
            return;
        }
        var starPos = astroPoses[projectile.PlanetId / 100 * 100].uPos;
        SailBullet bullet = default;
        bullet.lBegin = projectile.LocalPos;
        bullet.uBegin = astroPoses[projectile.PlanetId].uPos +
                        Maths.QRotateLF(astroPoses[projectile.PlanetId].uRot, projectile.LocalPos);
        bullet.uEnd = starPos + VectorLF3.Cross(swarm.orbits[orbitId].up, starPos - bullet.uBegin).normalized *
            swarm.orbits[orbitId].radius;
        bullet.maxt = (float)((bullet.uEnd - bullet.uBegin).magnitude / 4000.0);
        bullet.uEndVel = VectorLF3.Cross(bullet.uEnd - starPos, swarm.orbits[orbitId].up).normalized *
                         Math.Sqrt(swarm.dysonSphere.gravity / swarm.orbits[orbitId].radius);
        swarm.AddBullet(bullet, orbitId);
    }

    private static void AddRocket(DysonSphere sphere, DysonLaunchData.Projectile projectile)
    {
        ref var astroPoses = ref GameMain.data.galaxy.astrosData;
        // Assume layerId < 16, nodeId < 4096
        var layerId = projectile.TargetId >> 12;
        var nodeId = projectile.TargetId & 0x0FFF;
        var node = sphere.FindNode(layerId, nodeId);
        if (node == null)
        {
            return;
        }
        DysonRocket rocket = default;
        rocket.planetId = projectile.PlanetId;
        rocket.uPos = astroPoses[projectile.PlanetId].uPos + Maths.QRotateLF(astroPoses[projectile.PlanetId].uRot,
            projectile.LocalPos + projectile.LocalPos.normalized * 6.1f);
        rocket.uRot = astroPoses[projectile.PlanetId].uRot * Maths.SphericalRotation(projectile.LocalPos, 0f) *
                      Quaternion.Euler(-90f, 0f, 0f);
        rocket.uVel = rocket.uRot * Vector3.forward;
        rocket.uSpeed = 0f;
        rocket.launch = projectile.LocalPos.normalized;
        sphere.AddDysonRocket(rocket, node);
    }
}

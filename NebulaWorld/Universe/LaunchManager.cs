using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Universe;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaWorld.Universe
{
    public class LaunchManager
    {
        public bool IsUpdateNeeded { get; private set; }
        public ConcurrentBag<DysonLaunchData.Projectile> ProjectileBag { get; private set; }

        private List<DysonLaunchData> snapshots;
        private ConcurrentDictionary<INebulaConnection, int> subscribers;
        private List<int> planetIds;

        public LaunchManager()
        {
            ProjectileBag = new ConcurrentBag<DysonLaunchData.Projectile>();
            snapshots = new List<DysonLaunchData>();
            subscribers = new ConcurrentDictionary<INebulaConnection, int>();
            planetIds = new List<int>();
        }

        public void Dispose()
        {
            ProjectileBag = null;
            snapshots = null;
            subscribers = null;
            planetIds = null;
        }
           
        public void RegisterPlayer(INebulaConnection conn, int starIndex)
        {
            // TODO: Manage starIndex in list in future?
            subscribers.AddOrUpdate(conn, starIndex, (c,s) => starIndex);
            if (!IsUpdateNeeded)
            {
                // Clear sanpshots
                foreach (DysonLaunchData snapshot in snapshots)
                {
                    snapshot.BulletList.Clear();
                    snapshot.RocketList.Clear();
                    snapshot.BulletTick = 0;
                    snapshot.RocketTick = 0;
                }
                // Clear remaining projectile data in bag
                while (ProjectileBag.TryTake(out _))
                    ;
                IsUpdateNeeded = true;
            }
        }

        public void UnRegisterPlayer(INebulaConnection conn)
        {
            if (subscribers.TryRemove(conn, out _) && subscribers.IsEmpty)
            {
                IsUpdateNeeded = false;
            }
        }

        public void CollectProjectile()
        {            
            if (!IsUpdateNeeded)
            {
                return;
            }
            // Try to take out all projectiles produced in this tick
            while (ProjectileBag.TryTake(out DysonLaunchData.Projectile data))
            {
                int starId = data.PlanetId / 100 - 1;
                DysonLaunchData snapshot = snapshots.Find(x => x.StarIndex == starId);
                if (snapshot == null)
                {
                    snapshot = new DysonLaunchData(starId);
                    snapshots.Add(snapshot);
                }
                // bullet: targetId is orbitId, range 1~20
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
            foreach (DysonLaunchData snapshot in snapshots)
            {
                snapshot.BulletTick++;
                snapshot.RocketTick++;
            }
        }

        public void SendBroadcastIfNeeded()
        {
            if (!IsUpdateNeeded)
            {
                return;
            }
            //Export and prepare update packet
            DysonLaunchDataPacket packet;
            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                int count = 0;
                foreach (DysonLaunchData snapshot in snapshots)
                {
                    if (snapshot.BulletList.Count > 0 || snapshot.RocketList.Count > 0)
                    {
                        snapshot.Export(writer.BinaryWriter);
                        count++;
                    }
                }
                packet = new DysonLaunchDataPacket(count, writer.CloseAndGetBytes());
            }

            //Broadcast the update packet to subscribers
            if (packet.Count > 0)
            {
                foreach (KeyValuePair<INebulaConnection, int> subscriber in subscribers)
                {
                    subscriber.Key.SendPacket(packet);
                }
            }

            // Clear sanpshots
            foreach (DysonLaunchData snapshot in snapshots)
            {
                snapshot.BulletList.Clear();
                snapshot.RocketList.Clear();
                snapshot.BulletTick = 0;
                snapshot.RocketTick = 0;
            }
        }

        public void ImportPacket(DysonLaunchDataPacket packet)
        {
            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.BinaryData))
            {
                for (int i = 0; i < packet.Count; i++)
                {
                    DysonLaunchData snapshot = new DysonLaunchData();
                    snapshot.Import(reader.BinaryReader);
                    snapshots.Add(snapshot);
                }
            }

            // Update planetId list of loaded factories
            planetIds.Clear();
            for (int i = 0; i < GameMain.data.factoryCount; i++)
            {
                planetIds.Add(GameMain.data.factories[i].planetId);
            }
            planetIds.Sort();
        }

        public void LaunchProjectile()
        {
            DysonLaunchData discardTarget = null;
            foreach (DysonLaunchData snapshot in snapshots)
            {
                DysonSphere sphere = GameMain.data.dysonSpheres[snapshot.StarIndex];
                if (sphere == null)
                {
                    discardTarget = snapshot;
                    continue;
                }
                while (snapshot.BulletCursor < snapshot.BulletList.Count)
                {
                    if (snapshot.BulletList[snapshot.BulletCursor].Interval <= snapshot.BulletTick)
                    {
                        // Only fire when the planet factory isn't loaded 
                        if (planetIds.BinarySearch(snapshot.BulletList[snapshot.BulletCursor].PlanetId) < 0)
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
                        if (planetIds.BinarySearch(snapshot.RocketList[snapshot.RocketCursor].PlanetId) < 0)
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
                    discardTarget = snapshot;
                }
            }
            if (discardTarget != null)
            {
                snapshots.Remove(discardTarget);
            }
        }

        private static void AddBullet(DysonSwarm swarm,DysonLaunchData.Projectile projectile)
        {
            ref AstroPose[] astroPoses = ref GameMain.data.galaxy.astroPoses;
            int orbitId = projectile.TargetId; 
            if (swarm.OrbitExist(orbitId))
            {
                VectorLF3 starPos = astroPoses[projectile.PlanetId / 100 * 100].uPos;
                SailBullet bullet = default;
                bullet.lBegin = projectile.LocalPos;
                bullet.uBegin = astroPoses[projectile.PlanetId].uPos + Maths.QRotateLF(astroPoses[projectile.PlanetId].uRot, projectile.LocalPos);
                bullet.uEnd = starPos + VectorLF3.Cross(swarm.orbits[orbitId].up, starPos - bullet.uBegin).normalized * swarm.orbits[orbitId].radius;
                bullet.maxt = (float)((bullet.uEnd - bullet.uBegin).magnitude / 4000.0);
                bullet.uEndVel = VectorLF3.Cross(bullet.uEnd - starPos, swarm.orbits[orbitId].up).normalized * Math.Sqrt(swarm.dysonSphere.gravity / swarm.orbits[orbitId].radius);
                swarm.AddBullet(bullet, orbitId);
            }
        }

        private static void AddRocket(DysonSphere sphere, DysonLaunchData.Projectile projectile)
        {
            ref AstroPose[] astroPoses = ref GameMain.data.galaxy.astroPoses;
            // Assume layerId < 16, nodeId < 4096
            int layerId = projectile.TargetId >> 12;
            int nodeId = projectile.TargetId & 0x0FFF;
            DysonNode node = sphere.FindNode(layerId, nodeId);
            if (node != null) 
            {
                DysonRocket rocket = default;
                rocket.planetId = projectile.PlanetId;
                rocket.uPos = astroPoses[projectile.PlanetId].uPos + Maths.QRotateLF(astroPoses[projectile.PlanetId].uRot, projectile.LocalPos + projectile.LocalPos.normalized * 6.1f);
                rocket.uRot = astroPoses[projectile.PlanetId].uRot * Maths.SphericalRotation(projectile.LocalPos, 0f) * Quaternion.Euler(-90f, 0f, 0f);
                rocket.uVel = rocket.uRot * Vector3.forward;
                rocket.uSpeed = 0f;
                rocket.launch = projectile.LocalPos.normalized;
                sphere.AddDysonRocket(rocket, node);
            }
        }
    }
}

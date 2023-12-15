#region

using System.Collections.Generic;
using NebulaAPI.DataStructures;
using NebulaAPI.Interfaces;
using NebulaAPI.Packets;
using UnityEngine;

#endregion

namespace NebulaModel.DataStructures;

[RegisterNestedType]
public class DysonLaunchData(int starIndex) : INetSerializable
{
    public DysonLaunchData() : this(0) { }

    public int StarIndex { get; set; } = starIndex;
    public List<Projectile> BulletList { get; set; } = [];
    public List<Projectile> RocketList { get; set; } = [];
    public int BulletCursor { get; set; }
    public int RocketCursor { get; set; }
    public byte BulletTick { get; set; }
    public byte RocketTick { get; set; }

    public void Serialize(INetDataWriter writer)
    {
        writer.Put(StarIndex);
        writer.Put((ushort)BulletList.Count);
        for (ushort i = 0; i < (ushort)BulletList.Count; i++)
        {
            var data = BulletList[i];
            writer.Put((byte)(data.PlanetId % 100));
            writer.Put(data.Interval);
            writer.Put(data.TargetId);
            writer.Put(data.LocalPos.ToFloat3());
        }
        writer.Put((ushort)RocketList.Count);
        for (ushort i = 0; i < (ushort)RocketList.Count; i++)
        {
            var data = RocketList[i];
            writer.Put((byte)(data.PlanetId % 100));
            writer.Put(data.Interval);
            writer.Put(data.TargetId);
            writer.Put(data.LocalPos.ToFloat3());
        }
    }

    public void Deserialize(INetDataReader reader)
    {
        StarIndex = reader.GetInt();
        var starId = (StarIndex + 1) * 100;
        int count = reader.GetUShort();
        BulletList = new List<Projectile>(count);
        for (var i = 0; i < count; i++)
        {
            var data = new Projectile
            {
                PlanetId = reader.GetByte() + starId,
                Interval = reader.GetByte(),
                TargetId = reader.GetUShort(),
                LocalPos = reader.GetFloat3().ToVector3()
            };
            BulletList.Add(data);
        }
        count = reader.GetUShort();
        RocketList = new List<Projectile>(count);
        for (ushort i = 0; i < count; i++)
        {
            var data = new Projectile
            {
                PlanetId = reader.GetByte() + starId,
                Interval = reader.GetByte(),
                TargetId = reader.GetUShort(),
                LocalPos = reader.GetFloat3().ToVector3()
            };
            RocketList.Add(data);
        }
    }

    public struct Projectile
    {
        public int PlanetId;
        public byte Interval;
        public ushort TargetId;
        public Vector3 LocalPos;
    }
}

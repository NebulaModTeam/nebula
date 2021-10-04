using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NebulaModel.DataStructures
{
    public class DysonLaunchData
    {
        public struct Projectile
        {            
            public int PlanetId;
            public byte Interval;
            public ushort TargetId;
            public Vector3 LocalPos;
        }

        public int StarIndex { get; set; }
        public List<Projectile> BulletList { get; set; }
        public List<Projectile> RocketList { get; set; }
        public int BulletCursor { get; set; }
        public int RocketCursor { get; set; }
        public byte BulletTick { get; set; }
        public byte RocketTick { get; set; }

        public DysonLaunchData(int starIndex)
        {
            StarIndex = starIndex;
            BulletList = new List<Projectile>();
            RocketList = new List<Projectile>();
        }

        public DysonLaunchData() : this(0) { }

        public void Export(BinaryWriter bw)
        {
            bw.Write(StarIndex);
            bw.Write((ushort)BulletList.Count);
            for (ushort i = 0; i < (ushort)BulletList.Count; i++)
            {
                Projectile data = BulletList[i];
                bw.Write((byte)(data.PlanetId % 100));
                bw.Write(data.Interval);
                bw.Write(data.TargetId);
                bw.Write(data.LocalPos.x);
                bw.Write(data.LocalPos.y);
                bw.Write(data.LocalPos.z);
            }
            bw.Write((ushort)RocketList.Count);
            for (ushort i = 0; i < (ushort)RocketList.Count; i++)
            {
                Projectile data = RocketList[i];
                bw.Write((byte)(data.PlanetId % 100));
                bw.Write(data.Interval);
                bw.Write(data.TargetId);
                bw.Write(data.LocalPos.x);
                bw.Write(data.LocalPos.y);
                bw.Write(data.LocalPos.z);
            }
        }

        public void Import(BinaryReader br)
        {
            StarIndex = br.ReadInt32();
            int starId = (StarIndex + 1) * 100;
            int count = br.ReadUInt16();
            BulletList = new List<Projectile>(count);
            for (int i = 0; i < count; i++)
            {
                Projectile data = new Projectile
                {
                    PlanetId = br.ReadByte() + starId,
                    Interval = br.ReadByte(),
                    TargetId = br.ReadUInt16(),
                    LocalPos = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle())
                };
                BulletList.Add(data);
            }
            count = br.ReadUInt16();
            RocketList = new List<Projectile>(count);
            for (ushort i = 0; i < count; i++)
            {
                Projectile data = new Projectile
                {
                    PlanetId = br.ReadByte() + starId,
                    Interval = br.ReadByte(),
                    TargetId = br.ReadUInt16(),
                    LocalPos = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle())
                };
                RocketList.Add(data);
            }
        }
    }
}

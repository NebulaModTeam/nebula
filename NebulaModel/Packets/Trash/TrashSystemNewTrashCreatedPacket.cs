using NebulaAPI;

namespace NebulaModel.Packets.Trash
{
    public class TrashSystemNewTrashCreatedPacket
    {
        public int TrashId { get; set; }
        public Float3 RPos { get; set; }
        public Float3 LPos { get; set; }
        public Double3 UPos { get; set; }
        public Float4 RRot { get; set; }
        public Float4 LRot { get; set; }
        public Float4 URot { get; set; }
        public Double3 UVel { get; set; }
        public Float3 UAgl { get; set; }
        public short Item { get; set; }
        public byte Count { get; set; }
        public int Expire { get; set; }
        public int LandPlanetId { get; set; }
        public int NearPlanetId { get; set; }
        public int NearStarId { get; set; }
        public double NearStarGravity { get; set; }
        public ushort PlayerId { get; set; }
        public int LocalPlanetId { get; set; }

        public TrashSystemNewTrashCreatedPacket() { }
        public TrashSystemNewTrashCreatedPacket(int trashId, TrashObject trashObj, TrashData trashData, ushort playerId, int localPlanetId)
        {
            TrashId = trashId;
            RPos = new Float3(trashObj.rPos);
            LPos = new Float3(trashData.lPos);
            LocalPlanetId = localPlanetId;
            PlayerId = playerId;
            UPos = new Double3(trashData.uPos.x, trashData.uPos.y, trashData.uPos.z);
            RRot = new Float4(trashObj.rRot);
            LRot = new Float4(trashData.lRot);
            URot = new Float4(trashData.uRot);
            UVel = new Double3(trashData.uVel.x, trashData.uVel.y, trashData.uVel.z);
            UAgl = new Float3(trashData.uAgl);
            Item = (short)trashObj.item;
            Count = (byte)trashObj.count;
            Expire = trashObj.expire;
            LandPlanetId = trashData.landPlanetId;
            NearPlanetId = trashData.nearPlanetId;
            NearStarId = trashData.nearStarId;
            NearStarGravity = trashData.nearStarGravity;
        }

        public TrashObject GetTrashObject()
        {
            TrashObject result = new TrashObject();
            result.rPos = RPos.ToVector3();
            result.rRot = RRot.ToQuaternion();
            result.item = Item;
            result.count = Count;
            result.expire = Expire;
            return result;
        }

        public TrashData GetTrashData()
        {
            TrashData result = new TrashData();
            result.lPos = LPos.ToVector3();
            result.uPos = new VectorLF3(UPos.x, UPos.y, UPos.z);
            result.lRot = LRot.ToQuaternion();
            result.uRot = URot.ToQuaternion();
            result.uVel = new VectorLF3(UVel.x, UVel.y, UVel.z);
            result.uAgl = UAgl.ToVector3();
            result.landPlanetId = LandPlanetId;
            result.nearPlanetId = NearPlanetId;
            result.nearStarId = NearStarId;
            result.nearStarGravity = NearStarGravity;
            return result;
        }
    }
}

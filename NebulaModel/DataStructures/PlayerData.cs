using NebulaModel.Attributes;
using NebulaModel.Networking.Serialization;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public class PlayerData : INetSerializable
    {
        public ushort PlayerId { get; set; }
        public int LocalPlanetId { get; set; }
        public Float3 Color { get; set; }
        public Float3 LocalPlanetPosition { get; set; }
        public Double3 UPosition { get; set; }
        public Float3 Rotation { get; set; }
        public Float3 BodyRotation { get; set; }
        public MechaData Mecha { get; set; }
        public int LocalStarId { get; set; }

        public PlayerData() { }
        public PlayerData(ushort playerId, int localPlanetId, Float3 color, Float3 localPlanetPosition = new Float3(), Double3 position = new Double3(), Float3 rotation = new Float3(), Float3 bodyRotation = new Float3())
        {
            PlayerId = playerId;
            LocalPlanetId = localPlanetId;
            LocalPlanetPosition = localPlanetPosition;
            Color = color;
            UPosition = position;
            Rotation = rotation;
            BodyRotation = bodyRotation;
            Mecha = new MechaData();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(LocalPlanetId);
            Color.Serialize(writer);
            LocalPlanetPosition.Serialize(writer);
            UPosition.Serialize(writer);
            Rotation.Serialize(writer);
            BodyRotation.Serialize(writer);
            Mecha.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetUShort();
            LocalPlanetId = reader.GetInt();
            Color = reader.GetFloat3();
            LocalPlanetPosition = reader.GetFloat3();
            UPosition = reader.GetDouble3();
            Rotation = reader.GetFloat3();
            BodyRotation = reader.GetFloat3();
            Mecha = new MechaData();
            Mecha.Deserialize(reader);
        }

        public PlayerData CreateCopyWithoutMechaData()
        {
            return new PlayerData(PlayerId, LocalPlanetId, Color, LocalPlanetPosition, UPosition, Rotation, BodyRotation);
        }
    }
}

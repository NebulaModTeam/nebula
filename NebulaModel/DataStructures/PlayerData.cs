using NebulaAPI;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public class PlayerData : IPlayerData
    {
        public string Username { get; set; }
        public ushort PlayerId { get; set; }
        public int LocalPlanetId { get; set; }
        public MechaAppearance MechaAppearance { get; set; }
        public Float3 LocalPlanetPosition { get; set; }
        public Double3 UPosition { get; set; }
        public Float3 Rotation { get; set; }
        public Float3 BodyRotation { get; set; }
        public IMechaData Mecha { get; set; }
        public int LocalStarId { get; set; }

        public PlayerData() { }
        public PlayerData(ushort playerId, int localPlanetId, MechaAppearance mechaAppearance, string username = null, Float3 localPlanetPosition = new Float3(), Double3 position = new Double3(), Float3 rotation = new Float3(), Float3 bodyRotation = new Float3())
        {
            PlayerId = playerId;
            LocalPlanetId = localPlanetId;
            MechaAppearance = mechaAppearance;
            Username = !string.IsNullOrWhiteSpace(username) ? username : $"Player {playerId}";
            LocalPlanetPosition = localPlanetPosition;
            UPosition = position;
            Rotation = rotation;
            BodyRotation = bodyRotation;
            Mecha = new MechaData();
        }

        public void Serialize(INetDataWriter writer)
        {
            writer.Put(Username);
            writer.Put(PlayerId);
            writer.Put(LocalPlanetId);

            var appearanceArray = MechaAppearance.ToByte();
            writer.Put(appearanceArray.Length);
            writer.Put(appearanceArray);

            LocalPlanetPosition.Serialize(writer);
            UPosition.Serialize(writer);
            Rotation.Serialize(writer);
            BodyRotation.Serialize(writer);
            Mecha.Serialize(writer);
        }

        public void Deserialize(INetDataReader reader)
        {
            Username = reader.GetString();
            PlayerId = reader.GetUShort();
            LocalPlanetId = reader.GetInt();

            var appearanceArray = new byte[reader.GetInt()];
            reader.GetBytes(appearanceArray, appearanceArray.Length);
            MechaAppearance = new MechaAppearance();
            MechaAppearance.Init();
            MechaAppearance.FromByte(appearanceArray);

            LocalPlanetPosition = reader.GetFloat3();
            UPosition = reader.GetDouble3();
            Rotation = reader.GetFloat3();
            BodyRotation = reader.GetFloat3();
            Mecha = new MechaData();
            Mecha.Deserialize(reader);
        }

        public IPlayerData CreateCopyWithoutMechaData()
        {
            return new PlayerData(PlayerId, LocalPlanetId, MechaAppearance, Username, LocalPlanetPosition, UPosition, Rotation, BodyRotation);
        }
    }
}

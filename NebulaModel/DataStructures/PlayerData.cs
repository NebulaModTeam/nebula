using NebulaAPI;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public class PlayerData : IPlayerData
    {
        public const ushort REVISION = 3;
        public string Username { get; set; }
        public ushort PlayerId { get; set; }
        public int LocalPlanetId { get; set; }
        public Float4[] MechaColors { get; set; }
        public Float3 LocalPlanetPosition { get; set; }
        public Double3 UPosition { get; set; }
        public Float3 Rotation { get; set; }
        public Float3 BodyRotation { get; set; }
        public IMechaData Mecha { get; set; }
        public int LocalStarId { get; set; }
        public ushort Revision { get; set; } = REVISION;

        public PlayerData() { }
        public PlayerData(ushort playerId, int localPlanetId, Float4[] mechaColors, string username = null, Float3 localPlanetPosition = new Float3(), Double3 position = new Double3(), Float3 rotation = new Float3(), Float3 bodyRotation = new Float3())
        {
            PlayerId = playerId;
            LocalPlanetId = localPlanetId;
            MechaColors = mechaColors;
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
            writer.Put(MechaColors?.Length ?? 0);
            for (int i = 0; i < (MechaColors?.Length ?? 0); i++)
            {
                MechaColors[i].Serialize(writer);
            }
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
            MechaColors = new Float4[reader.GetInt()];
            for (int i = 0; i < MechaColors.Length; i++)
            {
                MechaColors[i] = reader.GetFloat4();
            }
            LocalPlanetPosition = reader.GetFloat3();
            UPosition = reader.GetDouble3();
            Rotation = reader.GetFloat3();
            BodyRotation = reader.GetFloat3();
            Mecha = new MechaData();
            Mecha.Deserialize(reader);
        }

        public IPlayerData CreateCopyWithoutMechaData()
        {
            return new PlayerData(PlayerId, LocalPlanetId, MechaColors, Username, LocalPlanetPosition, UPosition, Rotation, BodyRotation);
        }
    }
}

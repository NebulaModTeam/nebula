using NebulaAPI;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public class PlayerData : IPlayerData
    {
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
        public ushort DataRevision { get; set; } = 3;

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
            writer.Put("PDREV");
            writer.Put(DataRevision);
            writer.Put(LocalPlanetId);
            LocalPlanetPosition.Serialize(writer);
            UPosition.Serialize(writer);
            Rotation.Serialize(writer);
            BodyRotation.Serialize(writer);
            Mecha.Serialize(writer);
        }

        public void Deserialize(INetDataReader reader)
        {
            try
            {
                if (reader.GetString() != "PDREV")
                {
                    throw new System.Exception();
                }

                DataRevision = reader.GetUShort();

                Logger.Log.Debug($"Deserializing PlayerData with Revision {DataRevision}");

                LocalPlanetId = reader.GetInt();
                LocalPlanetPosition = reader.GetFloat3();
                UPosition = reader.GetDouble3();
                Rotation = reader.GetFloat3();
                BodyRotation = reader.GetFloat3();
                Mecha = new MechaData();
                Mecha.Deserialize(reader);
            }
            catch (System.Exception)
            {
                Logger.Log.Error("Tried to load invalid PlayerData. Data is either corrupt or from an unsupported Nebula version.");
            }
        }

        public IPlayerData CreateCopyWithoutMechaData()
        {
            return new PlayerData(PlayerId, LocalPlanetId, MechaColors, Username, LocalPlanetPosition, UPosition, Rotation, BodyRotation);
        }
    }
}

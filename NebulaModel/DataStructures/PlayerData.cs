using NebulaAPI;
using System.IO;

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
        public MechaAppearance Appearance { get; set; }
        public MechaAppearance DIYAppearance { get; set; }
        public int[] DIYItemId { get; set; }
        public int[] DIYItemValue { get; set; }

        public PlayerData()
        {
            Appearance = null;
            DIYAppearance = null;
            DIYItemId = new int[0];
            DIYItemValue = new int[0];
        }
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
            Appearance = null;
            DIYAppearance = null;
            DIYItemId = new int[0];
            DIYItemValue = new int[0];
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
            writer.Put(Appearance != null);
            if(Appearance != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter wr = new BinaryWriter(ms))
                    {
                        Appearance.Export(wr);
                    }
                    byte[] export = ms.ToArray();
                    writer.Put(export.Length);
                    writer.Put(export);
                }
            }
            writer.Put(DIYAppearance != null);
            if (DIYAppearance != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter wr = new BinaryWriter(ms))
                    {
                        DIYAppearance.Export(wr);
                    }
                    byte[] export = ms.ToArray();
                    writer.Put(export.Length);
                    writer.Put(export);
                }
            }
            writer.Put(DIYItemId.Length);
            for(int i = 0; i < DIYItemId.Length; i++)
            {
                writer.Put(DIYItemId[i]);
                writer.Put(DIYItemValue[i]);
            }
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
            bool isAppearancePresent = reader.GetBool();
            if (isAppearancePresent)
            {
                int len = reader.GetInt();
                byte[] data = new byte[len];
                reader.GetBytes(data, len);
                using (MemoryStream ms = new MemoryStream(data))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    Appearance = new MechaAppearance();
                    Appearance.Init();
                    Appearance.Import(br);
                }
            }
            bool isDIYAppearancePresent = reader.GetBool();
            if (isDIYAppearancePresent)
            {
                int len = reader.GetInt();
                byte[] data = new byte[len];
                reader.GetBytes(data, len);
                using (MemoryStream ms = new MemoryStream(data))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    DIYAppearance = new MechaAppearance();
                    DIYAppearance.Init();
                    DIYAppearance.Import(br);
                }
            }
            int DIYItemLen = reader.GetInt();
            DIYItemId = new int[DIYItemLen];
            DIYItemValue = new int[DIYItemLen];
            for(int i = 0; i < DIYItemLen; i++)
            {
                DIYItemId[i] = reader.GetInt();
                DIYItemValue[i] = reader.GetInt();
            }
        }

        public void Deserialize_5(INetDataReader reader)
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
            bool isAppearancePresent = reader.GetBool();
            if (isAppearancePresent)
            {
                int len = reader.GetInt();
                byte[] data = new byte[len];
                reader.GetBytes(data, len);
                using (MemoryStream ms = new MemoryStream(data))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    Appearance = new MechaAppearance();
                    Appearance.Init();
                    Appearance.Import(br);
                }
            }
        }

        public void Deserialize_4(INetDataReader reader)
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

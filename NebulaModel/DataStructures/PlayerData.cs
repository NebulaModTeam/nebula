#region

using System;
using System.IO;
using NebulaAPI.DataStructures;
using NebulaAPI.GameState;
using NebulaAPI.Interfaces;
using NebulaAPI.Packets;

#endregion

namespace NebulaModel.DataStructures;

[RegisterNestedType]
public class PlayerData : IPlayerData
{
    public PlayerData()
    {
        Appearance = null;
        DIYAppearance = null;
        DIYItemId = Array.Empty<int>();
        DIYItemValue = Array.Empty<int>();
    }

    public PlayerData(ushort playerId, int localPlanetId, string username = null, Float3 localPlanetPosition = new(),
        Double3 position = new(), Float3 rotation = new(), Float3 bodyRotation = new())
    {
        PlayerId = playerId;
        LocalPlanetId = localPlanetId;
        Username = !string.IsNullOrWhiteSpace(username) ? username : $"Player {playerId}";
        LocalPlanetPosition = localPlanetPosition;
        UPosition = position;
        Rotation = rotation;
        BodyRotation = bodyRotation;
        Mecha = new MechaData();
        Appearance = null;
        DIYAppearance = null;
        DIYItemId = Array.Empty<int>();
        DIYItemValue = Array.Empty<int>();
    }

    public string Username { get; set; }
    public ushort PlayerId { get; set; }
    public int LocalPlanetId { get; set; }
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

    public void Serialize(INetDataWriter writer)
    {
        writer.Put(Username);
        writer.Put(PlayerId);
        writer.Put(LocalPlanetId);
        LocalPlanetPosition.Serialize(writer);
        UPosition.Serialize(writer);
        Rotation.Serialize(writer);
        BodyRotation.Serialize(writer);
        Mecha.Serialize(writer);
        writer.Put(Appearance != null);
        if (Appearance != null)
        {
            using var ms = new MemoryStream();
            using (var wr = new BinaryWriter(ms))
            {
                Appearance.Export(wr);
            }
            var export = ms.ToArray();
            writer.Put(export.Length);
            writer.Put(export);
        }
        writer.Put(DIYAppearance != null);
        if (DIYAppearance != null)
        {
            using var ms = new MemoryStream();
            using (var wr = new BinaryWriter(ms))
            {
                DIYAppearance.Export(wr);
            }
            var export = ms.ToArray();
            writer.Put(export.Length);
            writer.Put(export);
        }
        writer.Put(DIYItemId.Length);
        for (var i = 0; i < DIYItemId.Length; i++)
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
        LocalPlanetPosition = reader.GetFloat3();
        UPosition = reader.GetDouble3();
        Rotation = reader.GetFloat3();
        BodyRotation = reader.GetFloat3();
        Mecha = new MechaData();
        Mecha.Deserialize(reader);
        var isAppearancePresent = reader.GetBool();
        if (isAppearancePresent)
        {
            var len = reader.GetInt();
            var data = new byte[len];
            reader.GetBytes(data, len);
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);
            Appearance = new MechaAppearance();
            Appearance.Init();
            Appearance.Import(br);
        }
        var isDIYAppearancePresent = reader.GetBool();
        if (isDIYAppearancePresent)
        {
            var len = reader.GetInt();
            var data = new byte[len];
            reader.GetBytes(data, len);
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);
            DIYAppearance = new MechaAppearance();
            DIYAppearance.Init();
            DIYAppearance.Import(br);
        }
        var DIYItemLen = reader.GetInt();
        DIYItemId = new int[DIYItemLen];
        DIYItemValue = new int[DIYItemLen];
        for (var i = 0; i < DIYItemLen; i++)
        {
            DIYItemId[i] = reader.GetInt();
            DIYItemValue[i] = reader.GetInt();
        }
    }

    public IPlayerData CreateCopyWithoutMechaData()
    {
        return new PlayerData(PlayerId, LocalPlanetId, Username, LocalPlanetPosition, UPosition, Rotation, BodyRotation);
    }

    // Backward compatiblity for older versions
    public void Import(INetDataReader reader, int revision)
    {
        Username = reader.GetString();
        PlayerId = reader.GetUShort();
        LocalPlanetId = reader.GetInt();
        if (revision < 7)
        {
            // MechaColors is obsoleted by MechaAppearance
            var mechaColors = new Float4[reader.GetInt()];
            for (var i = 0; i < mechaColors.Length; i++)
            {
                mechaColors[i] = reader.GetFloat4();
            }
        }
        LocalPlanetPosition = reader.GetFloat3();
        UPosition = reader.GetDouble3();
        Rotation = reader.GetFloat3();
        BodyRotation = reader.GetFloat3();
        MechaData mechaData = new();
        mechaData.Import(reader, revision);
        Mecha = mechaData;
        if (revision >= 5)
        {
            var isAppearancePresent = reader.GetBool();
            if (isAppearancePresent)
            {
                var len = reader.GetInt();
                var data = new byte[len];
                reader.GetBytes(data, len);
                using var ms = new MemoryStream(data);
                using var br = new BinaryReader(ms);
                Appearance = new MechaAppearance();
                Appearance.Init();
                Appearance.Import(br);
            }
        }
        if (revision < 6)
        {
            return;
        }
        {
            var isDIYAppearancePresent = reader.GetBool();
            if (isDIYAppearancePresent)
            {
                var len = reader.GetInt();
                var data = new byte[len];
                reader.GetBytes(data, len);
                using var ms = new MemoryStream(data);
                using var br = new BinaryReader(ms);
                DIYAppearance = new MechaAppearance();
                DIYAppearance.Init();
                DIYAppearance.Import(br);
            }
            var DIYItemLen = reader.GetInt();
            DIYItemId = new int[DIYItemLen];
            DIYItemValue = new int[DIYItemLen];
            for (var i = 0; i < DIYItemLen; i++)
            {
                DIYItemId[i] = reader.GetInt();
                DIYItemValue[i] = reader.GetInt();
            }
        }
    }
}

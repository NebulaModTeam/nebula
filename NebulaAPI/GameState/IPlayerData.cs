#region

using NebulaAPI.DataStructures;
using NebulaAPI.Interfaces;

#endregion

namespace NebulaAPI.GameState;

public interface IPlayerData : INetSerializable
{
    string Username { get; set; }
    ushort PlayerId { get; set; }
    int LocalPlanetId { get; set; }
    Float3 LocalPlanetPosition { get; set; }
    Double3 UPosition { get; set; }
    Float3 Rotation { get; set; }
    Float3 BodyRotation { get; set; }
    int LocalStarId { get; set; }

    IMechaData Mecha { get; set; }
    MechaAppearance Appearance { get; set; }
    MechaAppearance DIYAppearance { get; set; }
    int[] DIYItemId { get; set; }
    int[] DIYItemValue { get; set; }

    IPlayerData CreateCopyWithoutMechaData();
}

﻿namespace NebulaAPI
{
    public interface IPlayerData : INetSerializable
    {
        string Username { get; set; }
        ushort PlayerId { get; set; }
        int LocalPlanetId { get; set; }
        MechaAppearance MechaAppearance { get; set; }
        Float3 LocalPlanetPosition { get; set; }
        Double3 UPosition { get; set; }
        Float3 Rotation { get; set; }
        Float3 BodyRotation { get; set; }
        int LocalStarId { get; set; }

        IMechaData Mecha { get; set; }

        IPlayerData CreateCopyWithoutMechaData();
    }
}
namespace NebulaAPI
{
    public interface IPlayerData : INetSerializable
    {
        string Username { get; set; }
        ushort PlayerId { get; set; }
        int LocalPlanetId { get; set; }
        Float4[] MechaColors { get; set; }
        Float3 LocalPlanetPosition { get; set; }
        Double3 UPosition { get; set; }
        Float3 Rotation { get; set; }
        Float3 BodyRotation { get; set; }
        int LocalStarId { get; set; }

        IMechaData Mecha { get; set; }
        MechaAppearance Appearance { get; set; }

        IPlayerData CreateCopyWithoutMechaData();
    }
}
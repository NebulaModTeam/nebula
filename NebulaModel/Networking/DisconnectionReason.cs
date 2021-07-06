namespace NebulaModel.Networking
{
    public enum DisconnectionReason
    {
        // Websocket Error Codes
        Normal = 1000,
        ProtocolError = 1002,
        InvalidData = 1007,

        // Nebula Specific Error Codes
        HostStillLoading = 2000,
        ClientRequestedDisconnect = 2001,
        ModVersionMismatch = 2002,
        GameVersionMismatch = 2003,

        // Mod Specific Error Codes
        GalacticScaleMissmatch = 3000,
    }
}

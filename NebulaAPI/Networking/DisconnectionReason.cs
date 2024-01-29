namespace NebulaAPI.Networking;

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
    ModIsMissing = 2500,
    ModIsMissingOnServer = 2501
}

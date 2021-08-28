using NebulaModel.DataStructures;
using NebulaModel.Networking;
using System.Collections.Generic;

namespace NebulaModel
{
    public interface IPlayerManager
    {
        Locker GetPendingPlayers(out Dictionary<NebulaConnection, NebulaPlayer> pendingPlayers);

        Locker GetSyncingPlayers(out Dictionary<NebulaConnection, NebulaPlayer> syncingPlayers);

        Locker GetConnectedPlayers(out Dictionary<NebulaConnection, NebulaPlayer> connectedPlayers);

        Locker GetSavedPlayerData(out Dictionary<string, PlayerData> savedPlayerData);

        PlayerData[] GetAllPlayerDataIncludingHost();

        NebulaPlayer GetPlayer(NebulaConnection conn);

        NebulaPlayer GetSyncingPlayer(NebulaConnection conn);

        void SendPacketToAllPlayers<T>(T packet) where T : class, new();

        void SendPacketToLocalStar<T>(T packet) where T : class, new();

        void SendPacketToLocalPlanet<T>(T packet) where T : class, new();

        void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new();

        void SendPacketToStar<T>(T packet, int starId) where T : class, new();

        void SendPacketToStarExcept<T>(T packet, int starId, NebulaConnection exclude) where T : class, new();

        void SendRawPacketToStar(byte[] rawPacket, int starId, NebulaConnection sender);

        void SendRawPacketToPlanet(byte[] rawPacket, int planetId, NebulaConnection sender);

        void SendPacketToOtherPlayers<T>(T packet, NebulaPlayer sender) where T : class, new();

        NebulaPlayer PlayerConnected(NebulaConnection conn);

        void PlayerDisconnected(NebulaConnection conn);

        ushort GetNextAvailablePlayerId();

        void UpdateMechaData(MechaData mechaData, NebulaConnection conn);

        void SendTechRefundPackagesToClients(int techId);
    }
}

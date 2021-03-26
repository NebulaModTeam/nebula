using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;
using NebulaWorld;
using System.Reflection;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class PlayerJoiningProcessor : IPacketProcessor<PlayerJoining>
    {
        public void ProcessPacket(PlayerJoining packet, NebulaConnection conn)
        {
            SimulatedWorld.SpawnRemotePlayerModel(packet.PlayerData);
            typeof(GameMain).GetField("_paused", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(GameMain.instance, true);
            InGamePopup.ShowInfo("Loading", "Player joining the game, please wait", null);
        }
    }
}

#region

using BepInEx.Bootstrap;
using NebulaAPI;
using NebulaAPI.DataStructures;
using NebulaAPI.Interfaces;
using NebulaModel.Networking;

#endregion

/*
 * This packet is only here to handle older clients and tell them to upgrade to a newer nebula version. (this packet was replaced by the lobby packets)
 */
namespace NebulaModel.Packets.Session;

public class HandshakeRequest
{
    public HandshakeRequest() { }

    public HandshakeRequest(byte[] clientCert, string username, Float4[] mechaColors)
    {
        Username = username;
        MechaColors = mechaColors;

        using (var writer = new BinaryUtils.Writer())
        {
            var count = 0;
            foreach (var pluginInfo in Chainloader.PluginInfos)
            {
                if (pluginInfo.Value.Instance is IMultiplayerMod mod)
                {
                    writer.BinaryWriter.Write(pluginInfo.Key);
                    writer.BinaryWriter.Write(mod.Version);
                    count++;
                }
                else
                {
                    foreach (var dependency in pluginInfo.Value.Dependencies)
                    {
                        if (dependency.DependencyGUID != NebulaModAPI.API_GUID)
                        {
                            continue;
                        }
                        writer.BinaryWriter.Write(pluginInfo.Key);
                        writer.BinaryWriter.Write(pluginInfo.Value.Metadata.Version.ToString());
                    }
                }
            }

            ModsVersion = writer.CloseAndGetBytes();
            ModsCount = count;
        }

        GameVersionSig = GameConfig.gameVersion.sig;
        ClientCert = clientCert;
    }

    public string Username { get; set; }
    public Float4[] MechaColors { get; set; }
    public byte[] ModsVersion { get; set; }
    public int ModsCount { get; set; }
    public int GameVersionSig { get; set; }
    public byte[] ClientCert { get; set; }
}

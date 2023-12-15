#region

using BepInEx.Bootstrap;
using NebulaAPI;
using NebulaAPI.Interfaces;
using NebulaModel.Networking;

#endregion

namespace NebulaModel.Packets.Session;

public class LobbyRequest
{
    public LobbyRequest() { }

    public LobbyRequest(byte[] clientCert, string username)
    {
        Username = username;

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
                        count++;
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
    public byte[] ModsVersion { get; set; }
    public int ModsCount { get; set; }
    public int GameVersionSig { get; set; }
    public byte[] ClientCert { get; set; }
}

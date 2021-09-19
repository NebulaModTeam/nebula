using NebulaAPI;
using NebulaModel.Networking;

namespace NebulaModel.Packets.Session
{
    public class HandshakeRequest
    {
        public string Username { get; set; }
        public Float4[] MechaColors { get; set; }
        public byte[] ModsVersion { get; set; }
        public int ModsCount { get; set; }
        public int GameVersionSig { get; set; }
        public byte[] ClientCert { get; set; }

        public HandshakeRequest() { }

        public HandshakeRequest(byte[] clientCert, string username, Float4[] mechaColors)
        {
            Username = username;
            MechaColors = mechaColors;

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                int count = 0;
                foreach (System.Collections.Generic.KeyValuePair<string, BepInEx.PluginInfo> pluginInfo in BepInEx.Bootstrap.Chainloader.PluginInfos)
                {
                    if (pluginInfo.Value.Instance is IMultiplayerMod mod)
                    {
                        writer.BinaryWriter.Write(pluginInfo.Key);
                        writer.BinaryWriter.Write(mod.Version);
                        count++;
                    }
                }

                ModsVersion = writer.CloseAndGetBytes();
                ModsCount = count;
            }

            GameVersionSig = GameConfig.gameVersion.sig;
            ClientCert = clientCert;
        }
    }
}

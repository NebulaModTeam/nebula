using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NebulaModel.Utils
{
    public static class CryptoUtils
    {
        private static readonly string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string dataPath = Path.Combine(docPath, GameConfig.gameName);
        private static readonly string keyFile = Path.Combine(docPath, dataPath, "player.key");

        public static RSA GetOrCreateUserCert()
        {
            if(string.IsNullOrEmpty(docPath))
            {
                Logger.Log.Warn("Could not find documents folder! Using game directory.");
                try
                {
                    Directory.CreateDirectory(dataPath);
                }
                catch(UnauthorizedAccessException e)
                {
                    Logger.Log.Error($"Unable to create directory {dataPath}, permission denied.");
                    throw e;
                }
            }

            RSA rsa = RSA.Create();
            if (File.Exists(keyFile))
            {
                rsa.FromXmlString(File.ReadAllText(keyFile));
            }
            else
            {
                File.WriteAllText(keyFile, rsa.ToXmlString(true));
            }
            return rsa;
        }

        public static byte[] GetPublicKey(RSA rsa)
        {
            return Convert.FromBase64String(rsa.ToXmlString(false).Substring(22, 172));
        }

        public static string Hash(byte[] input)
        {
            byte[] hash = new SHA1Managed().ComputeHash(input);
            return Convert.ToBase64String(hash);
        }

        public static string GetCurrentUserPublicKeyHash()
        {
            return Hash(GetPublicKey(GetOrCreateUserCert()));
        }

        public static string ToBase64(this string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }

        public static string FromBase64(this string s)
        {
            var bytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}

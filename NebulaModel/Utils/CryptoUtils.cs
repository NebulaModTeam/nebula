using System;
using System.IO;
using System.Security.Cryptography;

namespace NebulaModel.Utils
{
    public static class CryptoUtils
    {
        public static string KeyFile = Path.Combine(new string[] { Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), GameConfig.gameName, "player.key" });

        public static RSA GetOrCreateUserCert()
        {
            RSA rsa = RSA.Create();
            if (File.Exists(KeyFile))
            {
                rsa.FromXmlString(File.ReadAllText(KeyFile));
            }
            else
            {
                File.WriteAllText(KeyFile, rsa.ToXmlString(true));
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
    }
}

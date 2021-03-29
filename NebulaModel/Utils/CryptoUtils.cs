using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NebulaModel.Utils
{
    public static class CryptoUtils
    {
        public static string KeyFile = GameConfig.gameDocumentFolder + "player.key";

        public static RSA GetOrCreateUserCert()
        {
            RSA rsa = RSA.Create();
            if (rsa != null)
            {
                return rsa;
            }
            rsa = RSA.Create();
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
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
    }
}

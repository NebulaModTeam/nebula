#region

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NebulaModel.Logger;

#endregion

namespace NebulaModel.Utils;

public static class CryptoUtils
{
    // There are 2 places to store player.key:
    // (1) Documents\Dyson Sphere Program\
    // (2) .\Dyson Sphere Program\ (if MyDocuments folder doesn't exist or is inaccessible)

    private static readonly string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private static readonly string dataPath = Path.Combine(docPath, GameConfig.gameName);
    private static readonly string keyFile = Path.Combine(docPath, dataPath, "player.key");
    private static readonly string dataPath2 = Path.Combine(GameConfig.gameName);
    private static readonly string keyFile2 = Path.Combine(dataPath2, "player.key");

    public static RSA GetOrCreateUserCert()
    {
        if (string.IsNullOrEmpty(docPath))
        {
            Log.Warn("Could not find documents folder! Using game directory.");
            try
            {
                Directory.CreateDirectory(dataPath);
            }
            catch
            {
                Log.Error($"Unable to create directory {dataPath}, permission denied.");
                throw;
            }
        }

        var rsa = RSA.Create();
        if (File.Exists(keyFile))
        {
            rsa.FromXmlString(File.ReadAllText(keyFile));
        }
        else if (File.Exists(keyFile2))
        {
            rsa.FromXmlString(File.ReadAllText(keyFile2));
        }
        else
        {
            try
            {
                Log.Info($"Store player key in {keyFile}");
                File.WriteAllText(keyFile, rsa.ToXmlString(true));
            }
            catch (Exception e)
            {
                Log.Warn($"Unable to write to default path, reason: {e.GetType()}");
                if (!Directory.Exists(dataPath2))
                {
                    Log.Info($"Create directory {dataPath2}");
                    Directory.CreateDirectory(dataPath2);
                }
                Log.Info($"Store player key in {keyFile2}");
                File.WriteAllText(keyFile2, rsa.ToXmlString(true));
            }
        }
        return rsa;
    }

    public static byte[] GetPublicKey(RSA rsa)
    {
        return Convert.FromBase64String(rsa.ToXmlString(false).Substring(22, 172));
    }

    public static string Hash(byte[] input)
    {
        var hash = new SHA1Managed().ComputeHash(input);
        return Convert.ToBase64String(hash);
    }

    public static string Hash(string input)
    {
        var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
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

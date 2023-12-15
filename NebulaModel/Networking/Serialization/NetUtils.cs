#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

#endregion

namespace NebulaModel.Networking.Serialization;

/// <summary>
///     Address type that you want to receive from NetUtils.GetLocalIp method
/// </summary>
[Flags]
public enum LocalAddrType
{
    IPv4 = 1,
    IPv6 = 2,
    All = IPv4 | IPv6
}

/// <summary>
///     Some specific network utilities
/// </summary>
public static class NetUtils
{
    private static readonly List<string> IpList = [];

    public static IPEndPoint MakeEndPoint(string hostStr, int port)
    {
        return new IPEndPoint(ResolveAddress(hostStr), port);
    }

    private static IPAddress ResolveAddress(string hostStr)
    {
        if (hostStr == "localhost")
        {
            return IPAddress.Loopback;
        }

        if (!IPAddress.TryParse(hostStr, out var ipAddress))
        {
            // We can assume true because the version of unity is new enough (2018.4)
            //if (NetSocket.IPv6Support)
            ipAddress = ResolveAddress(hostStr, AddressFamily.InterNetworkV6) ??
                        ResolveAddress(hostStr, AddressFamily.InterNetwork);
        }
        if (ipAddress == null)
        {
            throw new ArgumentException("Invalid address: " + hostStr);
        }

        return ipAddress;
    }

    private static IPAddress ResolveAddress(string hostStr, AddressFamily addressFamily)
    {
        var addresses = ResolveAddresses(hostStr);
        return addresses.FirstOrDefault(ip => ip.AddressFamily == addressFamily);
    }

    private static IPAddress[] ResolveAddresses(string hostStr)
    {
#if NETSTANDARD || NETCOREAPP
        var hostTask = Dns.GetHostEntryAsync(hostStr);
        hostTask.GetAwaiter().GetResult();
        var host = hostTask.Result;
#else
        var host = Dns.GetHostEntry(hostStr);
#endif
        return host.AddressList;
    }

    /// <summary>
    ///     Get all local ip addresses
    /// </summary>
    /// <param name="addrType">type of address (IPv4, IPv6 or both)</param>
    /// <returns>List with all local ip addresses</returns>
    public static List<string> GetLocalIpList(LocalAddrType addrType)
    {
        var targetList = new List<string>();
        GetLocalIpList(targetList, addrType);
        return targetList;
    }

    /// <summary>
    ///     Get all local ip addresses (non alloc version)
    /// </summary>
    /// <param name="targetList">result list</param>
    /// <param name="addrType">type of address (IPv4, IPv6 or both)</param>
    private static void GetLocalIpList(List<string> targetList, LocalAddrType addrType)
    {
        var ipv4 = (addrType & LocalAddrType.IPv4) == LocalAddrType.IPv4;
        var ipv6 = (addrType & LocalAddrType.IPv6) == LocalAddrType.IPv6;
        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                //Skip loopback and disabled network interfaces
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    ni.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                var ipProps = ni.GetIPProperties();

                //Skip address without gateway
                if (ipProps.GatewayAddresses.Count == 0)
                {
                    continue;
                }

                targetList.AddRange(from ip in ipProps.UnicastAddresses select ip.Address into address where ipv4 && address.AddressFamily == AddressFamily.InterNetwork || ipv6 && address.AddressFamily == AddressFamily.InterNetworkV6 select address.ToString());
            }
        }
        catch
        {
            //ignored
            //todo: should this catch be ignored?
        }

        //Fallback mode (unity android)
        if (targetList.Count == 0)
        {
            var addresses = ResolveAddresses(Dns.GetHostName());
            targetList.AddRange(from ip in addresses where ipv4 && ip.AddressFamily == AddressFamily.InterNetwork || ipv6 && ip.AddressFamily == AddressFamily.InterNetworkV6 select ip.ToString());
        }
        if (targetList.Count != 0)
        {
            return;
        }
        if (ipv4)
        {
            targetList.Add("127.0.0.1");
        }

        if (ipv6)
        {
            targetList.Add("::1");
        }
    }

    /// <summary>
    ///     Get first detected local ip address
    /// </summary>
    /// <param name="addrType">type of address (IPv4, IPv6 or both)</param>
    /// <returns>IP address if available. Else - string.Empty</returns>
    public static string GetLocalIp(LocalAddrType addrType)
    {
        lock (IpList)
        {
            IpList.Clear();
            GetLocalIpList(IpList, addrType);
            return IpList.Count == 0 ? string.Empty : IpList[0];
        }
    }
}

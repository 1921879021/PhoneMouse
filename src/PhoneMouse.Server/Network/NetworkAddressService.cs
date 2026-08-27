using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PhoneMouse.Server.Network;

public static class NetworkAddressService
{
    public static string? GetLocalIPv4Address()
    {
        foreach (
            NetworkInterface networkInterface
            in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (
                networkInterface.OperationalStatus
                != OperationalStatus.Up)
            {
                continue;
            }

            if (
                networkInterface.NetworkInterfaceType
                == NetworkInterfaceType.Loopback)
            {
                continue;
            }

            IPInterfaceProperties properties =
                networkInterface.GetIPProperties();

            foreach (
                UnicastIPAddressInformation address
                in properties.UnicastAddresses)
            {
                if (
                    address.Address.AddressFamily
                    != AddressFamily.InterNetwork)
                {
                    continue;
                }

                if (
                    IPAddress.IsLoopback(
                        address.Address))
                {
                    continue;
                }

                string ip =
                    address.Address.ToString();

                if (
                    ip.StartsWith(
                        "169.254."))
                {
                    continue;
                }

                if (
                    ip.StartsWith("192.168.") ||
                    ip.StartsWith("10.") ||
                    IsPrivate172(ip))
                {
                    return ip;
                }
            }
        }

        return null;
    }


    public static string GetPhoneUrl(
        int port = 9527)
    {
        string? ip =
            GetLocalIPv4Address();

        if (
            string.IsNullOrWhiteSpace(ip))
        {
            return
                $"http://127.0.0.1:{port}";
        }

        return
            $"http://{ip}:{port}";
    }


    private static bool IsPrivate172(
        string ip)
    {
        string[] parts =
            ip.Split('.');

        if (
            parts.Length != 4)
        {
            return false;
        }

        if (
            !int.TryParse(
                parts[0],
                out int first))
        {
            return false;
        }

        if (
            !int.TryParse(
                parts[1],
                out int second))
        {
            return false;
        }

        return
            first == 172 &&
            second >= 16 &&
            second <= 31;
    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace DevTools
{
    public class Networking
    {
        public virtual void Send(string text)
        {

        }
        private static void PrintIPData()
        {
            Console.Write("Local IP: ");
            NetworkingPrint(GetLocalIPAddress());

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in interfaces)
            {
                Console.Write("Name: ");
                NetworkingPrint(adapter.Name);
                Console.WriteLine(adapter.Description);
                NetworkingPrint(String.Empty.PadLeft(adapter.Description.Length, '='));
                NetworkingPrint("  Interface type .......................... : " + adapter.NetworkInterfaceType);
                NetworkingPrint("  Operational status ...................... : " + adapter.OperationalStatus);
                string versions = "";

                // Create a display string for the supported IP versions.
                if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                {
                    versions = "IPv4";
                }
                if (adapter.Supports(NetworkInterfaceComponent.IPv6))
                {
                    if (versions.Length > 0)
                    {
                        versions += " ";
                    }
                    versions += "IPv6";
                }
                CustomConsole.NetworkingPrint("  IP version .............................. : " + versions);
                Console.WriteLine();
            }
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            expectingError = true;
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
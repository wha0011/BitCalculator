using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace DevTools
{
    public class Networking
    {
        public virtual void Send(string text)
        {

        }
        public static void PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                CustomConsole.NetworkingPrint(string.Format("Pinging {0} with 32 bytes of data", nameOrAddress));
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
                if (pingable)
                {
                    CustomConsole.NetworkingPrint(string.Format("Reply from {0}, Time={1}ms", nameOrAddress, reply.RoundtripTime));
                }
                else
                {
                    CustomConsole.NetworkingPrint(string.Format("Ping request to {0} failed", nameOrAddress));
                }
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }
        }
        public static void PrintIPData()
        {
            Console.Write("Local IP: ");
            CustomConsole.NetworkingPrint(GetLocalIPAddress());

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in interfaces)
            {
                Console.Write("Name: ");
                CustomConsole.NetworkingPrint(adapter.Name);
                Console.WriteLine(adapter.Description);
                CustomConsole.NetworkingPrint(String.Empty.PadLeft(adapter.Description.Length, '='));
                CustomConsole.NetworkingPrint("  Interface type .......................... : " + adapter.NetworkInterfaceType);
                CustomConsole.NetworkingPrint("  Operational status ...................... : " + adapter.OperationalStatus);
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
            Program.expectingError = true;
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
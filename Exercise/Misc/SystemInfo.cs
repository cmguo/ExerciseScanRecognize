using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Misc
{
    class SystemInfo
    {
        public static string ComputerName = System.Environment.GetEnvironmentVariable("ComputerName");

        public static string[] LocalIpAddresses
        {
            get
            {
                IPAddress[] hostAddresses = Dns.GetHostAddresses("");
                return hostAddresses.Where((hostAddress) =>
                {
                    return hostAddress.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(hostAddress) &&  // ignore loopback addresses
                        !hostAddress.ToString().StartsWith("169.254.");  // ignore link-local addresses
                }).Select((hostAddress) => hostAddress.ToString()).ToArray();
            }
        }

        public static string[] LocalPhysicalAddresses
        {
            get
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                return interfaces.Where((f) =>
                {
                    return f.OperationalStatus == OperationalStatus.Up && (
                        f.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                        || f.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                        && !f.Name.StartsWith("Npcap");  // ignore link-local addresses
                }).Select((f) => BitConverter.ToString(f.GetPhysicalAddress().GetAddressBytes())).ToArray();
            }
        }
    }
}

using Makaretu.Dns;
using System;
using System.Linq;

namespace Exercise.Misc
{
    class ServiceManager
    {

        private MulticastService MulticastService;
        private ServiceDiscovery ServiceDiscovery;

        internal ServiceManager()
        {
            MulticastService = new MulticastService();
            MulticastService.QueryReceived += (s, e) =>
            {
                var names = e.Message.Questions
                    .Select(q => q.Name + " " + q.Type);
                Console.WriteLine("got a query for " + String.Join(", ", names));
            };
            MulticastService.NetworkInterfaceDiscovered += (s, e) =>
            {
                foreach (var nic in e.NetworkInterfaces)
                {
                    Console.WriteLine("discovered NIC ' " + nic.Name + "'");
                }
            };
            ServiceDiscovery = new ServiceDiscovery(MulticastService);
        }

        internal void AddService(string type, ushort port)
        {
            var service = new ServiceProfile(SystemInfo.ComputerName, "_" + type + "._tcp", port);
            ServiceDiscovery.Advertise(service);
        }

        internal void Start()
        {
            MulticastService.Start();
        }

    }
}

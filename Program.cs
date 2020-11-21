using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace network_watcher
{
    class Program
    {
        private static readonly FileStream _stream = new FileStream("log.txt", FileMode.Append);
        private static StreamWriter _writer;
        private static StreamWriter Writer
        {
            get
            {
                if (_writer == null)
                {
                    _writer = new StreamWriter(_stream);
                    _writer.AutoFlush = true;
                }
                return _writer;
            }
        }

        // static void Main(string[] args)
        // {
        //     while (true)
        //     {
        //         var tasks = new List<Task>();
        //         for (var i = 0; i < 100; i++)
        //         {
        //             tasks.Add(Task.Run(() => Ping(IPAddress.Parse("192.168.1.1"))));
        //         }
        //         var command = Console.ReadLine().ToLower();
        //         Task.WaitAll(tasks.ToArray());
        //         if (command == "exit")
        //         {
        //             break;
        //         }
        //     }
        // }



        static void Main(string[] args)
        {
            Console.Clear();
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(OnNetworkAvailabilityChanged);
            while (true)
            {
                var command = Console.ReadLine();
                if (command.ToLower() == "exit")
                {
                    return;
                }
            }
        }

        private static void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs args)
        {
            try
            {
                WriteLine("***Network Availability Changed***");
                WriteLine($"Available: {args.IsAvailable}, Time: {DateTime.Now}");
                var addresses = GetAndDisplayGatewayAddresses();
                addresses.Add(IPAddress.Loopback);
                addresses.Add(IPAddress.Parse("192.168.1.1"));
                WriteLine("Staring pinging...");
                var pingResult = Parallel.ForEach(addresses, a => Ping(a));
            }
            catch (Exception ex)
            {
                WriteLine($"Unhandled exception. Message: {ex.Message}");
            }
        }

        private static List<IPAddress> GetAndDisplayGatewayAddresses()
        {
            WriteLine("Gateways");
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            List<IPAddress> gateways = new List<IPAddress>();
            foreach (var adapter in adapters)
            {
                var adapterProperties = adapter.GetIPProperties();
                var addresses = adapterProperties.GatewayAddresses;
                if (addresses.Count > 0)
                {
                    WriteLine(adapter.Description);
                    foreach (var address in addresses)
                    {
                        WriteLine($"Gateway Address: {address.Address}");
                        gateways.Add(address.Address);
                    }
                }
            }
            return gateways;
        }

        private static void Ping(IPAddress address)
        {
            var ping = new Ping();
            for (var i = 0; i < 5; i++)
            {
                var addressStr = address.ToString();
                var reply = ping.Send(addressStr, 2000);
                var message = $"Address: {reply.Address}, Status: {reply.Status}";
                WriteLine(message);
            }
        }

        private static void WriteLine(string message)
        {
            Console.WriteLine(message);
            Writer.WriteLine(message);
        }
    }
}

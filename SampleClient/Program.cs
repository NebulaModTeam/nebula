using NebulaClient;
using NebulaModel.DataStructures;
using NebulaModel.Packets;
using System;
using System.Threading;

namespace SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            string ip = "localhost";
            int port = 8469;
#else
            Console.WriteLine("Enter Port:");
            string ip = Console.ReadLine();

            Console.WriteLine("Enter Port:");
            int port = int.Parse(Console.ReadLine());
#endif

            Client client = new Client();
            client.Connect(ip, port);

            client.SendPacket(new PlayerSpawned() {
                Position = new Float3(163, -36, -109),
                Rotation = new Float3(0, 33.80f, 259.6f),
            });

            while (!Console.KeyAvailable)
            {
                client.Update();

                Thread.Sleep(15);
            }

            client.Disconnect();
        }
    }
}

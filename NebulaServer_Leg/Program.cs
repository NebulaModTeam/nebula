using System;
using System.Threading;

namespace NebulaServer
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            int port = 8469;
#else
            Console.WriteLine("Enter listening port:");
            int port = int.Parse(Console.ReadLine());
#endif

            Server server = new Server();
            server.Start(port);

            Console.WriteLine($"Server listening on port: {port}");

            while (!Console.KeyAvailable)
            {
                server.Update();
                Thread.Sleep(15);
            }

            server.Stop();
        }
    }
}

using System;

namespace TcpSample.Server
{
    internal class Program
    {
        private static readonly Server Server = new Server();

        private static void Main(string[] args)
        {
            Server.StartServer();
            Console.ReadLine();
        }
    }
}
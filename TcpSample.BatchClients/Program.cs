﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TcpSample.Client;
using TcpSample.Messages;

namespace TcpSample.BatchClients
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Begin();

            Console.ReadLine();
        }

        private static async void Begin()
        {
            var clients = new List<ClientWrapper>();
            var random = new Random();
            for (int i = 0; i < 200; i++)
            {
                await Task.Delay(10);
                clients.Add(new ClientWrapper(new ClientConnection(), random.Next(800, 900)));
            }
        }
    }

    public class ClientWrapper
    {
        private readonly ClientConnection _connection;

        public ClientWrapper(ClientConnection connection, int interval)
        {
            _connection = connection;
            _connection.Start();
            Loop(interval);
        }

        private async void Loop(int interval)
        {
            while (true)
            {
                await Task.Delay(interval);
                _connection.Send(TcpClientMessage.Click);
            }
        }
    }
}
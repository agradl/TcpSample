using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TcpSample.Messages;

namespace TcpSample.Server
{
    public class Server
    {
        public const int Port = 54321;
        public static ConcurrentDictionary<string, Player> ActivePlayers = new ConcurrentDictionary<string, Player>();
        public static ConcurrentDictionary<string, Game> ActiveGames = new ConcurrentDictionary<string, Game>();
        public static ConcurrentQueue<Player> PlayerQueue = new ConcurrentQueue<Player>();
        private static readonly ConcurrentStack<Game> PendingStart = new ConcurrentStack<Game>();
        private int _age;
        private int _gamesPerSecond;
        private int _gamesPlayed;
        private TcpListener _listener;

        private Timer _status;

        public void StartServer()
        {
            Init();
            Task.Run(() => AcceptNewClients());
            Task.Run(() => HandleGameQueue());
            Task.Run(() => HandleGameEnded());
            Task.Run(() => RunGames());
        }

        private void Init()
        {
            Console.WriteLine("Starting server");
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            _status = new Timer(x =>
                                    {
                                        Console.WriteLine("Games : {0}", _gamesPlayed);
                                        Console.WriteLine("Games Per Second : {0}", _gamesPerSecond/3);
                                        if (_gamesPerSecond > 0)
                                            Console.WriteLine("Avg Age : {0}", _age/_gamesPerSecond);
                                        _gamesPerSecond = 0;
                                        _age = 0;
                                        Console.WriteLine("Active Games : {0}", ActiveGames.Count);
                                        Console.WriteLine("Active Players : {0}", ActivePlayers.Count);
                                        Console.WriteLine("In Queue : {0}", PlayerQueue.Count);
                                        Console.WriteLine();
                                    }, null, 0, 3000);
            Console.WriteLine("Server started : {0}", Port);
        }

        private async void AcceptNewClients()
        {
            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                var player = new Player(client);
                PlayerQueue.Enqueue(player);
                ActivePlayers.TryAdd(player.Id, player);
            }
        }

        private async void HandleGameQueue()
        {
            while (true)
            {
                if (PlayerQueue.Count/2 == 0)
                {
                    await Task.Delay(100);
                    continue;
                }
                Player player1;
                Player player2;

                PlayerQueue.TryDequeue(out player1);
                PlayerQueue.TryDequeue(out player2);

                if (player1 == null || player2 == null)
                {
                    PlayerQueue.Enqueue(player1 ?? player2);
                    continue;
                }

                var game = new Game(player1, player2);
                ActiveGames.TryAdd(game.Id, game);
                PendingStart.Push(game);
            }
        }

        private async void RunGames()
        {
            while (true)
            {
                var array = new Game[25];
                if (PendingStart.TryPopRange(array, 0, array.Length) > 0)
                {
                    foreach (Game i in array.Where(x => x != null))
                    {
                        i.Start();
                    }
                }
                else
                {
                    await Task.Delay(10);
                }
            }
        }

        private async void HandleGameEnded()
        {
            while (true)
            {
                IEnumerable<KeyValuePair<string, Game>> games = ActiveGames.Where(x => x.Value.GameOver);
                if (!games.Any())
                {
                    await Task.Delay(100);
                    continue;
                }
                foreach (var kv in games)
                {
                    Game game = kv.Value;
                    if (!ActiveGames.TryRemove(game.Id, out game))
                    {
                        return;
                    }
                    _gamesPlayed += 1;
                    _age += game.SecondsOld;
                    _gamesPerSecond += 1;
                    if (!game.Player1.Disconnected)
                    {
                        PlayerQueue.Enqueue(game.Player1);
                        game.Player1.Send(TcpServerMessage.InQueue);
                    }
                    else
                    {
                        Player y;
                        ActivePlayers.TryRemove(game.Player1.Id, out y);
                    }

                    if (!game.Player2.Disconnected)
                    {
                        PlayerQueue.Enqueue(game.Player2);
                        game.Player2.Send(TcpServerMessage.InQueue);
                    }
                    else
                    {
                        Player y;
                        ActivePlayers.TryRemove(game.Player2.Id, out y);
                    }
                }
            }
        }
    }
}
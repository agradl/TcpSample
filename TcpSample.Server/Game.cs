using System;
using System.Threading.Tasks;
using TcpSample.Messages;

namespace TcpSample.Server
{
    public class Game
    {
        public string Id = Guid.NewGuid().ToString();
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public int SecondsOld { get { return (int)(DateTime.Now - _startDate).TotalSeconds; } }
        private readonly DateTime _startDate = DateTime.Now;
        public Game(Player player1, Player player2)
        {
            Player1 = player1;
            Player2 = player2;
            SendToPlayers(TcpServerMessage.InGame);
        }

        async public Task Start()
        {
            while (_round <= 3 && !GameOver)
            {
                while (_state != GameState.Click && !GameOver)
                {
                    await Task.Delay(500);
                    Step();
                    
                }
                Player1.ReceiveCallback = x => Clicked(1);
                Player2.ReceiveCallback = x => Clicked(2);
                while (_state == GameState.Click && !GameOver)
                {
                    CheckPlayerStatus();//end game if both players disconnect
                    await Task.Delay(500);
                }
            }
            ChangeState(GameState.Ended);
        }

        public enum GameState
        {
            Wait5 = 0,
            Wait4 = 1,
            Wait3 = 2,
            Wait2 = 3,
            Wait1 = 4,
            CountDown3 = 5,
            CountDown2 = 6,
            CountDown1 = 7,
            Click = 8,
            Ended = 9
        }

        private int _round = 1;
        private GameState _state = GameState.Wait5;

        private void Clicked(int player)
        {
            lock(this)//only one winner!
            {
                if (_state == GameState.Click)
                {
                    ChangeState(GameState.Wait5);
                    if (player == 1)
                    {
                        SendToPlayers(TcpServerMessage.Won, TcpServerMessage.Lost);
                    }
                    else
                    {
                        SendToPlayers(TcpServerMessage.Lost, TcpServerMessage.Won);
                    }
                    _round++;
                }
            }
        }

        private void Step()
        {
            ChangeState((GameState) ((int) _state + 1));
            switch (_state)
            {
                case GameState.CountDown3:
                    SendToPlayers(TcpServerMessage.CountDown3);
                    break;
                case GameState.CountDown2:
                    SendToPlayers(TcpServerMessage.CountDown2);
                    break;
                case GameState.CountDown1:
                    SendToPlayers(TcpServerMessage.CountDown1);
                    break;
                case GameState.Click:
                    SendToPlayers(TcpServerMessage.Click);
                    break;
            }
        }

        private void SendToPlayers(TcpServerMessage message, TcpServerMessage message2 = TcpServerMessage.None)
        {
            try
            {
                Player1.Send(message);
                Player2.Send(message2 != TcpServerMessage.None ? message2 : message);
            }
            catch
            {
                ChangeState(GameState.Ended);
            }
        }
        private readonly object _lock = new object();
        private void ChangeState(GameState state)
        {
            lock (_lock)
            {
                if (_state != GameState.Ended && (int)state <= (int)GameState.Ended)
                    _state = state;
            }
        }

        private void CheckPlayerStatus()
        {
            if (Player1.Disconnected || Player2.Disconnected)
                ChangeState(GameState.Ended);
        }

        public bool GameOver { get { return _state == GameState.Ended; } }
    }
}
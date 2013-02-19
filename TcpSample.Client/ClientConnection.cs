using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using TcpSample.Messages;

namespace TcpSample.Client
{
    public class ClientConnection
    {
        private const string IpAddress = "127.0.0.1";
        private const int Port = 54321;

        private readonly byte[] _bytes = new byte[1024];

        public Action<TcpServerMessage> MessageHandler = x => { };
        public Action<TcpState> StateHandler = x => { };
        private TcpClient Connection { get; set; }

        public void Start()
        {
            Connect(); //returns immediately
        }

        private async void Connect()
        {
            try
            {
                if (Connection == null || !Connection.Connected)
                {
                    Connection = new TcpClient(); //after losing a connection, need a new TcpClient
                    State(TcpState.Connecting);
                    await Connection.ConnectAsync(IpAddress, Port);
                    State(TcpState.Connected);
                    Receive();
                }
            }
            catch
            {
                State(TcpState.Disconnected);
            }
            await Task.Delay(5000);
            Connect();
        }


        public async void Send(TcpClientMessage message)
        {
            if (Connection.Connected && Connection.GetStream().CanWrite)
            {
                await Connection.GetStream().WriteAsync(BitConverter.GetBytes((int) message), 0, 4);
            }
        }


        private async void Receive()
        {
            try
            {
                await Connection.GetStream().ReadAsync(_bytes, 0, _bytes.Length);
                TcpServerMessage serverMessage = ReadMessage(_bytes);
                Message(serverMessage);
                //clear the buffer
                Array.Clear(_bytes, 0, _bytes.Length);
                //start receiving next message
                Receive();
            }
            catch
            {
                State(TcpState.Disconnected);
            }
        }


        private void Message(TcpServerMessage msg)
        {
            //_context.Post(x => MessageHandler((TcpServerMessage) x), msg);
            MessageHandler(msg);
        }

        private void State(TcpState state)
        {
            //_context.Post(x => StateHandler((TcpState)x), state);
            StateHandler(state);
        }

        private TcpServerMessage ReadMessage(byte[] bytes)
        {
            return (TcpServerMessage) BitConverter.ToInt16(bytes, 0);
        }
    }
}
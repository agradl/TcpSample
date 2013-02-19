using System;
using System.Net.Sockets;
using TcpSample.Messages;

namespace TcpSample.Server
{
    public class Player
    {
        public string Id = Guid.NewGuid().ToString();
        private byte[] _buffer;

        public Player(TcpClient connection)
        {
            ReceiveCallback = message => { };
            Connection = connection;
            Receive(); //keep the buffer empty so Receive callback is called the instant click is recieved
        }

        public TcpClient Connection { get; set; }

        async public void Send(TcpServerMessage message)
        {
            try
            {
                byte[] msg = BitConverter.GetBytes((int)message);
                await Connection.GetStream().WriteAsync(msg, 0, msg.Length);
            }
            catch 
            {
                Disconnected = true;
                throw;
            }
        }

        public Action<TcpClientMessage> ReceiveCallback { get; set; }
        async private void Receive()
        {
            try
            {
                while (true)
                {
                    _buffer = new byte[256];
                    await Connection.GetStream().ReadAsync(_buffer, 0, _buffer.Length);
                    ReceiveCallback((TcpClientMessage) BitConverter.ToInt32(_buffer, 0));
                }
            }
            catch
            {
                Disconnected = true;
                throw;
            }

        }

        public bool Disconnected { get; set; }
    }
}
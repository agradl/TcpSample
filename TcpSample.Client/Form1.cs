using System;
using System.Windows.Forms;
using TcpSample.Messages;

namespace TcpSample.Client
{
    public partial class Form1 : Form
    {
        
        //private int max = 7;
        //private int position;
        private ClientConnection ClientConnection { get; set; }
        public Form1()
        {
            InitializeComponent();
            Load += OnLoad;
            button1.Click += Button1Click;
        }

        void Button1Click(object sender, EventArgs e)
        {
            ClientConnection.Send(TcpClientMessage.Click);
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {

            ClientConnection = new ClientConnection
                                   {
                                       MessageHandler = HandleMessageChanged, 
                                       StateHandler = HandleStateChanged
                                   };
            ClientConnection.Start();
        }
        
        private void HandleStateChanged(TcpState state)
        {
            lblStatus.Text = state.ToString();
            button1.Enabled = false;
            lblMessage.Text = "";
        }
        private void HandleMessageChanged(TcpServerMessage message)
        {
            switch (message)
            {
                case TcpServerMessage.CountDown3:
                    lblMessage.Text = "3";
                    break;
                case TcpServerMessage.CountDown2:
                    lblMessage.Text = "2";
                    break;
                case TcpServerMessage.CountDown1:
                    lblMessage.Text = "1";
                    break;
                case TcpServerMessage.Click:
                    lblMessage.Text = "0";
                    button1.Enabled = true;
                    break;
                case TcpServerMessage.Lost:
                    button1.Enabled = false;
                    lblMessage.Text = "You Lost!";
                    break;
                case TcpServerMessage.Won:
                    button1.Enabled = false;
                    lblMessage.Text = "You Won!";
                    break;
                case TcpServerMessage.InQueue:
                    lblMessage.Text = "In Queue";
                    break;
                case TcpServerMessage.InGame:
                    lblMessage.Text = "In Game";
                    break;
            }
        }


    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DevTools
{
    public class ClientNetworking : Networking
    {
        private Socket _clientSocket;
        private static byte[] _buffer = new byte[1000];
        public Func<string,bool> RecieveMessage;

        public string IP;
        public int Port;

        public ClientNetworking(string IP, int Port, Func<string, bool> RecieveMessage, ProtocolType protocolType)
        {
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);

            this.IP = IP;
            this.Port = Port;
            this.RecieveMessage = RecieveMessage;
            SetupClient();
        }

        public void SetupClient()
        {
            ConnectLoop();
            _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), _clientSocket);
        }
        private void RecieveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int recieved = socket.EndReceive(AR);
            byte[] dataBuf = new byte[recieved];
            Array.Copy(_buffer, dataBuf, recieved);

            string text = Encoding.ASCII.GetString(dataBuf);
            RecieveMessage(text);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
        }
        private void ConnectLoop()
        {
            int attempts = 0;
            while (!_clientSocket.Connected)
            {
                try
                {
                    ++attempts;
                    if (DateTime.Now.Hour >= 15)
                    {
                        _clientSocket.Connect(IPAddress.Parse(IP), Port);
                    }
                    else
                    {
                        _clientSocket.Connect(IPAddress.Parse(IP), Port);
                    }
                }
                catch (Exception e)
                {
                }
            }
            Send("Connected client");
        }
        public override void Send(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            _clientSocket.Send(data);
        }
    }
}

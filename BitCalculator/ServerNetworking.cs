using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DevTools
{
    class ServerNetworking : Networking
    {
        private readonly Socket serverSocket;
        private readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private int Port;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];

        public Func<string, bool> Log;

        public ServerNetworking(int Port, Func<string,bool> Log, ProtocolType protocolType)
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);

            this.Log = Log;
            this.Port = Port;
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Log("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Log("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Log("Server recieved: " + text);

            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }
        public override void Send(string text)
        {
            foreach (var client in clientSockets)
            {
                try
                {
                    byte[] data = Encoding.ASCII.GetBytes(text);
                    client.Send(data);
                    Log(string.Format("Sent {0} bytes of data", data.Length));
                }
                catch
                {

                }
            }
        }
    }

}

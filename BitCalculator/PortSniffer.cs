using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DevTools
{
    class PortSniffer
    {
        Socket mainSocket;
        byte[] INBYTES = new byte[1000];
        byte[] inbytes
        {
            set
            {
                INBYTES = value;
                Log("port " + portno + " recieved: " + Encoding.ASCII.GetString(value));
            }
            get
            {
                return INBYTES;
            }
        }
        byte[] OUTBYTES = new byte[1000];
        byte[] outbytes
        {
            set
            {
                OUTBYTES = value;
                Log("port " + portno + " recieved: " + Encoding.ASCII.GetString(value));
            }
            get
            {
                return OUTBYTES;
            }
        }

        Func<string, bool> Log;
        public int portno;
        public PortSniffer(int portno, Func<string,bool> log)
        {
            byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
            byte[] byOut = new byte[4] { 1, 0, 0, 0 };

            Log = log;
            this.portno = portno;
            var level = 0;
            try
            {
                mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw,
                                       ProtocolType.IP);
                ++level;
                mainSocket.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.22"), 0));

                mainSocket.SetSocketOption(SocketOptionLevel.IP,  //Applies only to IP packets
                           SocketOptionName.HeaderIncluded, //Set the include header
                           true);

                ++level;
                mainSocket.IOControl(IOControlCode.ReceiveAll,  //SIO_RCVALL of Winsock
                                     byTrue, byOut);
                ++level;
                mainSocket.BeginReceive(INBYTES, 0, 1000, SocketFlags.None, new AsyncCallback(RecieveCallback), null);
                ++level;
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log(level.ToString());
            }
            Log("Started sniffing");
        }
        private void RecieveCallback(IAsyncResult AR)
        {
            Log("Recieved info");
            var level = 0;
            try
            {
                Socket socket = (Socket)AR.AsyncState;
                ++level;
                int recieved = socket.EndReceive(AR);
                ++level;
                byte[] dataBuf = new byte[recieved];
                ++level;
                Array.Copy(INBYTES, dataBuf, recieved);

                ++level;
                string text = Encoding.ASCII.GetString(dataBuf);
                ++level;
                Log("Client recieved: " + text);
                ++level;
                mainSocket.BeginReceive(INBYTES, 0, 1000, SocketFlags.None, new AsyncCallback(RecieveCallback), null);
                ++level;
            }
            catch
            {
                Log("Networking error");
                Log(level.ToString());
            }
        }
    }
}

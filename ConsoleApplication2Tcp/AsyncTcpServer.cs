using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ConsoleApplication2Tcp
{
    class AsyncTcpServer
    {
        private TcpListenerStateObject _server;
        private Hashtable _clientList = new Hashtable();
        internal class TcpListenerStateObject
        {
            public TcpListener Listener=null;
            public NetworkStream NetworkStream = null;
        }
        internal class Client
        {
            private string _id;
            private string _ip;
            private int _port;

            public Client(string id, string ip, int port)
            {
                _id = id;
                _ip = ip;
                _port = port;
            }

        }
        public AsyncTcpServer(string ipAddress, string port)
        {
            StartListener(ipAddress, port);
        }

        private void StartListener(string ipAddress, string port)
        {
            _server = new TcpListenerStateObject();
            _server.Listener = new TcpListener(IPAddress.Parse(ipAddress),int.Parse(port));
            TcpClient client;
            _server.Listener.Start();
            Console.WriteLine("waiting for connect...");
            while (true)
            {
                client = _server.Listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(DealTheClient, client);
            }
        }

        private void DealTheClient(object state)
        {
            var client = (TcpClient) state;
            //GCHandle clientGcHandle = GCHandle.Alloc(client, GCHandleType.Normal);

            //Get the client ip address
            //string clientID = AddressOf(clientGcHandle);
            string clientID = Guid.NewGuid().ToString();

            string clientIPAddress = IPAddress.Parse(((
                IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()).ToString();
            int clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;

            _clientList.Add(clientID,new Client(clientID,clientIPAddress,clientPort));

            Console.WriteLine(clientID + ":" + clientIPAddress + ":" + clientPort);
            Console.WriteLine("conneted client number:"+_clientList.Count);
            _server.NetworkStream = client.GetStream();
            if (false)
            {
                #region GC
                //clientGcHandle.Free();
                _clientList.Remove(clientID);
                client.Close();
                #endregion
            }
            
        }
        public byte[] ByteCountLittleEndian(int a)
        {
            return BitConverter.GetBytes(a);
        }
        public byte[] ByteCountBigEndian(int a)
        {
            byte[] b = BitConverter.GetBytes(a);
            return b.Reverse().ToArray();
        }
        private string AddressOf(GCHandle handle)
        {
            IntPtr pointer = GCHandle.ToIntPtr(handle);
            return "0x" + pointer.ToString("X");
        }

    }
}

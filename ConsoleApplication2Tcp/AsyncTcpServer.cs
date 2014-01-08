using System;
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
        internal class TcpListenerStateObject
        {
            public TcpListener Listener=null;
            public NetworkStream NetworkStream = null;
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
            GCHandle cliGcHandle = GCHandle.Alloc(client, GCHandleType.Normal);

            //Get the client ip address
string clientIPAddress = "Your Ip Address is: " + IPAddress.Parse(((
    IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
var port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
Console.WriteLine(AddressOf(cliGcHandle) + ":" + clientIPAddress+":"+port);
        }
        private string AddressOf(GCHandle handle)
        {
            IntPtr pointer = GCHandle.ToIntPtr(handle);
            return "0x" + pointer.ToString("X");
        }

        private void ReleaseGcHandle(GCHandle a)
        {
            a.Free();
        }
    }
}

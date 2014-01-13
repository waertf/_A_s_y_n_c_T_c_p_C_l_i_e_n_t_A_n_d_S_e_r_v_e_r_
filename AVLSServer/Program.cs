using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using System.Threading;

namespace AVLSServer
{
    class Program
    {
        internal class Client
        {
            private TcpClient t6002;
            private TcpClient t7000;

            public Client(TcpClient client6002, TcpClient client7000)
            {
                t6002 = client6002;
                t7000 = client7000;
            }

            public TcpClient getClient7000()
            {
                return t7000;
            }
            public TcpClient getClient6002()
            {
                return t6002;
            }
        }
        static void Main(string[] args)
        {
            TcpListener tcpListener7000 = new TcpListener(IPAddress.Parse(ConfigurationManager.AppSettings["ip"]),7000);
            TcpListener tcpListener6002 = new TcpListener(IPAddress.Parse(ConfigurationManager.AppSettings["ip"]),6002);
            TcpClient client7000,client6002;
            tcpListener6002.Start();
            tcpListener7000.Start();
            Console.WriteLine("waiting for connect...");
            while (true)
            {
                client6002 = tcpListener6002.AcceptTcpClient();
                client7000 = tcpListener7000.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(DealTheClient, new Client(client6002,client7000));
            }

        }

        private static void DealTheClient(object state)
        {
            Client clientState = (Client) state;
            TcpClient client7000 = clientState.getClient7000();
            TcpClient client6002 = clientState.getClient6002();
            NetworkStream netStream7000 = client7000.GetStream();
            NetworkStream netStream6002 = client6002.GetStream();
        }
    }
}

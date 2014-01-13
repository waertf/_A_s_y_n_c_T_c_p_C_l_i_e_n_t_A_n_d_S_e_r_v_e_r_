using System;
using System.Collections.Generic;
using System.IO;
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

        struct UNITReportPacket
        {
            public string ID;
            public string GPSValid;
            public string DateTime;
            public string Loc;
            public string Speed;
            public string Dir;
            public string Temp;
            public string Status;
            public string Event;
            public string Message;

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
            string client7000Address = IPAddress.Parse(((
                IPEndPoint) client7000.Client.RemoteEndPoint).Address.ToString()).ToString();
            string client6002Address = IPAddress.Parse(((
                IPEndPoint)client6002.Client.RemoteEndPoint).Address.ToString()).ToString();
            NetworkStream netStream7000 = client7000.GetStream();
            NetworkStream netStream6002 = client6002.GetStream();
            using (StreamReader reader = new StreamReader(netStream7000))
            {
                uint message7000Counter = 0;
                int idCounter = 0;
                while (true)
                {
                    string message = reader.ReadLine();

                    if (message == null)
                    {
                        Console.WriteLine(client7000Address + " has disconnected");
                        break;
                    }

                    message7000Counter++;
                    Console.WriteLine(client7000Address+String.Format(" >> [{0}] Message received: {1}", message7000Counter, message));
                    string[] stringSeparators = new string[] { ",","%%" };
                    string[] receiveStrings = message.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    int counter = 0;
                    UNITReportPacket recvReportPacket = new UNITReportPacket();
                    foreach (var receiveString in receiveStrings)
                    {
                        
                        Console.WriteLine(counter +":"+receiveString);
                        switch (counter)
                        {
                            case 0:
                                recvReportPacket.ID = receiveString;
                                break;
                            case 1:
                                recvReportPacket.GPSValid = receiveString;
                                break;
                            case 2:
                                recvReportPacket.DateTime = receiveString;
                                break;
                            case 3:
                                recvReportPacket.Loc = receiveString;
                                break;
                            case 4:
                                recvReportPacket.Speed = receiveString;
                                break;
                            case 5:
                                recvReportPacket.Dir = receiveString;
                                break;
                            case 6:
                                recvReportPacket.Temp = receiveString;
                                break;
                            case 7:
                                recvReportPacket.Status = receiveString;
                                break;
                            case 8:
                                recvReportPacket.Event = receiveString;
                                break;
                            case 9:
                                recvReportPacket.Message = receiveString;
                                break;
                        }
                        counter++;
                    }
                    using (var m = new MemoryStream())
                    {
                        string Head = "$CMD_H#";
                        string Tail = "$CMD_T#";
                        
                        byte[] headBytes = Encoding.ASCII.GetBytes(Head);
                        byte[] HeadLength = ByteCountBigEndian(headBytes.Count());

                        byte[] Cmd_Type = new byte[]{2} ;
                        byte[] id = ByteCountBigEndian(idCounter++);
                        byte[] priority = new byte[]{5};
                        byte[] Attach_Type = new byte[]{1};

                        #region avlsPackageFromPort7000

                        byte[] uid = Encoding.ASCII.GetBytes(recvReportPacket.ID);
                        byte[] uidLength = ByteCountBigEndian(uid.Count());
                        #endregion

                        byte[] tailBytes = Encoding.ASCII.GetBytes(Tail);
                        byte[] TailLength = ByteCountBigEndian(tailBytes.Count());

                    }
                }
            }
        }
        static byte[] ByteCountBigEndian(int a)
        {
            byte[] b = BitConverter.GetBytes(a);
            return b.Reverse().ToArray();
        }
    }
}

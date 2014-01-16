using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using System.Threading;

namespace ConsoleApplication1AVSLPackageSender
{
    class Program
    {
        static string[] uidStrings = new string[] { "900301", "912639", "912429", "913498", "912635" };
        static string[] GPSValid = new string[] { "L", "A" };
        const string Temp = "NA";
        const string Status = "00000000";
        static string[] Event = new[] { "181", "182", "175", "000", "-1", "150" };
        static string[] Loc = new[]
        {
            "N2457.7728E12155.29", "N2506.7122E12154.9096",
            "N2509.8574E12124.9516","N2507.2636E12153.2788",
            "N2435.7498E12151.5004"            
        };
         static Random rand = new Random();
        private static NetworkStream networkStream;
        static TcpClient avlsTcpClient;
        static void Main(string[] args)
        {
            avlsTcpClient = new TcpClient(ConfigurationManager.AppSettings["ServerIP"], 7000);
            networkStream = avlsTcpClient.GetStream();
            Thread sendthread = new Thread(() => send(networkStream));
            sendthread.Start();
            
        }

        static  object send(NetworkStream networkStream)
        {
            while (true)
            {
                {
                    string time = DateTime.UtcNow.ToString("yyMMddHHmmss");
                    string Speed = rand.Next(0, 1000).ToString();
                    string Dir = rand.Next(0, 360).ToString();
                    string uid = uidStrings[rand.Next(0, uidStrings.Length)];
                    string gps = GPSValid[rand.Next(0, GPSValid.Length)];
                    string _event = Event[rand.Next(0, Event.Length)];
                    string loc = Loc[rand.Next(0, Loc.Length)];
                    string package = "%%" + uid + "," +
                              gps + "," +
                              time + "," +
                              loc + "," +
                              Speed + "," +
                              Dir + "," +
                              Temp + "," +
                              Status + "," +
                              _event + ",test" + Environment.NewLine;
                    byte[] sendByte = Encoding.ASCII.GetBytes(package);
                    
                    if (true)
                    {
                        try
                        {
                            networkStream.Write(sendByte, 0, sendByte.Length);
                            //networkStream.BeginWrite(sendByte, 0, sendByte.Length, sendAsyncCallback, networkStream);
                            networkStream.Flush();
                        }
                        catch (Exception ex)
                        {
                            networkStream.Close();
                            avlsTcpClient.Close();
                            avlsTcpClient = new TcpClient(ConfigurationManager.AppSettings["ServerIP"], 7000);
                            networkStream = avlsTcpClient.GetStream();
                            Thread sendthread = new Thread(() => send(networkStream));
                            sendthread.Start();
                        }
                    }
                    Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["sleepInMilliSeconds"]));
                }
                
            }
        }

        private static void sendAsyncCallback(IAsyncResult ar)
        {
            try
            {
                NetworkStream myNetworkStream = (NetworkStream)ar.AsyncState;
                myNetworkStream.EndWrite(ar);
            }
            catch (Exception ex)
            {
                networkStream.Close();
                avlsTcpClient.Close();
                avlsTcpClient = new TcpClient(ConfigurationManager.AppSettings["ServerIP"], 7000);
                networkStream = avlsTcpClient.GetStream();
                Thread sendthread = new Thread(() => send(networkStream));
                sendthread.Start();
            }
        }
    }
}

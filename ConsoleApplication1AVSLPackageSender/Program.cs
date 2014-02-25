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
        static string[] Event = new[] { "181", "182", "175", "0", "-1", "150" };
        static string[] Loc = new[]
        {
            //24.69203 121.8355
            "N2441.5218E12150.13",
            //24.69203 121.83558
            "N2441.5218E12150.1348",
            //25.13316 121.74165
            "N2507.9896E12144.499",
            //25.16042 121.7341
            "N2509.6252E12144.046",
            //25.02105 121.95243
            "N2501.263E12157.1458"            
            
        };
        static string[] Msg = new[]
        {
            //24.69203 121.8355
            "test",
            //24.69203 121.83558
            "p_prohibited,"+ConfigurationManager.AppSettings["gid0"]+","+ConfigurationManager.AppSettings["fullname0"],
            //25.13316 121.74165
            "patrol_location,"+ConfigurationManager.AppSettings["gid1"]+","+ConfigurationManager.AppSettings["fullname1"],
            //25.16042 121.7341
            "p_prohibited,"+ConfigurationManager.AppSettings["gid0"]+","+ConfigurationManager.AppSettings["fullname0"]+";"+
            "patrol_location,"+ConfigurationManager.AppSettings["gid1"]+","+ConfigurationManager.AppSettings["fullname1"],
            //25.02105 121.95243
           "patrol_location,"+ConfigurationManager.AppSettings["gid1"]+","+ConfigurationManager.AppSettings["fullname1"]+";"+            
            "p_prohibited,"+ConfigurationManager.AppSettings["gid0"]+","+ConfigurationManager.AppSettings["fullname0"]
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
                    //string uid = uidStrings[rand.Next(0, uidStrings.Length)];
                    string uid = rand.Next(0, 1000).ToString();
                    string gps = GPSValid[rand.Next(0, GPSValid.Length)];
                    string _event = Event[rand.Next(0, Event.Length)];
                    string loc = Loc[rand.Next(0, Loc.Length)];
                    string message = Msg[rand.Next(0, Msg.Length)];
                    string package = "%%" + uid + "," +
                              gps + "," +
                              time + "," +
                              loc + "," +
                              Speed + "," +
                              Dir + "," +
                              Temp + "," +
                              Status + "," +
                              _event + "," +
                              message+Environment.NewLine;
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

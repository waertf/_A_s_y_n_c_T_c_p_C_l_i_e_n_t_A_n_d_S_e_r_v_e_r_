using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Configuration;
using System.Threading;
using Gurock.SmartInspect;


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
        static TcpListener tcpListener7000, tcpListener6002;
        static TcpClient client7000t, client6002t;
        static ManualResetEvent stopEvent = new ManualResetEvent(false);
        static bool port7000reset, port6002reset,port7000reconnect;
        private static HandlerRoutine _ConsoleCtrlCheckDelegate;
        static Mutex _mutex = new Mutex(false, "avlsServer.exe");
        static void Main(string[] args)
        {
            if (!_mutex.WaitOne(1000, false))
                return;
            //Thread.Sleep(5000);
            _ConsoleCtrlCheckDelegate=new HandlerRoutine(ConsoleCtrlCheck);
            SetConsoleCtrlHandler(_ConsoleCtrlCheckDelegate, true);//detect when console be closed
            #region catchCloseEvent
            Thread catchCloseEvent = new System.Threading.Thread
              (delegate()
              {

                  while (!isclosing)
                  {
                      Thread.Sleep(1000);
                  }
                  GC.KeepAlive(_ConsoleCtrlCheckDelegate); 
                  Environment.Exit(0);

              });
            catchCloseEvent.Start();
            #endregion catchCloseEvent
            SiAuto.Si.Enabled = true;
            SiAuto.Si.Level = Level.Debug;
            //Console.WriteLine(@"file(filename=""" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log.sil\",rotate=weekly,append=true,maxparts=5,maxsize=500MB)");
            SiAuto.Si.Connections = @"file(filename=""" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log.sil\",rotate=weekly,append=true,maxparts=5,maxsize=500MB)";
            SiAuto.Main.LogText(Level.Debug, "waiting for connect", "");
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            port7000reconnect = true;
            if (bool.Parse(ConfigurationManager.AppSettings["manualIP"]))
            {
                 tcpListener7000 = new TcpListener(IPAddress.Parse(ConfigurationManager.AppSettings["ip"]),7000);
                 tcpListener6002 = new TcpListener(IPAddress.Parse(ConfigurationManager.AppSettings["ip"]),6002); 
            }
            else
            {
                 tcpListener6002 = new TcpListener(IPAddress.Any, 6002);
                 tcpListener7000 = new TcpListener(IPAddress.Any, 7000);
            }
            
           
            
            tcpListener6002.Start();
            tcpListener7000.Start();
            Console.WriteLine(DateTime.Now+":"+"waiting for connect...");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            while (true)
            {
                if (client7000t == null)
                {
                    client7000t = tcpListener7000.AcceptTcpClient();
                    port7000reset = true;
                }
                //Console.WriteLine(DateTime.Now+":"+"tcpListener7000.AcceptTcpClient");
                if (client6002t == null)
                {
                    client6002t = tcpListener6002.AcceptTcpClient();
                    port6002reset = true;
                }
                //Console.WriteLine(DateTime.Now+":"+"tcpListener6002.AcceptTcpClient");
                stopEvent.Reset();
                Thread dealTheClienThread = new Thread(DealTheClient);
                dealTheClienThread.Start(new Client(client6002t, client7000t));
                //ThreadPool.QueueUserWorkItem(DealTheClient, new Client(client6002t,client7000t));
                stopEvent.WaitOne();
                Thread.SpinWait(1);
            }

        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            string logMsg = string.Empty;
            logMsg = "Close time:" + DateTime.Now.ToString("G") + Environment.NewLine +
                  "Memory usage:" +
                  Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0;
            SiAuto.Main.LogError(logMsg);
            _mutex.ReleaseMutex();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
                SiAuto.Main.LogError("Restart:"+exception.ToString());
            Environment.Exit(1);
            //Restart();
        }
        static TcpClient client7000, client6002;
        static string client7000Address,client7000Port, client6002Address;
        static NetworkStream netStream7000, netStream6002;
        static StreamReader reader = null;
        static bool sendingTo6002 = false;

        internal class StateObject
        {
            public readonly TcpClient Client;
            public readonly NetworkStream NetStream;
            public readonly byte[] PackageBytes;

            public StateObject(TcpClient c, NetworkStream n, byte[] b)
            {
                this.Client = c;
                this.NetStream = n;
                PackageBytes = new byte[b.Length];
                Array.Copy(b,PackageBytes,b.Length);
            }
        }
        private static void DealTheClient(object state)
        {
            //Console.WriteLine(DateTime.Now+":"+"+DealTheClient");
            #region checkSendingTo6006
            Thread checkSendingTo6006 = new System.Threading.Thread
              (delegate()
              {

                  while (true)
                  {
                      
                      if(sendingTo6002)
                      { SiAuto.Main.LogText(Level.Debug, "SendingPackageTo6002", DateTime.Now.ToString(CultureInfo.InvariantCulture)); }
                      Thread.Sleep(60*1000);
                  }
                  
              });
            //checkSendingTo6006.Start();
            #endregion checkSendingTo6006
            Client clientState = (Client) state;
            Chilkat.Xml doc = new Chilkat.Xml(); ;
            

            if (port7000reset)
            {
                client7000 = clientState.getClient7000();
                client7000Address = IPAddress.Parse(((
                    IPEndPoint)client7000.Client.RemoteEndPoint).Address.ToString()).ToString();
                client7000Port = ((
                    IPEndPoint)client7000.Client.RemoteEndPoint).Port.ToString();
                netStream7000 = client7000.GetStream();
                port7000reset = false;
                Console.WriteLine(DateTime.Now+":"+client7000Address + ":7000 has connected");
                SiAuto.Main.LogText(Level.Debug, "7000 connected", client7000Address);
            }
            if (port6002reset)
            {
                 client6002 = clientState.getClient6002();
                 client6002Address = IPAddress.Parse(((
                    IPEndPoint)client6002.Client.RemoteEndPoint).Address.ToString()).ToString();
                 netStream6002 = client6002.GetStream();
                port6002reset = false;
                Console.WriteLine(DateTime.Now+":"+client6002Address + ":6002 has connected");
                SiAuto.Main.LogText(Level.Debug, "6002 connected", client6002Address);
                #region resend package to 6002 from bin.xml
                
                if (!File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bin.xml"))
                {

                    doc.Encoding = "iso-8859-1";
                    doc.Standalone = true;
                    doc.Tag = "root";
                    doc.SaveXml(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bin.xml");
                }
                else
                {
                    Console.WriteLine("reload last packages from bin.xml and send to port 6002 again");
                    SiAuto.Main.LogMessage("reload last packages from bin.xml and send to port 6002 again");
                    doc.LoadXml(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bin.xml");
                    for (int i = 0; i < doc.NumChildren - 1; i++)
                    {
                        byte[] packageSendTo6002 = HexToByte(doc.GetChildContentByIndex(i));
                        try
                        {
                            //netStream6002.Write(packageSendTo6002, 0, packageSendTo6002.Length);
                           
                            //Thread writeThread = new Thread(() => netStream6002.Write(packageSendTo6002, 0, packageSendTo6002.Length));
                            if (netStream6002.CanWrite)
                            {
                                //writeThread.Start();
                                StateObject stateObject = new StateObject(client6002,netStream6002,packageSendTo6002);
                                IAsyncResult result=netStream6002.BeginWrite(packageSendTo6002, 0, packageSendTo6002.Length,
                                    new AsyncCallback(SendingTo6002Callback), stateObject);
                                result.AsyncWaitHandle.WaitOne();
                                //netStream6002.Write(packageSendTo6002, 0, packageSendTo6002.Length);
                                sendingTo6002 = true;
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now+":"+client6002Address + ":6002 has disconnected");
                            SiAuto.Main.LogText(Level.Debug, "6002 has disconnected", ex.Message);
                            sendingTo6002 = false;
                            netStream6002.Close();
                            client6002.Close();
                            client6002t = null;
                            stopEvent.Set();
                            break;
                            client6002 = client6002t= tcpListener6002.AcceptTcpClient();
                            netStream6002 = client6002.GetStream();
                            if (netStream6002.CanWrite)
                            {
                                //writeThread.Start();
                                StateObject stateObject = new StateObject(client6002, netStream6002, packageSendTo6002);
                                IAsyncResult result = netStream6002.BeginWrite(packageSendTo6002, 0, packageSendTo6002.Length,
                                    new AsyncCallback(SendingTo6002Callback), stateObject);
                                result.AsyncWaitHandle.WaitOne();
                                //netStream6002.Write(packageSendTo6002, 0, packageSendTo6002.Length);
                                sendingTo6002 = true;
                            }
                        }
                        //if (netStream6002 != null)
                            //netStream6002.Flush();

                    }
                }
                #endregion resend package to 6002 from bin.xml
                SiAuto.Main.LogText(Level.Debug, "send package from xml to 6002", client6002Address);
            }
            
            if (reader == null)
            {
                reader = new StreamReader(netStream7000);
            }
            {
                
                uint message7000Counter = 0;
                int idCounter = 0;
                while (true)
                {
                    string message;
                    try
                    {
                        message = reader.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(DateTime.Now+":"+client7000Address + ":7000 has disconnected");
                        SiAuto.Main.LogText(Level.Debug, "7000 has disconnected", client7000Address);
                        netStream7000.Close();
                        client7000.Close();
                        client7000t  =null;
                        reader = null;
                        stopEvent.Set();
                        break;
                    }

                    if (message == null)
                    {
                        Console.WriteLine(DateTime.Now + ":" + client7000Address + ":7000 has disconnected2");
                        SiAuto.Main.LogText(Level.Debug, "7000 has disconnected2", client7000Address);
                        netStream7000.Close();
                        client7000.Close();
                        client7000t = null;
                        reader = null;
                        stopEvent.Set();
                        break;
                    }

                    message7000Counter++;
                    //Console.WriteLine(DateTime.Now+":"+client7000Address+String.Format(" >> [{0}] Message received: {1}", message7000Counter, message));
                    string[] stringSeparators = new string[] { ",","%%" };
                    string[] receiveStrings = message.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    int counter = 0;
                    UNITReportPacket recvReportPacket = new UNITReportPacket();
                    foreach (var receiveString in receiveStrings)
                    {
                        
                        //Console.WriteLine(DateTime.Now+":"+counter +":"+receiveString);
                        switch (counter)
                        {
                            case 0:
                                recvReportPacket.ID = receiveString;
                                break;
                            case 1:
                                recvReportPacket.GPSValid = receiveString;
                                break;
                            case 2:
                                recvReportPacket.DateTime = "20"+receiveString;
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
                    //byte[] packageSendTo6002;
                    using (var m = new MemoryStream())
                    {
                        string Head = "$CMD_H#";
                        string Tail = "$CMD_T#";

                        byte[] headBytes = Encoding.UTF8.GetBytes(Head);
                        byte[] HeadLength = ByteCountBigEndian(headBytes.Length);

                        byte[] Cmd_Type = new byte[]{2} ;
                        byte[] id = ByteCountBigEndian(idCounter++);
                        byte[] priority = new byte[]{5};
                        byte[] Attach_Type = new byte[]{1};

                        
                        #region avlsPackageFromPort7000_attach

                        byte[] uid = Encoding.UTF8.GetBytes(recvReportPacket.ID);
                        byte[] uidLength = ByteCountBigEndian(uid.Length);

                        byte[] statusBytes = new byte[]{0x0A};

                        DateTime time = DateTime.ParseExact(recvReportPacket.DateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, 
                            DateTimeStyles.AssumeUniversal |
                            DateTimeStyles.AdjustToUniversal);
                        long timeLong = Decimal.ToInt64(Decimal.Divide(time.Ticks - 621355968000000000, 10000));
                        byte[] timeBytes = BitConverter.GetBytes(timeLong).Reverse().ToArray();
                        byte[] GPSValid;
                        switch (recvReportPacket.GPSValid)
                        {
                            case "A"://valid.
                                GPSValid = Encoding.UTF8.GetBytes(recvReportPacket.GPSValid);
                                break;
                            case "L"://invalid.
                                GPSValid = Encoding.UTF8.GetBytes(recvReportPacket.GPSValid);
                                break;
                            default:
                                GPSValid = Encoding.UTF8.GetBytes(recvReportPacket.GPSValid);
                                break;
                        }
                        byte[] Loc = SendLocBackToWeb(recvReportPacket.Loc);
                        byte[] origin_lo = new byte[]{0x00,0x00,0x00,0x00};
                        byte[] origin_la = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                        byte[] judge = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                        byte[] speed = BitConverter.GetBytes(float.Parse(recvReportPacket.Speed)).Reverse().ToArray();
                        byte[] course = BitConverter.GetBytes(float.Parse(recvReportPacket.Dir)).Reverse().ToArray();

                        byte[] distance = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                        byte[] temperature = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                        byte[] voltage = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                        byte[] satellites = new byte[] { 0x00, 0x00 };
                        byte[] road_Length = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                        byte[] town_Length = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                        byte[] city_Length = new byte[] { 0x00, 0x00, 0x00, 0x00 };

                        byte[] option0 = Encoding.UTF8.GetBytes(recvReportPacket.Temp);
                        byte[] option0_Length = ByteCountBigEndian(option0.Length);
                        byte[] option1 = Encoding.UTF8.GetBytes(recvReportPacket.Status);
                        byte[] option1_Length = ByteCountBigEndian(option1.Length);
                        byte[] option2 = Encoding.UTF8.GetBytes(recvReportPacket.Event);
                        byte[] option2_Length = ByteCountBigEndian(option2.Length);
                        byte[] option3 = Encoding.UTF8.GetBytes(recvReportPacket.Message);
                        byte[] option3_Length = ByteCountBigEndian(option3.Length);

                        byte[] judegs = new byte[]
                        {
                            0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00
                        };

                        
                        int attachSize = uidLength.Length +
                                         uid.Length +
                                         statusBytes.Length +
                                         timeBytes.Length +
                                         GPSValid.Length +
                                         Loc.Length +
                                         origin_lo.Length +
                                         origin_la.Length +
                                         judge.Length +
                                         speed.Length +
                                         course.Length +
                                         distance.Length +
                                         temperature.Length +
                                         voltage.Length +
                                         satellites.Length +
                                         road_Length.Length +
                                         town_Length.Length +
                                         city_Length.Length +
                                         option0_Length.Length +
                                         option0.Length +
                                         option1_Length.Length +
                                         option1.Length +
                                         option2_Length.Length +
                                         option2.Length +
                                         option3_Length.Length +
                                         option3.Length +
                                         judegs.Length;
                        byte[] attachSizeBytes = ByteCountBigEndian(attachSize);




                        #endregion avlsPackageFromPort7000_attach

                        byte[] tailBytes = Encoding.UTF8.GetBytes(Tail);
                        byte[] TailLength = ByteCountBigEndian(tailBytes.Length);

                        #region write to memorystream
                        //head
                        m.Write(HeadLength, 0, HeadLength.Length);
                        m.Write(headBytes, 0, headBytes.Length);
                        m.Write(Cmd_Type, 0, Cmd_Type.Length);
                        m.Write(id, 0, id.Length);
                        m.Write(priority, 0, priority.Length);
                        m.Write(Attach_Type, 0, Attach_Type.Length);
                        //attach size
                        m.Write(attachSizeBytes, 0, attachSizeBytes.Length);
                        //attch
                        m.Write(uidLength, 0, uidLength.Length);
                        m.Write(uid, 0, uid.Length);
                        m.Write(statusBytes, 0, statusBytes.Length);
                        m.Write(timeBytes, 0, timeBytes.Length);
                        m.Write(GPSValid, 0, GPSValid.Length);
                        m.Write(Loc, 0, Loc.Length);
                        m.Write(origin_lo, 0, origin_lo.Length);
                        m.Write(origin_la, 0, origin_la.Length);
                        m.Write(judge, 0, judge.Length);
                        m.Write(speed, 0, speed.Length);
                        m.Write(course, 0, course.Length);
                        m.Write(distance, 0, distance.Length);
                        m.Write(temperature, 0, temperature.Length);
                        m.Write(voltage, 0, voltage.Length);
                        m.Write(satellites, 0, satellites.Length);
                        m.Write(road_Length, 0, road_Length.Length);
                        m.Write(town_Length, 0, town_Length.Length);
                        m.Write(city_Length, 0, city_Length.Length);
                        m.Write(option0_Length, 0, option0_Length.Length);
                        m.Write(option0, 0, option0.Length);
                        m.Write(option1_Length, 0, option1_Length.Length);
                        m.Write(option1, 0, option1.Length);
                        m.Write(option2_Length, 0, option2_Length.Length);
                        m.Write(option2, 0, option2.Length);
                        m.Write(option3_Length, 0, option3_Length.Length);
                        m.Write(option3, 0, option3.Length);
                        m.Write(judegs, 0, judegs.Length);

                        //tail
                        m.Write(TailLength,0,TailLength.Length);
                        m.Write(tailBytes,0,tailBytes.Length);
                        #endregion write to memorystream

                        byte[] packageSendTo6002 = m.ToArray();
                        doc.LoadXml(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bin.xml");
                        bool xFound=doc.SearchForTag2(null, recvReportPacket.ID);
                        if (xFound)
                        {
                            //doc.UpdateChildContent(recvReportPacket.ID, ); //error use
                            doc.Content = ToHexString(packageSendTo6002);
                            doc.GetRoot2();
                        }
                        else
                        {
                            doc.NewChild2(recvReportPacket.ID, ToHexString(packageSendTo6002));
                            //doc.NewChild2(recvReportPacket.ID, "1");
                        }
                        doc.SaveXml(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bin.xml");
                        try
                        {
                            //Thread writeThread = new Thread(() => netStream6002.Write(packageSendTo6002, 0, packageSendTo6002.Length));
                            if (netStream6002.CanWrite)
                            {
                                //netStream6002.Write(packageSendTo6002, 0, packageSendTo6002.Length);
                                StateObject stateObject = new StateObject(client6002, netStream6002, packageSendTo6002);
                                IAsyncResult result = netStream6002.BeginWrite(packageSendTo6002, 0, packageSendTo6002.Length,
                                    new AsyncCallback(SendingTo6002Callback), stateObject);
                                result.AsyncWaitHandle.WaitOne();
                                SiAuto.Main.LogText(Level.Debug, recvReportPacket.ID + ":send msg to 6002:" + recvReportPacket.Event + ":" + recvReportPacket.Message, message);
                                sendingTo6002 = true;
                            }
                            
                            
                            
                            //writeThread.Start();
                            //SiAuto.Main.LogText(Level.Debug, recvReportPacket.ID + ":send msg to 6002:" + recvReportPacket.Event + ":" + recvReportPacket.Message, message);
                            /*
                            if (recvReportPacket.Event.Equals("150") ||
                                recvReportPacket.Message.Contains("p_prohibited") ||
                                recvReportPacket.Message.Contains("patrol_location") ||
                                recvReportPacket.Message.Contains("stay_over_specific_time"))
                            {
                                Console.WriteLine(DateTime.Now.ToString("G"));
                                Console.WriteLine(message);
                            }
                            */
                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now + ":" + client6002Address + ":6002 has disconnected");
                            SiAuto.Main.LogText(Level.Debug, "6002 has disconnected", ex.Message);
                            sendingTo6002 = false;
                            netStream6002.Close();
                            client6002.Close();
                            client6002t = null;
                            stopEvent.Set();
                            break;
                            client6002 = client6002t=tcpListener6002.AcceptTcpClient();
                            netStream6002 = client6002.GetStream();
                            if (netStream6002.CanWrite)
                            {
                                //writeThread.Start();
                                StateObject stateObject = new StateObject(client6002, netStream6002, packageSendTo6002);
                                IAsyncResult result = netStream6002.BeginWrite(packageSendTo6002, 0, packageSendTo6002.Length,
                                    new AsyncCallback(SendingTo6002Callback), stateObject);
                                result.AsyncWaitHandle.WaitOne();
                                //netStream6002.Write(packageSendTo6002, 0, packageSendTo6002.Length);
                                sendingTo6002 = true;
                            }
                        }
                        //if(netStream6002!=null)
                            //netStream6002.Flush();
                        //Thread writeThread = new Thread(() => netStream6002.Write(packageSendTo6002, 0, packageSendTo6002.Length));
                        //writeThread.Start();
                    }
                    //Console.WriteLine(DateTime.Now.ToString("s")+"send to port 6002");
                    //Console.WriteLine("-------------------------------------------");
                    //Console.WriteLine(Encoding.ASCII.GetString(packageSendTo6002));
                    //Console.WriteLine("-------------------------------------------");
                    //Console.WriteLine(BitConverter.ToString(packageSendTo6002));
                    //Console.WriteLine("-------------------------------------------");


                    Thread.SpinWait(1);
                }
            }
            //Console.WriteLine(DateTime.Now+":"+"-DealTheClient");
        }

        static void SendingTo6002Callback(IAsyncResult ar)
        {
            StateObject stateObject = (StateObject) ar.AsyncState;
            try
            {
                stateObject.NetStream.EndWrite(ar);
            }
            catch (Exception ex)
            {

                Console.WriteLine(DateTime.Now + ":" + client6002Address + ":6002 has disconnected");
                SiAuto.Main.LogText(Level.Debug, "6002 has disconnected", ex.Message);
                sendingTo6002 = false;
                stateObject.NetStream.Close();
                stateObject.Client.Close();
                //client6002t = null;
                //stopEvent.Set();
                //break;
                client6002 = client6002t = tcpListener6002.AcceptTcpClient();
                netStream6002 = client6002.GetStream();
                if (netStream6002.CanWrite)
                {
                    //writeThread.Start();
                    StateObject stateOO = new StateObject(client6002, netStream6002, stateObject.PackageBytes);
                    IAsyncResult result = netStream6002.BeginWrite(stateObject.PackageBytes, 0, stateObject.PackageBytes.Length,
                        new AsyncCallback(SendingTo6002Callback), stateOO);
                    result.AsyncWaitHandle.WaitOne();
                    //netStream6002.Write(packageSendTo6002, 0, packageSendTo6002.Length);
                    sendingTo6002 = true;
                }
            }
        }
        static byte[] ByteCountBigEndian(int a)
        {
            byte[] b = BitConverter.GetBytes(a);
            return b.Reverse().ToArray();
        }
        static float ConvertNmea0183ToUtm(float f)
        {
            int i = (int)(f / 100);
            return (f / 100 - i) * 100 / 60 + i;
        }
        static byte[] SendLocBackToWeb(string recev)
        {
            char[] delimiterChars = { 'N', 'E', 'S', 'W' };
            string[] tmp1 = recev.Split(delimiterChars);
            float lat = ConvertNmea0183ToUtm(float.Parse(tmp1[1]));
            float lon = ConvertNmea0183ToUtm(float.Parse(tmp1[2]));
            byte[] latBytes = netduino.BitConverter.GetBytes(lat, netduino.BitConverter.ByteOrder.BigEndian);
            byte[] lonBytes = netduino.BitConverter.GetBytes(lon, netduino.BitConverter.ByteOrder.BigEndian);
            var m = new MemoryStream();
            m.Write(lonBytes, 0, lonBytes.Length);
            m.Write(latBytes, 0, latBytes.Length);
            return m.ToArray();

        }
        public static string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }
        private static byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }
        private static bool isclosing = false;
        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {

            // Put your own handler here

            switch (ctrlType)
            {

                case CtrlTypes.CTRL_C_EVENT:

                    isclosing = true;

                    Console.WriteLine("CTRL+C received!");
                    SiAuto.Main.LogText(Level.Debug, "Closing", "CTRL+C received!");
                    break;



                case CtrlTypes.CTRL_BREAK_EVENT:

                    isclosing = true;

                    Console.WriteLine("CTRL+BREAK received!");
                    SiAuto.Main.LogText(Level.Debug, "Closing", "CTRL+BREAK received!");
                    break;



                case CtrlTypes.CTRL_CLOSE_EVENT:

                    isclosing = true;

                    Console.WriteLine("Program being closed!");
                    SiAuto.Main.LogText(Level.Debug, "Closing", "Program being closed!");
                    break;



                case CtrlTypes.CTRL_LOGOFF_EVENT:

                case CtrlTypes.CTRL_SHUTDOWN_EVENT:

                    isclosing = true;

                    Console.WriteLine("User is logging off!");
                    SiAuto.Main.LogText(Level.Debug, "Closing", "User is logging off!");
                    break;



            }

            return true;

        }


        private static void Restart()
        {
            //Process.Start(AppDomain.CurrentDomain.BaseDirectory + "Client.exe");

            //some time to start the new instance.
            //Thread.Sleep(2000);

            //Environment.Exit(-1);//Force termination of the current process.

            System.Windows.Forms.Application.Restart();
            Process.GetCurrentProcess().Kill();
        }




        #region unmanaged

        // Declare the SetConsoleCtrlHandler function

        // as external and receiving a delegate.



        [DllImport("Kernel32")]

        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);



        // A delegate type to be used as the handler routine

        // for SetConsoleCtrlHandler.

        public delegate bool HandlerRoutine(CtrlTypes CtrlType);



        // An enumerated type for the control messages

        // sent to the handler routine.

        public enum CtrlTypes
        {

            CTRL_C_EVENT = 0,

            CTRL_BREAK_EVENT,

            CTRL_CLOSE_EVENT,

            CTRL_LOGOFF_EVENT = 5,

            CTRL_SHUTDOWN_EVENT

        }



        #endregion

    }
}
namespace netduino
{
    public static class BitConverter
    {
        public static byte[] GetBytes(uint value)
        {
            return new byte[4] { 
                    (byte)(value & 0xFF), 
                    (byte)((value >> 8) & 0xFF), 
                    (byte)((value >> 16) & 0xFF), 
                    (byte)((value >> 24) & 0xFF) };
        }

        public static unsafe byte[] GetBytes(float value)
        {
            uint val = *((uint*)&value);
            return GetBytes(val);
        }

        public static unsafe byte[] GetBytes(float value, ByteOrder order)
        {
            byte[] bytes = GetBytes(value);
            if (order != ByteOrder.LittleEndian)
            {
                System.Array.Reverse(bytes);
            }
            return bytes;
        }

        public static uint ToUInt32(byte[] value, int index)
        {
            return (uint)(
                value[0 + index] << 0 |
                value[1 + index] << 8 |
                value[2 + index] << 16 |
                value[3 + index] << 24);
        }

        public static unsafe float ToSingle(byte[] value, int index)
        {
            uint i = ToUInt32(value, index);
            return *(((float*)&i));
        }

        public static unsafe float ToSingle(byte[] value, int index, ByteOrder order)
        {
            if (order != ByteOrder.LittleEndian)
            {
                System.Array.Reverse(value, index, value.Length);
            }
            return ToSingle(value, index);
        }

        public enum ByteOrder
        {
            LittleEndian,
            BigEndian
        }

        static public bool IsLittleEndian
        {
            get
            {
                unsafe
                {
                    int i = 1;
                    char* p = (char*)&i;

                    return (p[0] == 1);
                }
            }
        }
    }
}

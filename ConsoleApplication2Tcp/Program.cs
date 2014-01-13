using System;
using System.IO;
using System.Linq;
using System.Text;
using keeplive;
using System.Net.Sockets;
using System.Threading;


namespace ConsoleApplication2Tcp
{
    
    class Program
    {
        static TcpClient t = null;
        static NetworkStream myNetworkStream = null;
        static byte[] myWriteBuffer,myReadBuffer = new byte[1024];
        static int counter = 0;
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        static void Main(string[] args)
        {
            //AsyncTcpServer test = new AsyncTcpServer("192.168.56.1", "12345");
            AsyncTcpServer.Client dsdf = new AsyncTcpServer.Client("1","2",3,new TcpClient());
            //AsyncTcpClient test = new AsyncTcpClient("192.168.56.101",12345,"us-ascii");
            byte[] xxx=SendLocBackToWeb("S2446.5281W01234.5678");
            byte[] a = new byte[]{07,00,00,00};
            byte[] b = ByteCountLittleEndian(10);//Little Endian
            byte[] c = ByteCountBigEndian(10);//Big Endian
            string avlsHead = "$CMD_H#";
            byte[] head = Encoding.ASCII.GetBytes(avlsHead);
            byte[] headLength = ByteCountBigEndian(head.Count());
            using (var m = new MemoryStream())
            {
                m.Write(headLength, 0, headLength.Count());
                m.Write(head,0,head.Count());
                byte[] d = m.ToArray();

            }

            //test.WriteBytes(a);
            //string test = "你是誰";
            //Console.WriteLine(Encoding.Default.HeaderName);
            //Console.WriteLine(Encoding.UTF8.GetByteCount(test));
            //Console.WriteLine(Encoding.ASCII.GetByteCount(test));
            //Connect();
            //AsyncTcpClient a = new AsyncTcpClient("", 0, "us-ascii");
            while (false)
            {
                write();
                read(); 
                Thread.Sleep(7000);
            }
        }
        static private float ConvertNmea0183ToUtm(float f)
        {
            int i = (int)(f / 100);
            return (f / 100 - i) * 100 / 60 + i;
        }

        static public byte[] SendLocBackToWeb(string recev)
        {
            char[] delimiterChars = { 'N', 'E', 'S', 'W' };
            string[] tmp1 = recev.Split(delimiterChars);
            float lat = ConvertNmea0183ToUtm(float.Parse(tmp1[1]));
            float lon = ConvertNmea0183ToUtm(float.Parse(tmp1[2]));
            byte[] latBytes = netduino.BitConverter.GetBytes(lat,netduino.BitConverter.ByteOrder.BigEndian);
            byte[] lonBytes = netduino.BitConverter.GetBytes(lon,netduino.BitConverter.ByteOrder.BigEndian);
            var m = new MemoryStream();
            m.Write(lonBytes, 0, lonBytes.Count());
            m.Write(latBytes, 0, latBytes.Count());
            return m.ToArray();

        }

        static byte[] ByteCountLittleEndian(int a)
        {
            return BitConverter.GetBytes(a);
        }
        static byte[] ByteCountBigEndian(int a)
        {
            byte[] b= BitConverter.GetBytes(a);
            return b.Reverse().ToArray();
        }
        static byte[] int_to_hex_little_endian(int length)
        {
            var reversedBytes = System.Net.IPAddress.NetworkToHostOrder(length);
            string hex = reversedBytes.ToString("x");
            string trimmed = hex.Substring(0, 4);
            //System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            //Byte[] bytes = encoding.GetBytes(trimmed);
            byte[] bytes = StringToByteArray(trimmed);
            //string str = System.Text.Encoding.ASCII.GetString(bytes);
            return bytes;
            //return HexAsciiConvert(trimmed);
        }
        static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 4), 16))
                             .ToArray();
        }
        private static void read()
        {
            if(myNetworkStream!=null)
                if (myNetworkStream.CanRead)
                {
                    try
                    {
                        myNetworkStream.BeginRead(myReadBuffer, 0, myReadBuffer.Length,
                            new AsyncCallback(AsynReadCallBack), myNetworkStream);
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("AsynRead-" + ex.Message);
                        if (t != null && myNetworkStream != null)
                        {
                            t.Close();
                            myNetworkStream.Close();
                        }
                        Connect();

                        if (t != null && t.Client != null)
                        {
                            myNetworkStream = t.GetStream();
                            read();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                }
        }

        private static void AsynReadCallBack(IAsyncResult ar)
        {
            try
            {
                String myCompleteMessage = "";
                int numberOfBytesRead;

                numberOfBytesRead = myNetworkStream.EndRead(ar);
                myCompleteMessage =
                    String.Concat(myCompleteMessage, Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                // message received may be larger than buffer size so loop through until you have it all.
                while (myNetworkStream.DataAvailable)
                {

                    myNetworkStream.BeginRead(myReadBuffer, 0, myReadBuffer.Length,
                                                               new AsyncCallback(AsynReadCallBack),
                                                               myNetworkStream);

                }

                // Print out the received message to the console.
                Console.WriteLine("You received the following message : " +
                                            myCompleteMessage);
            }
            catch (Exception ex )
            {

                Console.WriteLine("AsynReadCallBack-" + ex.Message);
                if (t != null && myNetworkStream != null)
                {
                    t.Close();
                    myNetworkStream.Close();
                }
                Connect();

                if (t != null && t.Client != null)
                {
                    myNetworkStream = t.GetStream();
                    read();
                }
            }
        }

        private static void write()
        {
            if (myNetworkStream != null)
                if (myNetworkStream.CanWrite)
                {
                    try
                    {
                        myWriteBuffer = Encoding.ASCII.GetBytes(counter + ":" + "Are you receiving this message?");
                        myNetworkStream.BeginWrite(myWriteBuffer, 0, myWriteBuffer.Length,
                                                                     new AsyncCallback(AsynSendCallBack),
                                                                     myNetworkStream);
                    
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("AsynSend-" + ex.Message);
                        if (t != null  && myNetworkStream != null)
                        {
                            t.Close();
                            myNetworkStream.Close();
                        }
                        Connect();
                       
                        if (t != null && t.Client != null)
                        {
                            myNetworkStream = t.GetStream();
                            write();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Sorry.  You cannot write to this NetworkStream.");
                }
        }

        private static void AsynSendCallBack(IAsyncResult ar)
        {
            try
            {
                myNetworkStream = (NetworkStream)ar.AsyncState;
                myNetworkStream.EndWrite(ar);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now+":AsynSendCallBack-" + ex.Message);
                if (t != null && myNetworkStream != null)
                {
                    t.Close();
                    myNetworkStream.Close();
                }
                t = new TcpClient(AddressFamily.InterNetwork);
                Connect();
                
                myNetworkStream = t.GetStream();
                write();
            }
        }


        public static void Connect()
        {
            t = new TcpClient();
            connectDone.Reset();
            t.BeginConnect("192.168.64.128", 12345, ConnectCallBack, t);
            connectDone.WaitOne();
            Keeplive.keep(t.Client);
        }

        private static void ConnectCallBack(IAsyncResult ar)
        {
            try
            {
                if (t != null && t.Client != null)
                {
                    myNetworkStream = t.GetStream();
                    t.EndConnect(ar);
                    connectDone.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now+":ConnectCallback-" + ex.Message);
                if (t != null  /*&& netstream != null*/)
                {

                    t.Close();
                    //netstream.Close();
                }

                t = new TcpClient();
                Connect();
            }
        }
    }
}

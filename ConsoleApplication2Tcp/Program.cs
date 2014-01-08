using System;
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
            string test = "你是誰";
            Console.WriteLine(Encoding.Default.HeaderName);
            Console.WriteLine(Encoding.UTF8.GetByteCount(test));
            Console.WriteLine(Encoding.ASCII.GetByteCount(test));
            Connect();
            AsyncTcpClient a = new AsyncTcpClient("", 0, "us-ascii");
            while (true)
            {
                write();
                read(); 
                Thread.Sleep(7000);
            }
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

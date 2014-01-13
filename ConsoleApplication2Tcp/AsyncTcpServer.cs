using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            private TcpClient _tcpClient;

            public Client(string id, string ip, int port,TcpClient cliet)
            {
                _id = id;
                _ip = ip;
                _port = port;
                _tcpClient = cliet;
            }

            public string GetId()
            {
                return _id;
            }

            public string GetIp()
            {
                return _ip;
            }

            public int GetPort()
            {
                return _port;
            }

            public TcpClient GeTcpClient()
            {
                return _tcpClient;
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

            _clientList.Add(clientID, new Client(clientID, clientIPAddress, clientPort, client));

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
        /**
     * convert NMEA0183 latitude and longitude from degree minute, to degree
     * i.e. ddmm.mmmm -> dd.dddddd
     * @param f
     * @return
     * @auther Michael
     * @since Aug 24, 2006
     */
        private float ConvertNmea0183ToUtm(float f)
        {
            int i = (int)(f / 100);
            return (f / 100 - i) * 100 / 60 + i;
        }

        public byte[] SendLocBackToWeb(string loc)
        {
            char[] delimiterChars = { 'N','E','S','W'};
            string[] tmp1 = loc.Split(delimiterChars);
            float lat = ConvertNmea0183ToUtm(float.Parse(tmp1[1]));
            float lon = ConvertNmea0183ToUtm(float.Parse(tmp1[2]));
            byte[] latBytes = netduino.BitConverter.GetBytes(lat, netduino.BitConverter.ByteOrder.BigEndian);
            byte[] lonBytes = netduino.BitConverter.GetBytes(lon, netduino.BitConverter.ByteOrder.BigEndian);
            var m = new MemoryStream();
            m.Write(lonBytes,0,lonBytes.Count());
            m.Write(latBytes,0,latBytes.Count());
            return m.ToArray();

        }
        private string AddressOf(GCHandle handle)
        {
            IntPtr pointer = GCHandle.ToIntPtr(handle);
            return "0x" + pointer.ToString("X");
        }

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

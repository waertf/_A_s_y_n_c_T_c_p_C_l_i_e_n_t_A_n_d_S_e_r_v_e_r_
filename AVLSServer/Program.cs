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

                        #region avlsPackageFromPort7000_attach

                        byte[] uid = Encoding.ASCII.GetBytes(recvReportPacket.ID);
                        byte[] uidLength = ByteCountBigEndian(uid.Count());

                        byte[] statusBytes = new byte[]{0x0A};

                        DateTime time = DateTime.ParseExact(recvReportPacket.DateTime, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.AssumeUniversal);
                        long timeLong = time.Ticks/TimeSpan.TicksPerMillisecond;
                        byte[] timeBytes = BitConverter.GetBytes(timeLong).Reverse().ToArray();
                        byte[] GPSValid;
                        switch (recvReportPacket.GPSValid)
                        {
                            case "A"://valid.
                                GPSValid = Encoding.ASCII.GetBytes(recvReportPacket.GPSValid);
                                break;
                            case "L"://invalid.
                                GPSValid = Encoding.ASCII.GetBytes(recvReportPacket.GPSValid);
                                break;
                            default:
                                GPSValid = Encoding.ASCII.GetBytes(recvReportPacket.GPSValid);
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

                        byte[] option0 = Encoding.ASCII.GetBytes(recvReportPacket.Temp);
                        byte[] option0_Length = ByteCountBigEndian(option0.Count());
                        byte[] option1 = Encoding.ASCII.GetBytes(recvReportPacket.Status);
                        byte[] option1_Length = ByteCountBigEndian(option1.Count());
                        byte[] option2 = Encoding.ASCII.GetBytes(recvReportPacket.Event);
                        byte[] option2_Length = ByteCountBigEndian(option2.Count());
                        byte[] option3 = Encoding.ASCII.GetBytes(recvReportPacket.Message);
                        byte[] option3_Length = ByteCountBigEndian(option3.Count());

                        byte[] judegs_length = new byte[] { 0x00, 0x00, 0x00, 0x00 };

                        int attachSize = uidLength.Count() +
                                         uid.Count() +
                                         statusBytes.Count() +
                                         timeBytes.Count() +
                                         GPSValid.Count() +
                                         Loc.Count() +
                                         origin_lo.Count() +
                                         origin_la.Count() +
                                         judge.Count() +
                                         speed.Count() +
                                         course.Count() +
                                         distance.Count() +
                                         temperature.Count() +
                                         voltage.Count() +
                                         satellites.Count() +
                                         road_Length.Count() +
                                         town_Length.Count() +
                                         city_Length.Count() +
                                         option0_Length.Count() +
                                         option0.Count() +
                                         option1_Length.Count() +
                                         option1.Count() +
                                         option2_Length.Count() +
                                         option2.Count() +
                                         option3_Length.Count() +
                                         option3.Count() +
                                         judegs_length.Count();
                        byte[] attachSizeBytes = ByteCountBigEndian(attachSize);




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
            m.Write(lonBytes, 0, lonBytes.Count());
            m.Write(latBytes, 0, latBytes.Count());
            return m.ToArray();

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

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using keeplive;

namespace ConsoleApplication2Tcp
{
    class AsyncTcpClient
    {
        public string ReceiveString;
        public string Encoding;
        private const string Finish = "Finish";
        //Encoding list
        //http://msdn.microsoft.com/zh-tw/library/system.text.encoding%28v=vs.110%29.aspx
        internal class TcpClientStateObject
        {
            public TcpClient Client = null;
            public NetworkStream NetworkStream = null;
            public string ServerIp = string.Empty;
            public int ServerPort = 0, FBytesRead;
            public byte[] WriteBytes, ReadDataBytes, ReadLengthBytes, ReadDataBytesByDelimiter = new byte[BufferSize];
            public string WriteString, ReadString, Delimiter;
            public const int BufferSize = 1024;
            public StringBuilder Sb = new StringBuilder();

            public TcpClientStateObject(TcpClient client,string ip , int port)
            {
                Client = client;
                ServerIp = ip;
                ServerPort = port;
            }
        }

        private readonly ManualResetEvent _connectDone =
            new ManualResetEvent(false);
        private readonly ManualResetEvent _readDone =
            new ManualResetEvent(false);
        private readonly ManualResetEvent _writetDone =
            new ManualResetEvent(false);

        private TcpClientStateObject _tcpStateObject;
        public AsyncTcpClient( string ip, int port,string encoding)
        {
            Encoding = encoding;
            //var client = new TcpClient();
            var tcpStateObject = new TcpClientStateObject(new TcpClient(), ip, port);
            _tcpStateObject = tcpStateObject;
            Connect(tcpStateObject);
        }

        public string ReadByDelimiter(string delimiter)
        {
            _readDone.Reset();
            if(_tcpStateObject.NetworkStream!=null)
                if (_tcpStateObject.NetworkStream.CanRead)
                {
                    _tcpStateObject.Delimiter = delimiter;
                    try
                    {
                        _tcpStateObject.NetworkStream.BeginRead(_tcpStateObject.ReadDataBytesByDelimiter, 0,
                            _tcpStateObject.ReadDataBytesByDelimiter.Length, ReadByDelimiterCallBack, _tcpStateObject);
                        _readDone.WaitOne();
                        return Finish;
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(DateTime.Now + ":ReadByDelimiter-" + ex.Message);
                        if (_tcpStateObject.NetworkStream != null)
                            _tcpStateObject.NetworkStream.Close();
                        if (_tcpStateObject.Client != null)
                            _tcpStateObject.Client.Close();
                        _tcpStateObject.Client = new TcpClient();
                        Connect(_tcpStateObject);
                        if (_tcpStateObject.Client != null)
                        {
                            _tcpStateObject.NetworkStream = _tcpStateObject.Client.GetStream();
                            ReadByDelimiter(delimiter);
                        }
                        return DateTime.Now + ":ReadByDelimiter-" + ex.Message;
                    }
                }
                else
                {
                    return "cannot read from networkstream in function ReadByDelimiter";
                }
            else
            {
                return "cannot use networkstream in function ReadByDelimiter";
            }
        }

        private void ReadByDelimiterCallBack(IAsyncResult ar)
        {
            TcpClientStateObject t = (TcpClientStateObject)ar.AsyncState;
            try
            {
                int numberOfBytesRead = t.NetworkStream.EndRead(ar);
                if (numberOfBytesRead > 0)
                {
                    t.Sb.Append(System.Text.Encoding.GetEncoding(Encoding).GetString(_tcpStateObject.ReadDataBytesByDelimiter, 0, numberOfBytesRead));
                    t.ReadString = t.Sb.ToString();
                    if (t.ReadString.IndexOf(t.Delimiter, StringComparison.Ordinal) > -1)
                    {
                        Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    t.ReadString.Length, t.ReadString);
                        ReceiveString = t.ReadString;
                        _readDone.Set();
                    }
                    else
                    {
                        ReadByDelimiter(t.Delimiter);
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(DateTime.Now + ":ReadByDelimiterCallBack-" + ex.Message);
                if (t.NetworkStream != null)
                    t.NetworkStream.Close();
                if (t.Client != null)
                    t.Client.Close();
                t.Client = new TcpClient();
                Connect(t);
                if (t.Client != null)
                {
                    t.NetworkStream = t.Client.GetStream();
                    ReadByDelimiter(t.Delimiter);
                }
            }
        }

        public string ReadByLengthPrefix(int lengthOfPrefixInByte)
        {
            if (!Encoding.Equals("us-ascii"))
                return "only support us-ascii";
            else
            {
                _readDone.Reset();
                if (_tcpStateObject.NetworkStream != null)
                    if (_tcpStateObject.NetworkStream.CanRead)
                    {
                        try
                        {
                            _tcpStateObject.ReadLengthBytes = new byte[lengthOfPrefixInByte];
                            _tcpStateObject.NetworkStream.BeginRead(_tcpStateObject.ReadLengthBytes, 0,
                                _tcpStateObject.ReadLengthBytes.Length, ReadByLengthPrefixCallBack, _tcpStateObject);
                            _readDone.WaitOne();
                            return Finish;
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine(DateTime.Now + ":ReadByLengthPrefix-" + ex.Message);
                            if (_tcpStateObject.NetworkStream != null)
                                _tcpStateObject.NetworkStream.Close();
                            if (_tcpStateObject.Client != null)
                                _tcpStateObject.Client.Close();
                            _tcpStateObject.Client = new TcpClient();
                            Connect(_tcpStateObject);
                            if (_tcpStateObject.Client != null)
                            {
                                _tcpStateObject.NetworkStream = _tcpStateObject.Client.GetStream();
                                ReadByLengthPrefix(lengthOfPrefixInByte);
                            }
                            return DateTime.Now + ":ReadByLengthPrefix-" + ex.Message;
                        }
                    }
                    else
                    {
                        return "cannot read from networkstream in function ReadByLengthPrefix";
                    }
                else
                {
                    return "cannot use networkstream in function ReadByLengthPrefix";
                }
                
            }
            
        }

        private void ReadByLengthPrefixCallBack(IAsyncResult ar)
        {
            TcpClientStateObject t = (TcpClientStateObject)ar.AsyncState;
            try
            {
                int numberOfBytesRead = t.NetworkStream.EndRead(ar);
                if(numberOfBytesRead!=t.ReadLengthBytes.Length)
                    throw new Exception("numberOfBytesRead!=t.ReadLengthBytes.Length");
                int dataLength = GetLittleEndianIntegerFromByteArray(t.ReadLengthBytes, 0);
                if (dataLength.Equals(0))
                {
                    throw new Exception("data_length=0");
                }
                t.FBytesRead = 0;    
                t.ReadDataBytes = new byte[dataLength];
                t.NetworkStream.BeginRead(t.ReadDataBytes, 0, t.ReadDataBytes.Length,ReadDataCallBack,t);
            }
            catch (Exception ex)
            {

                Console.WriteLine(DateTime.Now + ":ReadByLengthPrefixCallBack-" + ex.Message);
                if (t.NetworkStream != null)
                    t.NetworkStream.Close();
                if (t.Client != null)
                    t.Client.Close();
                t.Client = new TcpClient();
                Connect(t);
                if (t.Client != null)
                {
                    t.NetworkStream = t.Client.GetStream();
                    ReadByLengthPrefix(t.ReadLengthBytes.Length);
                }
            }
        }

        private void ReadDataCallBack(IAsyncResult ar)
        {
            TcpClientStateObject t = (TcpClientStateObject)ar.AsyncState;
            try
            {
                int numberOfBytesRead = t.NetworkStream.EndRead(ar);
                if (0 == numberOfBytesRead)
                    throw new Exception("0 == numberOfBytesRead");
                t.FBytesRead += numberOfBytesRead;
                if (t.FBytesRead < t.ReadDataBytes.Length)
                {
                    t.NetworkStream.BeginRead(t.ReadDataBytes, t.FBytesRead, t.ReadDataBytes.Length - t.FBytesRead,
                        ReadDataCallBack, t);
                }
                else
                {
                    // Should be exactly the right number read now.
                    if(!t.FBytesRead.Equals(t.ReadDataBytes.Length))
                        throw new Exception("!t.fBytesRead.Equals(t.ReadDataBytes.Length)");
                    t.ReadString = System.Text.Encoding.GetEncoding(Encoding).GetString(t.ReadDataBytes);
                    Console.WriteLine("Read: Length: {0}, Data: {1}", t.ReadString.Length, t.ReadString);
                    ReceiveString = t.ReadString;
                    _readDone.Set();
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(DateTime.Now + ":ReadDataCallBack-" + ex.Message);
                if (t.NetworkStream != null)
                    t.NetworkStream.Close();
                if (t.Client != null)
                    t.Client.Close();
                t.Client = new TcpClient();
                Connect(t);
                if (t.Client != null)
                {
                    t.NetworkStream = t.Client.GetStream();
                    ReadByLengthPrefix(t.ReadLengthBytes.Length);
                }
            }
        }

        private void Connect(TcpClientStateObject tcpStateObject)
        {
            try
            {
                _connectDone.Reset();
                tcpStateObject.Client.BeginConnect(tcpStateObject.ServerIp, tcpStateObject.ServerPort, ConnectCallBack,
                    tcpStateObject);
                _connectDone.WaitOne();
                Keeplive.keep(tcpStateObject.Client.Client);
            }
            catch (Exception ex)
            {

                Console.WriteLine(DateTime.Now + ":Connect-" + ex.Message);
            }
           

        }

        private void ConnectCallBack(IAsyncResult ar)
        {
            TcpClientStateObject t = (TcpClientStateObject) ar.AsyncState;
            try
            {
                if (t.Client != null)
                {
                    t.NetworkStream = t.Client.GetStream();
                    t.Client.EndConnect(ar);
                    _tcpStateObject = t;
                    _connectDone.Set();
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(DateTime.Now + ":ConnectCallback-" + ex.Message);
                if (t.Client != null)
                {
                    t.Client.Close();
                }
                if (t.NetworkStream != null)
                {
                    t.NetworkStream.Close();
                }
                t.Client = new TcpClient();
                var stateObject = new TcpClientStateObject(t.Client,t.ServerIp,t.ServerPort);
                Connect(stateObject);
            }
        }

        private string WriteString(string str)
        {
            
            if(_tcpStateObject.NetworkStream!=null)
                if (_tcpStateObject.NetworkStream.CanWrite)
                {
                    _writetDone.Reset();
                    _tcpStateObject.WriteString = str;
                    _tcpStateObject.WriteBytes = System.Text.Encoding.GetEncoding(Encoding).GetBytes(str);
                    try
                    {
                        _tcpStateObject.NetworkStream.BeginWrite(_tcpStateObject.WriteBytes, 0, _tcpStateObject.WriteBytes.Length, WriteStringCallBack,
                            _tcpStateObject);
                        _writetDone.WaitOne();
                        return Finish;
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(DateTime.Now + ":WriteString-" + ex.Message);
                        if(_tcpStateObject.NetworkStream!=null)
                            _tcpStateObject.NetworkStream.Close();
                        if(_tcpStateObject.Client!=null)
                            _tcpStateObject.Client.Close();
                        _tcpStateObject.Client = new TcpClient();
                        Connect(_tcpStateObject);
                        if (_tcpStateObject.Client != null)
                        {
                            _tcpStateObject.NetworkStream = _tcpStateObject.Client.GetStream();
                            WriteString(str);
                        }
                        return DateTime.Now + ":WriteString-" + ex.Message;
                    }
                }
                else
                {
                    return "cannot write from networkstream in function WriteString";
                }
            else
            {
                return "cannot write networkstream in function WriteString";
            }
        }

        private void WriteStringCallBack(IAsyncResult ar)
        {
            TcpClientStateObject t = (TcpClientStateObject)ar.AsyncState;
            try
            {
                t.NetworkStream.EndWrite(ar);
                _writetDone.Set();
            }
            catch (Exception ex)
            {

                Console.WriteLine(DateTime.Now + ":WriteCallBack-" + ex.Message);
                if (t.NetworkStream != null)
                    t.NetworkStream.Close();
                if (t.Client != null)
                    t.Client.Close();
                t.Client = new TcpClient();
                Connect(t);
                if (t.Client != null)
                {
                    t.NetworkStream = t.Client.GetStream();
                    WriteString(t.WriteString);
                }
            }
        }

        public string WriteStringwithPrefixLength(int lengthOfPrefixInByte,string str)
        {
            if (!Encoding.Equals("us-ascii"))
                return "only support us-ascii";
            else
            {
                WriteBytes(data_append_dataLength(lengthOfPrefixInByte, str));
                return Finish;
            }
            
        }
        public string WriteBytes(byte[] str)
        {
            
            if (_tcpStateObject.NetworkStream != null)
                if (_tcpStateObject.NetworkStream.CanWrite)
                {
                    _writetDone.Reset();
                    _tcpStateObject.WriteBytes = str;
                    try
                    {
                        _tcpStateObject.NetworkStream.BeginWrite(_tcpStateObject.WriteBytes, 0, _tcpStateObject.WriteBytes.Length, WriteBytesCallBack,
                            _tcpStateObject);
                        _writetDone.WaitOne();
                        return Finish;
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(DateTime.Now + ":Write-" + ex.Message);
                        if (_tcpStateObject.NetworkStream != null)
                            _tcpStateObject.NetworkStream.Close();
                        if (_tcpStateObject.Client != null)
                            _tcpStateObject.Client.Close();
                        _tcpStateObject.Client = new TcpClient();
                        Connect(_tcpStateObject);
                        if (_tcpStateObject.Client != null)
                        {
                            _tcpStateObject.NetworkStream = _tcpStateObject.Client.GetStream();
                            WriteBytes(str);
                        }
                        return DateTime.Now + ":Write-" + ex.Message;
                    }
                }
                else
                {
                    return "cannot write from networkstream in function WriteBytes";
                }
            else
            {
                return "cannot write networkstream in function WriteBytes";
            }
        }

        private void WriteBytesCallBack(IAsyncResult ar)
        {
            TcpClientStateObject t = (TcpClientStateObject)ar.AsyncState;
            try
            {
                t.NetworkStream.EndWrite(ar);
                _writetDone.Set();
            }
            catch (Exception ex)
            {

                Console.WriteLine(DateTime.Now + ":WriteCallBack-" + ex.Message);
                if (t.NetworkStream != null)
                    t.NetworkStream.Close();
                if (t.Client != null)
                    t.Client.Close();
                t.Client = new TcpClient();
                Connect(t);
                if (t.Client != null)
                {
                    t.NetworkStream = t.Client.GetStream();
                    WriteBytes(t.WriteBytes);
                }
            }
        }
        private int GetLittleEndianIntegerFromByteArray(byte[] data, int startIndex)
        {
            Console.WriteLine("+GetLittleEndianIntegerFromByteArray");
            Console.WriteLine("date=" + data);
            Console.WriteLine("date.length=" + data.Length);
            Console.WriteLine("startIndex=" + startIndex);
            Console.WriteLine("-GetLittleEndianIntegerFromByteArray");
            switch (data.Length)
            {
                case 1:
                    return (data[startIndex]);
                case 2:
                    return (data[startIndex])
                 | (data[startIndex + 1] << 8); 
                case 3:
                    return (data[startIndex])
                           | (data[startIndex + 1] << 8)
                           | (data[startIndex + 2] << 16);
                case 4:
                    return (data[startIndex])
                 | (data[startIndex + 1] << 8)
                 | (data[startIndex + 2] << 16)
                 | (data[startIndex + 3] << 24);
                default:
                    return 0;
            }
            
        }
        private byte[] data_append_dataLength(int lengthOfPrefixInByte,string data)
        {
            byte[] byteArray = System.Text.Encoding.GetEncoding(Encoding).GetBytes(data);
            byte[] dataLength = int_to_hex_little_endian(data.Length, lengthOfPrefixInByte);

            byte[] rv = new byte[dataLength.Length + byteArray.Length];
            Buffer.BlockCopy(dataLength, 0, rv, 0, dataLength.Length);
            Buffer.BlockCopy(byteArray, 0, rv, dataLength.Length, byteArray.Length);
            return rv;
        }
        private byte[] int_to_hex_little_endian(int length, int lengthOfPrefixInByte)
        {
            var reversedBytes = IPAddress.NetworkToHostOrder(length);
            string hex = reversedBytes.ToString("x");
            string trimmed = hex.Substring(0, lengthOfPrefixInByte * 2);
            //System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            //Byte[] bytes = encoding.GetBytes(trimmed);
            byte[] bytes = StringToByteArray(trimmed);
            //string str = System.Text.Encoding.ASCII.GetString(bytes);
            return bytes;
            //return HexAsciiConvert(trimmed);
        }
        private byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
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
    }
}

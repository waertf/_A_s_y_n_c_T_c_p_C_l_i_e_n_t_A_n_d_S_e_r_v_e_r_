using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DataGridViewAutoFilter;
using System.Configuration;

namespace WindowsFormsApplication1AVLSServer
{
    
    public partial class Form1 : Form
    {
        private ToolStripStatusLabel filterStatusLabel, showAllLabel;
        private Thread totalDisplayRowsNumberThread;
        private List<Record> records = new List<Record>();
        private DataTable dt;
        private BindingSource dataSource;
        private readonly string xmlFile ;

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
         TcpListener tcpListener7000, tcpListener6002;
         TcpClient client7000t, client6002t;
         ManualResetEvent stopEvent = new ManualResetEvent(false);
         bool port7000reset, port6002reset, port7000reconnect;
        public Form1()
        {
            InitializeComponent();

            //DateTime time = System.DateTime.ParseExact(System.DateTime.UtcNow.ToString("yyyyMMddHHmmss"), "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                             //DateTimeStyles.AssumeUniversal);
            //Record record = new Record();
            xmlFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\records.xml";
            dataGridView1.AllowUserToAddRows = false;//remove blank row when initial
            dt = ClassToDataTable.CreateTable(new Record());
            dt.PrimaryKey = new DataColumn[] { dt.Columns["ID"] };
            if (File.Exists(xmlFile))
                dt.ReadXml(xmlFile);
            /*
            ClassToDataTable.AddRow(ref dt,new Record()
            {
                DateTime = "11",
                Direction = "2",
                Event = "3",
                GPSValid = "4",
                ID = "1",
                Lat_Lon = "6",
                Message = "7",
                Speed = "8",
                Status = "9",
                Temperature = "10"
            });
            ClassToDataTable.AddRow(ref dt, new Record()
            {
                DateTime = "1",
                Direction = "2",
                Event = "3",
                GPSValid = "4",
                ID = "2",
                Lat_Lon = "6",
                Message = "7",
                Speed = "8",
                Status = "9",
                Temperature = "10"
            });
            */
            //dt = ConvertToDatatable(records);
            
            dataSource = new BindingSource(dt, null);
            dataGridView1.DataSource = dataSource;

            dataGridView1.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(dataGridView1_DataBindingComplete);
            this.Shown += new EventHandler(Form1_Shown);
            dataGridView1.AutoSize = true;
            dataGridView1.BindingContextChanged += new EventHandler(dataGridView1_BindingContextChanged);

            filterStatusLabel = new ToolStripStatusLabel();
            showAllLabel = new ToolStripStatusLabel();
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            filterStatusLabel,showAllLabel});
            filterStatusLabel.Text = "";
            filterStatusLabel.Visible = false;
            showAllLabel.Text = "Show &All";
            showAllLabel.Visible = false;
            showAllLabel.IsLink = true;
            showAllLabel.LinkBehavior = LinkBehavior.HoverUnderline;

            showAllLabel.Click += new EventHandler(showAllLabel_Click);


            #region totalDisplayRowsNumberThread
            totalDisplayRowsNumberThread = new System.Threading.Thread
              (delegate()
              {
                  Random rand = new Random();
                  while (true)
                  {
                      string test = dataGridView1.RowCount.ToString();
                      this.UIThread(() => this.Text = "Total number : "+test);
                      Thread.Sleep(300);
                  }
              });
            totalDisplayRowsNumberThread.Start();
            #endregion totalDisplayRowsNumberThread

            Thread avlsServerThread = new Thread(()=>AVLSServer());
            avlsServerThread.Start();
            //AddOrModifyTable( dt,record);

        }

        private void AVLSServer()
        {
            port7000reconnect = true;
            if (bool.Parse(ConfigurationManager.AppSettings["manualIP"]))
            {
                tcpListener7000 = new TcpListener(IPAddress.Parse(ConfigurationManager.AppSettings["ip"]), 7000);
                tcpListener6002 = new TcpListener(IPAddress.Parse(ConfigurationManager.AppSettings["ip"]), 6002);
            }
            else
            {
                tcpListener6002 = new TcpListener(IPAddress.Any, 6002);
                tcpListener7000 = new TcpListener(IPAddress.Any, 7000);
            }



            tcpListener6002.Start();
            tcpListener7000.Start();
            Console.WriteLine("waiting for connect...");
            while (true)
            {
                if (client7000t == null)
                {
                    client7000t = tcpListener7000.AcceptTcpClient();
                    port7000reset = true;
                }
                //Console.WriteLine("tcpListener7000.AcceptTcpClient");
                if (client6002t == null)
                {
                    client6002t = tcpListener6002.AcceptTcpClient();
                    port6002reset = true;
                }
                //Console.WriteLine("tcpListener6002.AcceptTcpClient");
                stopEvent.Reset();
                ThreadPool.QueueUserWorkItem(DealTheClient, new Client(client6002t, client7000t));
                stopEvent.WaitOne();
                Thread.Sleep(1);
            }
        }
         TcpClient client7000, client6002;
         string client7000Address, client7000Port, client6002Address;
         NetworkStream netStream7000, netStream6002;
         StreamReader reader = null;
          void DealTheClient(object state)
         {
             //Console.WriteLine("+DealTheClient");
             Client clientState = (Client)state;



             if (port7000reset)
             {
                 client7000 = clientState.getClient7000();
                 client7000Address = IPAddress.Parse(((
                     IPEndPoint)client7000.Client.RemoteEndPoint).Address.ToString()).ToString();
                 client7000Port = ((
                     IPEndPoint)client7000.Client.RemoteEndPoint).Port.ToString();
                 netStream7000 = client7000.GetStream();
                 port7000reset = false;
                 Console.WriteLine(client7000Address + ":7000 has connected");
             }
             if (port6002reset)
             {
                 client6002 = clientState.getClient6002();
                 client6002Address = IPAddress.Parse(((
                    IPEndPoint)client6002.Client.RemoteEndPoint).Address.ToString()).ToString();
                 netStream6002 = client6002.GetStream();
                 port6002reset = false;
                 Console.WriteLine(client6002Address + ":6002 has connected");
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
                         Console.WriteLine(client7000Address + ":7000 has disconnected");
                         netStream7000.Close();
                         client7000.Close();
                         client7000t = null;
                         reader = null;
                         stopEvent.Set();
                         break;
                     }

                     if (message == null)
                     {
                         Console.WriteLine(client7000Address + ":7000 has disconnected");
                         netStream7000.Close();
                         client7000.Close();
                         client7000t = null;
                         stopEvent.Set();
                         break;
                     }

                     message7000Counter++;
                     //Console.WriteLine(client7000Address+String.Format(" >> [{0}] Message received: {1}", message7000Counter, message));
                     string[] stringSeparators = new string[] { ",", "%%" };
                     string[] receiveStrings = message.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                     int counter = 0;
                     UNITReportPacket recvReportPacket = new UNITReportPacket();
                     Record record = new Record();
                     foreach (var receiveString in receiveStrings)
                     {

                         //Console.WriteLine(counter +":"+receiveString);
                         
                         switch (counter)
                         {
                             case 0:
                                 recvReportPacket.ID = receiveString;
                                 record.ID = receiveString;
                                 break;
                             case 1:
                                 recvReportPacket.GPSValid = receiveString;
                                 record.GPSValid = receiveString;
                                 break;
                             case 2:
                                 recvReportPacket.DateTime = "20" + receiveString;
                                 DateTime time = System.DateTime.ParseExact(recvReportPacket.DateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                             DateTimeStyles.AssumeUniversal);
                                 record.DateTime = time.ToString("s");
                                 break;
                             case 3:
                                 recvReportPacket.Loc = receiveString;
                                 char[] delimiterChars = { 'N', 'E', 'S', 'W' };
                                 string[] tmp1 = receiveString.Split(delimiterChars);
                                 string lat = ConvertNmea0183ToUtm(float.Parse(tmp1[1])).ToString();
                                 string lon = ConvertNmea0183ToUtm(float.Parse(tmp1[2])).ToString();
                                 record.Lat_Lon = lat + "-" + lon;
                                 break;
                             case 4:
                                 recvReportPacket.Speed = receiveString;
                                 record.Speed = receiveString;
                                 break;
                             case 5:
                                 recvReportPacket.Dir = receiveString;
                                 record.Direction = receiveString;
                                 break;
                             case 6:
                                 recvReportPacket.Temp = receiveString;
                                 record.Temperature = receiveString;
                                 break;
                             case 7:
                                 recvReportPacket.Status = receiveString;
                                 record.Status = receiveString;
                                 break;
                             case 8:
                                 recvReportPacket.Event = receiveString;
                                 record.Event = receiveString;
                                 break;
                             case 9:
                                 recvReportPacket.Message = receiveString;
                                 record.Message = receiveString;
                                 break;
                         }
                         counter++;
                     }
                     Thread addOrModifyTableThread = new Thread(()=>AddOrModifyTable(dt, record));
                     addOrModifyTableThread.Start();
                     //byte[] packageSendTo6002;
                     using (var m = new MemoryStream())
                     {
                         string Head = "$CMD_H#";
                         string Tail = "$CMD_T#";

                         byte[] headBytes = Encoding.ASCII.GetBytes(Head);
                         byte[] HeadLength = ByteCountBigEndian(headBytes.Length);

                         byte[] Cmd_Type = new byte[] { 2 };
                         byte[] id = ByteCountBigEndian(idCounter++);
                         byte[] priority = new byte[] { 5 };
                         byte[] Attach_Type = new byte[] { 1 };


                         #region avlsPackageFromPort7000_attach

                         byte[] uid = Encoding.ASCII.GetBytes(recvReportPacket.ID);
                         byte[] uidLength = ByteCountBigEndian(uid.Length);

                         byte[] statusBytes = new byte[] { 0x0A };

                         DateTime time = System.DateTime.ParseExact(recvReportPacket.DateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                             DateTimeStyles.AssumeUniversal |
                             DateTimeStyles.AdjustToUniversal);
                         long timeLong = Decimal.ToInt64(Decimal.Divide(time.Ticks - 621355968000000000, 10000));
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
                         byte[] origin_lo = new byte[] { 0x00, 0x00, 0x00, 0x00 };
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
                         byte[] option0_Length = ByteCountBigEndian(option0.Length);
                         byte[] option1 = Encoding.ASCII.GetBytes(recvReportPacket.Status);
                         byte[] option1_Length = ByteCountBigEndian(option1.Length);
                         byte[] option2 = Encoding.ASCII.GetBytes(recvReportPacket.Event);
                         byte[] option2_Length = ByteCountBigEndian(option2.Length);
                         byte[] option3 = Encoding.ASCII.GetBytes(recvReportPacket.Message);
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

                         byte[] tailBytes = Encoding.ASCII.GetBytes(Tail);
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
                         m.Write(TailLength, 0, TailLength.Length);
                         m.Write(tailBytes, 0, tailBytes.Length);
                         #endregion write to memorystream

                         byte[] packageSendTo6002 = m.ToArray();
                         try
                         {
                             netStream6002.Write(packageSendTo6002, 0, packageSendTo6002.Length);
                         }
                         catch (Exception ex)
                         {
                             Console.WriteLine(client6002Address + ":6002 has disconnected");
                             netStream6002.Close();
                             client6002.Close();
                             client6002t = null;
                             stopEvent.Set();
                             break;
                         }
                         if (netStream6002 != null)
                             netStream6002.Flush();
                         //Thread writeThread = new Thread(() => netStream6002.Write(packageSendTo6002, 0, packageSendTo6002.Length));
                         //writeThread.Start();
                     }
                     //Console.WriteLine(DateTime.Now.ToString("s")+"send to port 6002");
                     //Console.WriteLine("-------------------------------------------");
                     //Console.WriteLine(Encoding.ASCII.GetString(packageSendTo6002));
                     //Console.WriteLine("-------------------------------------------");
                     //Console.WriteLine(BitConverter.ToString(packageSendTo6002));
                     //Console.WriteLine("-------------------------------------------");


                     Thread.Sleep(300);
                 }
             }
             //Console.WriteLine("-DealTheClient");
         }
          byte[] ByteCountBigEndian(int a)
         {
             byte[] b = BitConverter.GetBytes(a);
             return b.Reverse().ToArray();
         }
          float ConvertNmea0183ToUtm(float f)
         {
             int i = (int)(f / 100);
             return (f / 100 - i) * 100 / 60 + i;
         }
          byte[] SendLocBackToWeb(string recev)
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

        private void AddOrModifyTable( DataTable dataTable, Record record)
        {
            DataRow[] founDataRows = dataTable.Select("ID='" + record.ID + "'");
            if (founDataRows.Length == 0)
            {
                //add to table
                Thread addThread = new Thread(() => ClassToDataTable.AddRow(ref dt, record));
                addThread.Start();
                this.UIThread(() => dataGridView1.Refresh());
            }
            else
            {
                //modify table
                Thread modifyThread = new Thread(()=>ModifyTable( founDataRows, record));
                modifyThread.Start();
            }
            //this.UIThread(()=>dataSource.ResetBindings(false));
            //Thread.Sleep(300);
        }

        private void ModifyTable( DataRow[] founDataRows,  Record record)
        {
            founDataRows[0]["Lat_Lon"] = record.Lat_Lon;
            founDataRows[0]["Message"] = record.Message;
            founDataRows[0]["Speed"] = record.Speed;
            founDataRows[0]["Status"] = record.Status;
            founDataRows[0]["Temperature"] = record.Temperature;
            founDataRows[0]["DateTime"] = record.DateTime;
            founDataRows[0]["Direction"] = record.Direction;
            founDataRows[0]["Event"] = record.Event;
            founDataRows[0]["GPSValid"] = record.GPSValid;
            this.UIThread(() => dataGridView1.Refresh());
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            dt.WriteXml(xmlFile);
            totalDisplayRowsNumberThread.Abort();
            base.OnFormClosing(e);
        }
        private void showAllLabel_Click(object sender, EventArgs e)
        {
            DataGridViewAutoFilterTextBoxColumn.RemoveFilter(dataGridView1);
        }

        void dataGridView1_BindingContextChanged(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null) return;

            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.HeaderCell = new
                    DataGridViewAutoFilterColumnHeaderCell(col.HeaderCell);
            }
            dataGridView1.AutoResizeColumns();
        }

        void Form1_Shown(object sender, EventArgs e)
        {
            Size size = new Size(dataGridView1.Width, dataGridView1.Height);
            this.Size = size;
        }

        

        void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Put each of the columns into programmatic sort mode.
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            String filterStatus = DataGridViewAutoFilterColumnHeaderCell
          .GetFilterStatus(dataGridView1);
            if (String.IsNullOrEmpty(filterStatus))
            {
                showAllLabel.Visible = false;
                filterStatusLabel.Visible = false;
            }
            else
            {
                showAllLabel.Visible = true;
                filterStatusLabel.Visible = true;
                filterStatusLabel.Text = filterStatus;

            }
        }
        private static DataTable ConvertToDatatable<T>(List<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    table.Columns.Add(prop.Name, prop.PropertyType.GetGenericArguments()[0]);
                else
                    table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            dt.Rows[0][0] = "999";
            //dataSource.ResetBindings(false);

            /*
            records.RemoveAt(0);
            dt = ConvertToDatatable(records);

            dataSource = new BindingSource(dt, null);
            dataGridView1.DataSource = dataSource;*/
        }

        

        
    }
    public static class ControlExtensions
    {
        /// <summary>
        /// Executes the Action asynchronously on the UI thread, does not block execution on the calling thread.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="code"></param>
        public static void UIThread(this Control @this, Action code)
        {
            if (@this.InvokeRequired)
            {
                @this.BeginInvoke(code);
            }
            else
            {
                code.Invoke();
            }
        }
    }
    public static class ClassToDataTable
    {
        public static DataTable CreateTable(object objClass)
        {
            Type objType = objClass.GetType();
            DataTable result = new DataTable(objType.ToString().Split('.')[1]);
            List<PropertyInfo> propertyList = new List<PropertyInfo>(objType.GetProperties());

            foreach (PropertyInfo prop in propertyList)
            {
                object propValue = prop.GetValue(objClass, null);
                result.Columns.Add(prop.Name, prop.PropertyType);
            }
            return result;
        }

        public static void AddRow(ref DataTable table, object data)
        {
            Type objType = data.GetType();
            string className = objType.ToString().Split('.')[1];

            if (!table.TableName.Equals(className))
            {
                throw new Exception("DataTableConverter.AddRow: " +
                                    "TableName not equal to className.");
            }

            DataRow dRow = table.NewRow();
            List<PropertyInfo> propertyList = new List<PropertyInfo>(objType.GetProperties());

            foreach (PropertyInfo prop in propertyList)
            {
                if (table.Columns[prop.Name] == null)
                {
                    throw new Exception("DataTableConverter.AddRow: " +
                                        "Column name does not exist: " + prop.Name);
                }
                object propValue = prop.GetValue(data, null);
                dRow[prop.Name] = propValue;
            }
            table.Rows.Add(dRow);
        }
    }
    class Record
    {
        public string ID { get; set; }
        public string GPSValid { get; set; }
        public string DateTime { get; set; }
        public string Lat_Lon { get; set; }
        public string Speed { get; set; }
        public string Direction { get; set; }
        public string Temperature { get; set; }
        public string Status { get; set; }
        public string Event { get; set; }
        public string Message { get; set; }
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Configuration;
using System.Threading;
using Gurock.SmartInspect;

namespace RadarInfoDemo
{
    class Program
    {
        static string[] uidStrings;
        static StringBuilder sb = null;
        static object sbLock = new object();
        static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        static Queue<StringBuilder> sbQueue = new Queue<StringBuilder>(); 
        static void Main(string[] args)
        {
            SiAuto.Si.Enabled = true;
            SiAuto.Si.Connections = @"file(filename=""" +
                                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                    "\\log.sil\",rotate=weekly,append=true,maxparts=5,maxsize=500MB)";
            /*
            #region sql test
            SqlClient sql_client = new SqlClient(
                ConfigurationManager.AppSettings["SQL_SERVER_IP"], 
                ConfigurationManager.AppSettings["SQL_SERVER_PORT"], 
                ConfigurationManager.AppSettings["SQL_SERVER_USER_ID"], 
                ConfigurationManager.AppSettings["SQL_SERVER_PASSWORD"], 
                ConfigurationManager.AppSettings["SQL_SERVER_DATABASE"], 
                ConfigurationManager.AppSettings["Pooling"], 
                ConfigurationManager.AppSettings["MinPoolSize"], 
                ConfigurationManager.AppSettings["MaxPoolSize"], 
                ConfigurationManager.AppSettings["ConnectionLifetime"]);
            sql_client.connect();
            for (int i = 0; i < 1000; i++)
            {
                string cmd = @"INSERT INTO ""UidAndLoc""(
            uid, lat, lon)
    VALUES (1, 2,"+i+");";
                sql_client.modify(cmd);
            }
            sql_client.disconnect();
            
            sql_client.Dispose();
            sql_client = null;
            #endregion
            */
            {
                var sbTimer = new System.Timers.Timer(int.Parse(ConfigurationManager.AppSettings["sendSleepTime"]));
                sbTimer.Elapsed += (sender, e) =>
                {
                    if (sbQueue.Count > 0)
                    {
                        //Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["sendSleepTime"]));
                        SqlClient sql_client = new SqlClient(
                         ConfigurationManager.AppSettings["SQL_SERVER_IP"],
                         ConfigurationManager.AppSettings["SQL_SERVER_PORT"],
                         ConfigurationManager.AppSettings["SQL_SERVER_USER_ID"],
                         ConfigurationManager.AppSettings["SQL_SERVER_PASSWORD"],
                         ConfigurationManager.AppSettings["SQL_SERVER_DATABASE"],
                         ConfigurationManager.AppSettings["Pooling"],
                         ConfigurationManager.AppSettings["MinPoolSize"],
                         ConfigurationManager.AppSettings["MaxPoolSize"],
                         ConfigurationManager.AppSettings["ConnectionLifetime"]);
                        sql_client.connect();
                        sql_client.modify(sbQueue.Dequeue().ToString());
                        
                        sql_client.disconnect();

                        sql_client.Dispose();
                        sql_client = null;
                    }
                };
                sbTimer.Enabled = true;
            }
            uidStrings = GetAllFilesCSV(Environment.CurrentDirectory).ToArray();
            sb = new StringBuilder();
            for (int j = 0; j < 260; j++)//get 260 loc
            {
                Thread sendUidThread = null;
                for (int i = 0; i < uidStrings.Length; i++)
                {
                    int i1 = i;
                    /*
                    var workToDo = new WaitCallback(o =>
                      {
                        // Your stuff here
                          SendByUid(uidStrings[i1], networkStream);
                      });
                    ThreadPool.QueueUserWorkItem(workToDo);
                    */
                    sendUidThread = new Thread(delegate()
                    {
                        SendByUid(uidStrings[i1]);
                    });
                    sendUidThread.Start();
                    //autoResetEvent.Set();
                }
                sendUidThread.Join();
                sbQueue.Enqueue(sb);
                sb = null;
                sb = new StringBuilder();
                }
            Console.ReadLine();
        }

        private static void SendByUid(string uid)
        {
           
            string path = Environment.CurrentDirectory + "\\" + uid + ".csv";
            using (StreamReader sr = new StreamReader(path))
            {
                if (sr.Peek() >= 0)
                {
                    string[] oneLineStrings = sr.ReadLine().Split(new char[] { ',' });
                    string lon = oneLineStrings[1];
                    string lat = oneLineStrings[2];
                    string cmd = @"INSERT INTO ""UidAndLoc""(
            uid, lat, lon)
    VALUES ("+uid+","+lat+","+lon+");";
                    lock (sbLock)
                    {
                        while (sb!=null)
                        {
                            sb.Append(cmd);
                            break;
                        }
                        
                    }
                   // Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["sendSleepTime"]));
                }
            }
            //autoResetEvent.WaitOne();
        }
        static List<String> GetAllFilesCSV(String directory)
        {
            return Directory.GetFiles(directory, "*.csv", SearchOption.AllDirectories).Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }
    }
}

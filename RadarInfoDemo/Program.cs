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
            uidStrings = GetAllFilesCSV(Environment.CurrentDirectory).ToArray();
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
                Thread sendUidThread = new Thread(delegate()
                {
                    SendByUid(uidStrings[i1]);
                });
                sendUidThread.Start();
            }
        }

        private static void SendByUid(string uid)
        {
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
            string path = Environment.CurrentDirectory + "\\" + uid + ".csv";
            using (StreamReader sr = new StreamReader(path))
            {
                while (sr.Peek() >= 0)
                {
                    string[] oneLineStrings = sr.ReadLine().Split(new char[] { ',' });
                    string lon = oneLineStrings[1];
                    string lat = oneLineStrings[2];
                    string cmd = @"INSERT INTO ""UidAndLoc""(
            uid, lat, lon)
    VALUES ("+uid+","+lat+","+lon+")";
                    sql_client.modify(cmd);
                    Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["sendSleepTime"]));
                }
            }
            sql_client.disconnect();

            sql_client.Dispose();
            sql_client = null;
        }
        static List<String> GetAllFilesCSV(String directory)
        {
            return Directory.GetFiles(directory, "*.csv", SearchOption.AllDirectories).Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }
    }
}

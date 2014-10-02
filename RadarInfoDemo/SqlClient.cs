﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Devart.Data.PostgreSql;
using System.Data;
using Gurock.SmartInspect;

namespace RadarInfoDemo
{
    class SqlClient
    {

        PgSqlConnection pgSqlConnection;
        public bool IsConnected { get; set; }
 
        public SqlClient(string ip, string port, string user_id, string password, string database, string Pooling, string MinPoolSize, string MaxPoolSize, string ConnectionLifetime)
        {
            PgSqlConnectionStringBuilder pgCSB = new PgSqlConnectionStringBuilder();
            pgCSB.Host = ip;
            pgCSB.Port = int.Parse(port);
            pgCSB.UserId = user_id;
            pgCSB.Password = password;
            pgCSB.Database = database;

            pgCSB.Pooling = bool.Parse(Pooling);
            pgCSB.MinPoolSize = int.Parse(MinPoolSize);
            pgCSB.MaxPoolSize = int.Parse(MaxPoolSize);
            pgCSB.ConnectionLifetime = int.Parse(ConnectionLifetime); ;

            pgCSB.Unicode = true;
            pgSqlConnection = new PgSqlConnection(pgCSB.ConnectionString);
        }
        public bool connect()
        {
            try
            {
                if (pgSqlConnection != null)
                {
                    pgSqlConnection.Open();
                    IsConnected = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (PgSqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Connect exception occurs: {0}", ex.Error);
                SiAuto.Main.LogError("Connect exception occurs: " + ex.Error);
                Console.ResetColor();
                return false;
            }
        }
        public bool disconnect()
        {
            try
            {
                if (pgSqlConnection != null)
                {
                    pgSqlConnection.Close();
                    IsConnected = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (PgSqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Disconnect exception occurs: {0}", ex.Error);
                SiAuto.Main.LogError("Disconnect exception occurs: " + ex.Error);
                Console.ResetColor();
                return false;
            }
        }
        //For UPDATE, INSERT, and DELETE statements
        public void modify(string cmd)
        {
            PgSqlCommand command = null;
            try
            {
                if (pgSqlConnection != null && IsConnected)
                {
                    //insert
                    //command = pgSqlConnection.CreateCommand();
                    //command.CommandText = cmd;
                    //cmd.CommandText = "INSERT INTO public.test (id) VALUES (1)";
                    pgSqlConnection.BeginTransaction();
                    //async
                    //IAsyncResult cres = command.BeginExecuteNonQuery(null, null);
                    //Console.Write("In progress...");
                    //while (!cres.IsCompleted)
                    {
                        //Console.Write(".");
                        //Perform here any operation you need
                    }
                    /*
                    if (cres.IsCompleted)
                        Console.WriteLine("Completed.");
                    else
                        Console.WriteLine("Have to wait for operation to complete...");
                    */
                    //int RowsAffected = command.EndExecuteNonQuery(cres);
                    //Console.WriteLine("Done. Rows affected: " + RowsAffected.ToString());
                    /*
                     //sync
                     int aff = cmd.ExecuteNonQuery();
                     Console.WriteLine(aff + " rows were affected.");
                     * 
                     */
                    //pgSqlConnection.Commit();
                    PgSqlScript pgSqlScript = new PgSqlScript(cmd,pgSqlConnection);
                    pgSqlScript.Execute();
                    pgSqlConnection.Commit();
                    ThreadPool.QueueUserWorkItem(callback =>
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(
                            "S++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                        Console.WriteLine("sql Write:\r\n" + cmd);
                        Console.WriteLine(
                            "E++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                        Console.ResetColor();
                        SiAuto.Main.LogMessage("sql Write:\r\n" + cmd);
                    });


                }

            }
            catch (PgSqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Modify exception occurs: {0}" + Environment.NewLine + "{1}", ex.Error, cmd);
                SiAuto.Main.LogError("Modify exception occurs: " + Environment.NewLine + ex.Error + Environment.NewLine + cmd);
                Console.ResetColor();
                pgSqlConnection.Rollback();


            }


        }
        void EndModify(IAsyncResult ar)
        {
            PgSqlCommand command = null;
            try
            {
                command = ar.AsyncState as PgSqlCommand;
                int row = command.EndExecuteNonQuery(ar);
                //Console.WriteLine(row + " rows were affected.");
            }
            catch (PgSqlException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Modify exception occurs: {0}" + Environment.NewLine + "{1}", ex.Error, command.CommandText);
                SiAuto.Main.LogError("Modify exception occurs: " + Environment.NewLine + ex.Error + Environment.NewLine + command.CommandText);
                Console.ResetColor();
                pgSqlConnection.Rollback();
            }
            finally
            {
                if (command != null)
                    command.Dispose();
                command = null;
            }
        }
        //For SELECT statements
        public DataTable get_DataTable(string cmd)
        {
            Stopwatch stopWatch = new Stopwatch();
            PgSqlCommand command = null;
            stopWatch.Start();
            using (DataTable datatable = new DataTable())
            {
                try
                {
                    if (pgSqlConnection != null && IsConnected)
                    {
                        //DataTable datatable = new DataTable();
                        command = pgSqlConnection.CreateCommand();
                        command.CommandText = cmd;
                        //Console.WriteLine("Starting asynchronous retrieval of data...");
                        IAsyncResult cres = command.BeginExecuteReader();
                        //Console.Write("In progress...");
                        //while (!cres.IsCompleted)
                        {
                            //Console.Write(".");
                            //Perform here any operation you need
                        }

                        //if (cres.IsCompleted)
                        //Console.WriteLine("Completed.");
                        //else
                        //Console.WriteLine("Have to wait for operation to complete...");
                        PgSqlDataReader myReader = command.EndExecuteReader(cres);
                        try
                        {
                            // printing the column names
                            for (int i = 0; i < myReader.FieldCount; i++)
                            {
                                //Console.Write(myReader.GetName(i).ToString() + "\t");
                                datatable.Columns.Add(myReader.GetName(i).ToString(), typeof(string));
                            }
                            //Console.Write(Environment.NewLine);
                            while (myReader.Read())
                            {
                                DataRow dr = datatable.NewRow();

                                for (int i = 0; i < myReader.FieldCount; i++)
                                {
                                    //Console.Write(myReader.GetString(i) + "\t");
                                    dr[i] = myReader.GetString(i);
                                }
                                datatable.Rows.Add(dr);
                                //Console.Write(Environment.NewLine);
                                //Console.WriteLine(myReader.GetInt32(0) + "\t" + myReader.GetString(1) + "\t");
                            }
                        }
                        finally
                        {
                            myReader.Close();
                            stopWatch.Stop();
                            // Get the elapsed time as a TimeSpan value.
                            TimeSpan ts = stopWatch.Elapsed;

                            // Format and display the TimeSpan value.
                            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                ts.Hours, ts.Minutes, ts.Seconds,
                                ts.Milliseconds / 10);
                            SiAuto.Main.AddCheckpoint(Level.Debug, "sql query take time:" + elapsedTime, cmd);
                        }
                        /*
                        foreach (DataRow row in datatable.Rows) // Loop over the rows.
                        {
                            Console.WriteLine("--- Row ---"); // Print separator.
                            foreach (var item in row.ItemArray) // Loop over the items.
                            {
                                Console.Write("Item: "); // Print label.
                                Console.WriteLine(item); // Invokes ToString abstract method.
                            }
                        }
                        */
                        if (command != null)
                            command.Dispose();
                        command = null;
                        return datatable.Copy();
                    }
                    else
                    {

                        return null;
                    }

                }
                catch (PgSqlException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("GetDataTable exception occurs: {0}" + Environment.NewLine + "{1}", ex.Error, cmd);
                    SiAuto.Main.LogError("GetDataTable exception occurs: " + Environment.NewLine + ex.Error + Environment.NewLine + cmd);
                    Console.ResetColor();
                    if (command != null)
                        command.Dispose();
                    command = null;
                    return null;
                }
            }

        }
        public void Dispose()
        {
            PgSqlConnection.ClearAllPools(true);
            pgSqlConnection.Dispose();
            pgSqlConnection = null;
        }
        //~SqlClient()
        //{
        //PgSqlConnection.ClearPool(pgSqlConnection);
        //pgSqlConnection.Dispose();
        //pgSqlConnection = null;
        //}   
    }
}
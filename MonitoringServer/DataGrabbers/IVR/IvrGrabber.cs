using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
using System.Data.SqlClient;
using System.Configuration;
using MonitoringServer.Data;


namespace MonitoringServer.DataGrabbers.IVR
{
    public class IvrGrabber
    {

        private static bool isStarted = false;
        public static Hashtable ivrLineStatusTable = new Hashtable();
        private Hashtable configIvrTable;
        private Hashtable configDataTable;
        private SqlConnection sqlConnection;
        private SqlCommand sqlCommand; 
        public static bool collectionStart = false; 

        public IvrGrabber(Hashtable configIvrTable, Hashtable configDataTable)
        {
            this.configIvrTable = configIvrTable;
            this.configDataTable = configDataTable;
            this.sqlConnection = (SqlConnection)this.configIvrTable["TechnoCallSqlConnection"];
            this.sqlCommand = new SqlCommand("SELECT @@Version", this.sqlConnection);
        }

        public bool start() 
        {       
            if (sqlConnection.State.Equals("Open"))
            {
                sqlConnection.Close();
            }

            try
            {
                sqlConnection.Open();
                var reader = sqlCommand.ExecuteScalar();
                MonitoringServerService.log.Info("Подключение к DB TechnoCall установленно.");
                sqlConnection.Close();
            }
            catch (Exception ex) { MonitoringServerService.log.Error(ex); return false; }
            finally { sqlConnection.Close(); }

            try
            {
                this.sqlCommand.CommandText = "SELECT [FeederName] FROM [TechnoCall].[dbo].[TC_Feeder]";
                this.sqlConnection.Open();
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        IvrData ivrData = new IvrData(reader["FeederName"].ToString());
                        ivrLineStatusTable.Add(reader["FeederName"], ivrData);
                    }
                }
                this.sqlConnection.Close();
                MonitoringServerService.log.Info("В DB TechnoCall зарегистрировано " + ivrLineStatusTable.Count + " IVR'ов.");
                Monitoring monitoringIvr = new Monitoring(ivrLineStatusTable, configIvrTable, configDataTable);
                Thread thread = new Thread(monitoringIvr.startMonitor);
                thread.Start();

                return true;

            }
            catch (Exception ex) { MonitoringServerService.log.Error(ex); return false; }
            finally { this.sqlConnection.Close(); }
        }
    }
}


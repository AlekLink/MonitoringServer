using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using System.Data;
using MonitoringServer.Data;

namespace MonitoringServer.DataGrabbers.IVR
{
    public class Monitoring 
    {
        private SqlConnection conn;
        private SqlCommand comm;
        private string query;
        private string lineStatusQuery;
        private string transCountQuery;
        private string wqCountQuery;
        private static Hashtable ivrTable;
        private int timeOut;
        private int saveDataTimeout;
        private int counterDataTimeout;
        private LocalDataSet dataSet;
        private DataTable ivrChannels;
        private DataTable ivrsData;
        private DataTable ivrTransData;
        private DataTable ivrWQData;
        private bool firstStart;
        private ccMonitoringEntities ccMonitoringContext;
            

        public Monitoring(Hashtable ivrLineStatusTable, Hashtable ivrConfigTable, Hashtable dataConfigTable)
        {
            this.conn = (SqlConnection)ivrConfigTable["TechnoCallSqlConnection"];
            this.lineStatusQuery = (String)ivrConfigTable["LinesQuery"];
            this.transCountQuery = (String)ivrConfigTable["TransQuery"];
            this.wqCountQuery = (String)ivrConfigTable["WQQuery"];
            this.comm = new SqlCommand();
            this.dataSet = (LocalDataSet)dataConfigTable["LocalDataSet"];
            this.ccMonitoringContext = (ccMonitoringEntities)dataConfigTable["ccMonitoringModel"];
            this.timeOut = (int)ivrConfigTable["Timeout"];
            this.saveDataTimeout = (int)ivrConfigTable["SaveDataTimeout"];
            this.firstStart = (Boolean)ivrConfigTable["FirstStart"];
            this.ivrChannels = this.dataSet.ivrChannels;
            this.ivrsData = this.dataSet.ivrsData;
            this.ivrTransData = this.dataSet.ivrTransData;
            this.ivrWQData = this.dataSet.ivrWQData;
            
            ivrTable = ivrLineStatusTable;
        }

        public void startMonitor()
        {
            while(true)
            {
                mainQuery();
                Thread.Sleep(new TimeSpan(0, 0, this.timeOut));
            }
        }

        public void mainQuery()
        {

            if (this.conn.State.ToString().Equals("Open"))
                this.conn.Close();

            this.comm.Connection = this.conn;
            try
            {
                Console.WriteLine(this.conn.State.ToString());
                this.conn.Open();
                this.query = this.lineStatusQuery;
                this.comm.CommandText = this.lineStatusQuery;
                using (SqlDataReader reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DataRow ivrRow = this.ivrChannels.NewRow();
                        ivrRow["Channel"] = reader["Channel"];
                        ivrRow["IVRID"] = reader["IVRID"];
                        ivrRow["OpTime"] = reader["OpTime"];
                        ivrRow["Operation"] = reader["Operation"];
                        ivrRow["CallType"] = reader["CallType"];
                        ivrRow["IvrCallID"] = reader["IvrCallID"];
                        ivrRow["CallStart"] = reader["CallStart"];
                        ivrRow["Dir"] = reader["Dir"];
                        this.ivrChannels.Rows.Add(ivrRow);
                        ivrRow = null;
                    }
                }
                this.comm.CommandText = this.transCountQuery;
                using (SqlDataReader reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DataRow ivrRow = this.ivrTransData.NewRow();
                        ivrRow["CT"] = reader["CT"];
                        ivrRow["TRANSID"] = reader["TRANSID"];
                        ivrRow["SOURCESYS"] = reader["SOURCESYS"];
                        ivrRow["TARGETSYS"] = reader["TARGETSYS"];
                        ivrRow["FUNCTIONID"] = reader["FUNCTIONID"];
                        ivrRow["ENTRYTIME"] = reader["ENTRYTIME"];
                        this.ivrTransData.Rows.Add(ivrRow);
                        ivrRow = null;
                    }
                }
                this.comm.CommandText = this.wqCountQuery;
                using (SqlDataReader reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DataRow ivrRow = this.ivrWQData.NewRow();
                        ivrRow["CallType"] = reader["CallType"];
                        ivrRow["IVRID"] = reader["IVRID"];
                        ivrRow["Channel"] = reader["Channel"];
                        ivrRow["QueueTime"] = reader["QueueTime"];
                        ivrRow["PhoneNumber"] = reader["PhoneNumber"];
                        ivrRow["WaitEnd"] = reader["WaitEnd"];
                        this.ivrWQData.Rows.Add(ivrRow);
                        ivrRow = null;
                    }
                }
                conn.Close();
               
                getDataForEachIvr(ivrChannels, ivrTransData, ivrWQData);


                this.ivrChannels.Clear();
                this.ivrTransData.Clear();
                this.ivrWQData.Clear();

            }
            catch (Exception ex)
            {
                MonitoringServerService.log.Error("Возникла ошибка при попытке получить данные из DB TechnoCall.", ex);
            }
            finally
            {
                conn.Close();
            }
        }

        private void getDataForEachIvr(DataTable lineTable, DataTable trTable, DataTable wqTable)
        {
            List<string> listIvrName = new List<string>();
            
            try
            {
                Hashtable tempTable = new Hashtable();
                var answerLine = from ivr in lineTable.AsEnumerable()
                             select ivr;
                var answerTrans = from ivr in trTable.AsEnumerable()
                                  select ivr;
                var answerWQ = from ivr in wqTable.AsEnumerable()
                               select ivr;
                foreach (string eachIvrName in ivrTable.Keys)
                {
                    int lineIdle = 0
                        ,lineIvr = 0
                        ,lineWaiting = 0
                        ,lineAgent = 0
                        ,trCount = 0
                        ,wqCount = 0;

                    foreach (var p in answerLine)
                    {
                        if (eachIvrName.Equals(p.Field<string>("IVRID")))
                        {
                            if (p.Field<string>("Operation").Equals("IDLE"))
                                lineIdle++;
                            else if (p.Field<string>("Operation").Equals("IVR"))
                                lineIvr++;
                            else if (p.Field<string>("Operation").Equals("WAITING"))
                                lineWaiting++;
                            else if (p.Field<string>("Operation").Equals("AGENT"))
                                lineAgent++;
                            else
                                MonitoringServerService.log.Info("Внимание, обнаружен новый статус линии IVR. Новый статус: " + p.Field<string>("Operation"));
                        }                     
                    }

                    foreach (var p in answerTrans)
                    {
                        if (p.Field<string>("SOURCESYS").Equals(eachIvrName) || p.Field<string>("TARGETSYS").Equals(eachIvrName))
                        {
                            trCount++;
                        }
                    }

                    foreach (var p in answerWQ)
                    {
                        if (p.Field<string>("IVRID").Equals(eachIvrName))
                        {
                            wqCount++;
                        }
                    }

                    var ivrDataObject = (IvrData)ivrTable[eachIvrName];
                    ivrDataObject.ivrName = eachIvrName;
                    ivrDataObject.lineIdle = lineIdle;
                    ivrDataObject.lineIvr = lineIvr;
                    ivrDataObject.lineWaiting = lineWaiting;
                    ivrDataObject.lineAgent = lineAgent;
                    ivrDataObject.transactionCount = trCount;
                    ivrDataObject.wqueueCount = wqCount;
                    
                }
                

                if (saveDataTimeout > 19)
                {
                    foreach (IvrData o in ivrTable.Values)
                    {
                        ccMonitoringContext.sp_InsertLineHistoryStatus(o.ivrName, o.lineIdle, o.lineIvr, o.lineWaiting, o.lineAgent, o.transactionCount, o.wqueueCount);
                        ccMonitoringContext.SaveChanges();
                        
                    }
                    saveDataTimeout = 0;
                    //MonitoringServerService.log.Info("Данные в таблицу добавлены.");
                }
                saveDataTimeout++;
            }
            catch (Exception ex)
            {
                MonitoringServerService.log.Error(ex);
            }
        }
    }
}

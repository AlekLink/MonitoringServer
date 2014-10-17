using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using MonitoringServer.Data;
using System.Configuration;

namespace MonitoringServer.Initialize.Modules
{
    public class Ivr
    {
        private int ivrTimeout;
        private int saveDataTimeout;
        private string getIvrLineStatus;
        private string getIvrTransData;
        private string getIvrWQData;
        private string connStringTechnoCall;
        private bool firstStart;
        //private LocalDataSet linesDS;
        //private ccMonitoringEntities ccMonitoringModel;
        private SqlConnection conn;
        private Hashtable configTable = new Hashtable();
               

        public Ivr()
        {
            try
            {
                this.ivrTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["ivrTimeOut"]);
                this.saveDataTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["saveDataTimeout"]);
                this.getIvrLineStatus = ConfigurationManager.AppSettings["getIvrLineStatus"];
                this.getIvrTransData = ConfigurationManager.AppSettings["getIvrTransData"];
                this.getIvrWQData = ConfigurationManager.AppSettings["getIvrWQData"];
                this.connStringTechnoCall = ConfigurationManager.ConnectionStrings["TechnoCall"].ToString();
                this.firstStart = Convert.ToBoolean(ConfigurationManager.AppSettings["firstStart"]);
                //this.linesDS = new LocalDataSet();
                //this.ccMonitoringModel = new ccMonitoringEntities();

                this.conn = new SqlConnection(this.connStringTechnoCall);
            }
            catch (Exception ex)
            {
                MonitoringServerService.log.Error("Возникла ошибка при попытке чтения конфигурационного файла приложения. Модуль IVR.", ex);
                return;
            }
        }

        public object getConfig()
        {
            this.configTable.Add("Timeout", this.ivrTimeout);
            this.configTable.Add("SaveDataTimeout", this.saveDataTimeout);
            this.configTable.Add("LinesQuery", this.getIvrLineStatus);
            this.configTable.Add("TransQuery", this.getIvrTransData);
            this.configTable.Add("WQQuery", this.getIvrWQData);
            this.configTable.Add("TechnoCallConnectionString", this.connStringTechnoCall);
            //this.configTable.Add("LinesDataSet", this.linesDS);
            //this.configTable.Add("ccMonitoringModel", this.ccMonitoringModel);
            this.configTable.Add("TechnoCallSqlConnection", this.conn);
            this.configTable.Add("FirstStart", this.firstStart);
            return configTable;
        }
    }
}

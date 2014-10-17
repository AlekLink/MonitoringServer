using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitoringServer.Data;

namespace MonitoringServer.Initialize.Modules
{
    public class Data
    {
        private LocalDataSet localDataSet;
        private ccMonitoringEntities ccMonitoringModel;
        private Hashtable configDataTable = new Hashtable();

        public Data()
        {
            this.localDataSet = new LocalDataSet();
            this.ccMonitoringModel = new ccMonitoringEntities();
        }

        public object getConfig()
        {
            this.configDataTable.Add("LocalDataSet", this.localDataSet);
            this.configDataTable.Add("ccMonitoringModel", this.ccMonitoringModel);
            return this.configDataTable;
        }
    }
}

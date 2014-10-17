using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringServer.DataGrabbers.IVR
{
    public class IvrData
    {
        public string ivrName { get; set; }
        public int lineIdle { get; set; }
        public int lineIvr { get; set; }
        public int lineWaiting { get; set; }
        public int lineAgent { get; set; }
        public int transactionCount { get; set; }
        public int wqueueCount { get; set; }

        public IvrData(string ivrName)
        {
            this.ivrName = ivrName;
        }
    }
}

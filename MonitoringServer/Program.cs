using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if TEST
            MonitoringServerService monitoringServerService = new MonitoringServerService();
            monitoringServerService.StartInt();
            Console.ReadLine();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new MomitoringServerService() 
            };
            ServiceBase.Run(ServicesToRun);
#endif

            
        }
    }
}

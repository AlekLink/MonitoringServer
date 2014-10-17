using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using MonitoringServer.Initialize.Modules;

namespace MonitoringServer.Initialize
{
    public class Intializer
    {
        private Hashtable mainConfig = new Hashtable();
        

        public object allInit()
        {
            MonitoringServerService.log.Info("Инициализация...");
            try
            {
                mainConfig.Add("Ivr", ivr());
                MonitoringServerService.log.Debug("Модуль IVR проинициализирован.");
                mainConfig.Add("WebServer", webServer());
                MonitoringServerService.log.Debug("Модуль WebServer проинициализирован.");
                mainConfig.Add("Data", data());
                MonitoringServerService.log.Debug("Модуль Data проинициализирован.");
            }
            catch (Exception ex)
            {
                MonitoringServerService.log.Error("Возникла ошибка при инициализыции приложения.", ex);
            }
            finally
            {
                MonitoringServerService.log.Info("Инициализация завершена.");
            }
            return mainConfig;
            
        }

        private object ivr()
        {
            Ivr ivrInitClass = new Ivr();
            return ivrInitClass.getConfig(); 
        }

        private object webServer()
        {
            Modules.WebServer webServerInitClass = new Modules.WebServer();
            return webServerInitClass.getConfig();
        }

        private object data()
        {
            Modules.Data dataInitClass = new Modules.Data();
            return dataInitClass.getConfig();
        }

    }
}

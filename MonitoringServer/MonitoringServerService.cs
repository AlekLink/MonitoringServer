using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using log4net;
using log4net.Config;
using System.Collections;


namespace MonitoringServer
{
    public partial class MonitoringServerService : ServiceBase
    {
        public static readonly ILog log = LogManager.GetLogger(typeof(MonitoringServerService));
        private Initialize.Intializer iniatize;
        private DataGrabbers.IVR.IvrGrabber ivrGrabber; 

        public MonitoringServerService()
        {
            XmlConfigurator.Configure();
            iniatize = new Initialize.Intializer();
            InitializeComponent();
        }
        /// <summary>
        /// Ничего не добавлять в этот метод. Он работает только в Debug режиме.
        /// </summary>
        internal void StartInt()
        {
            OnStart(null);
        }

        /// <summary>
        ///  Запускаем сервис
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            //Инициализируе конфигурацию приложения.
            var configuration = (Hashtable)iniatize.allInit();
            //Выделяем таблицы помодульно.
            var ivrConfig = (Hashtable)configuration["Ivr"];//Модуль IVR
            var webServerConfig = (Hashtable)configuration["WebServer"];// Модуль WebServer
            var dataConfig = (Hashtable)configuration["Data"];//Модуль хранения данных внутри приложения.
            
            //WebServer.Server webServer = new WebServer.Server(webServerConfig, dataConfig);
            
            try
            {
                ivrGrabber = new DataGrabbers.IVR.IvrGrabber(ivrConfig, dataConfig);
                if (ivrGrabber.start())
                    log.Info("Модуль IVR запущен.");
                else
                    log.Info("Не удалось запустить IVR.");
                
            }
            catch (Exception ex) { log.Error("Возникла ошибка при запуске модуля IVR", ex); }
            
            try
            {
                WebServer.Server webServer = new WebServer.Server(webServerConfig, dataConfig);
                if (webServer.start())
                {
                    log.Info("Модуль WebServer запущен.");
                }
                else
                {
                    log.Info("Не удалось запустить WebServer.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Возникла ошибка при запуске модуля WebServer", ex);
            }


        }

        /// <summary>
        ///  Останавливаем сервис
        /// </summary>
        protected override void OnStop()
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Configuration;

namespace MonitoringServer.Initialize.Modules
{
    public class WebServer
    {
        private Hashtable configTable = new Hashtable();
        private Socket serverSocket;
        private IPAddress ipAddress;
        private int port;
        private int maxNOfConnection;
        private int serverTimeout;

        public WebServer()
        {
            try
            {
                this.ipAddress = IPAddress.Parse(ConfigurationManager.AppSettings["LocalIpAddress"]);
                this.port = Convert.ToInt32(ConfigurationManager.AppSettings["ServerPort"]);
                this.maxNOfConnection = Convert.ToInt32(ConfigurationManager.AppSettings["MaxNOfConnection"]);
                this.serverTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["ServerTimeout"]);
                //Конфигурируем Socket
                this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.serverSocket.Bind(new IPEndPoint(this.ipAddress, this.port));
                this.serverSocket.Listen(this.maxNOfConnection);
                this.serverSocket.ReceiveTimeout = this.serverTimeout;
                this.serverSocket.SendTimeout = this.serverTimeout;
            }
            catch (Exception ex)
            {
                MonitoringServerService.log.Error("Возникла ошибка при инициализации модуля WebServer.", ex);
            }
        }

        public object getConfig()
        {
            this.configTable.Add("ServerSocket", this.serverSocket);
            this.configTable.Add("Timeout", this.serverTimeout);
            return this.configTable;
        }
 
    }
}

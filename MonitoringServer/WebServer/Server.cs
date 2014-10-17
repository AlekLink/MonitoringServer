using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Collections;

namespace MonitoringServer.WebServer
{
    public class Server
    {
        private Hashtable configWebServerTable;
        private Hashtable configDataTable;
        private Socket serverSocket;
        private Encoding charEncoder;
        private Dictionary<string, string> extensions;
        public bool running = false;
        private int timeOut;
        private string contentPath;

        public Server(Hashtable configWebServerTable, Hashtable configDataTable)
        {
            this.configWebServerTable = configWebServerTable;
            this.configDataTable = configDataTable;
            this.serverSocket = (Socket)this.configWebServerTable["ServerSocket"];
            this.timeOut = (Int32)this.configWebServerTable["Timeout"];
            this.charEncoder = Encoding.UTF8;
            this.extensions = new Dictionary<string, string>() { { "htm", "text/html" }, { "html", "text/html" },{ "xml", "text/xml" },{ "txt", "text/plain" }};
        }

        /// <summary>
        /// Запускаем WebServer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="maxNOfCon"></param>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        public bool start()
        {
            if (running)
            {
                return false;
            }
                running = true;

            //Ждем подключения и создаем новые потоки.
            Thread requestListenerT = new Thread(() =>
            {
                while (running)
                {
                    Socket clientSocket;
                    try
                    {
                        clientSocket = serverSocket.Accept();
                        //Создаем поток для клиента и продолжаем слушать сокет.
                        Thread requestHandler = new Thread(() =>
                        {
                            clientSocket.ReceiveTimeout = timeOut;
                            clientSocket.SendTimeout = timeOut;
                            try
                            {
                                handleTheRequest(clientSocket);
                            }
                            catch
                            {
                                try { clientSocket.Close(); }
                                catch (Exception ex) { MonitoringServerService.log.Error("Возникла ошибка при попытке закрыть clientSocket.", ex); }
                            }
                        });
                        requestHandler.Start();
                    }
                    catch (Exception ex) { MonitoringServerService.log.Error("Возникло исключение при установки связи с клиентом (serverSocket.Accept()).", ex); }
                }
            });
            requestListenerT.Start();
            
            return true;
            
        }

        private void stop()
        {
            if (running)
            {
                try
                {
                    serverSocket.Close();
                }
                catch { }
                
                serverSocket = null;
                running = false;

                MonitoringServerService.log.Info("WebServer остановлен.");
            }
        }

        private void handleTheRequest(Socket clientSocket)
        {
            byte[] buffer = new byte[10240];
            Thread.Sleep(370); //Задержка для того чтобы клиент смог успеть записать данные в поток. Иначе сообщение приходит без Body.
            int receivedBCount = clientSocket.Receive(buffer);
            string strReceived = charEncoder.GetString(buffer, 0, receivedBCount);
            Console.WriteLine(strReceived);
            Console.WriteLine();
            string httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));
            //Console.WriteLine(httpMethod);
            int start = strReceived.IndexOf(httpMethod) + httpMethod.Length + 1;
            int length = strReceived.LastIndexOf("HTTP") - start - 1;
            string requestUrl = strReceived.Substring(start, length);

            string requestedFile;
            if (httpMethod.Equals("GET") || httpMethod.Equals("POST"))
            {
                requestedFile = requestUrl.Split('?')[0];
            }
            else
            {
                notImplemented(clientSocket);
                return;
            }

            requestedFile = requestedFile.Replace("/", "\\").Replace("\\..", "");
            start = requestedFile.LastIndexOf('.') + 1;

            if (start > 0)
            {
                length = requestedFile.Length - start;
                string extension = requestedFile.Substring(start, length);
                if (extensions.ContainsKey(extension))
                    if (File.Exists(contentPath + requestedFile))
                        sendOkResponse(clientSocket, File.ReadAllBytes(contentPath + requestedFile), extensions[extension]);
                    else
                        notFound(clientSocket);
            }
            else
            {
                if (requestedFile.Substring(length - 1, 1) != "\\")
                    requestedFile += "\\";
                if (File.Exists(contentPath + requestedFile + "index.htm"))
                    sendOkResponse(clientSocket, File.ReadAllBytes(contentPath + requestedFile + "\\index.htm"), "text/html");
                else if (File.Exists(contentPath + requestedFile + "index.html"))
                    sendOkResponse(clientSocket, File.ReadAllBytes(contentPath + requestedFile + "\\index.html"), "text/html");
                else
                    notFound(clientSocket);
            }
         }

        private void notImplemented(Socket clientSocket)
        {
            sendResponse(clientSocket,"<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"> </head><body><h2>Atasoy Simple Web Server</h2><div>501 - Method Not Implemented</div></body></html>", "501 Not Implemented", "text/html");
        }

        private void notFound(Socket clientSocket)
        {
            
            sendResponse(clientSocket, "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><h2>Atasoy Simple Web Server</h2><div>404 - Not Found</div></body></html>", "404 Not Found", "text/html");
        }
        
        private void sendOkResponse(Socket clientSocket, byte[] bContent, string contentType)
        {
            sendResponse(clientSocket, bContent, "200 OK", contentType);
        }

        private void sendResponse(Socket clientSocket, string strContent, string responseCode, string contentType)
        {
            byte[] bContent = charEncoder.GetBytes(strContent);
            sendResponse(clientSocket, bContent, responseCode, contentType);
        }

        private void sendResponse(Socket clientSocket, byte[] bContent, string responseCode, string contentType)
        {
            try
            {
                byte[] bHeader = charEncoder.GetBytes("HTTP/1.1 " + responseCode + "\r\n"
                          + "Server: Atasoy Simple Web Server\r\n"
                          + "Content-Length: " + bContent.Length.ToString() + "\r\n"
                          + "Connection: close\r\n"
                          + "Content-Type: " + contentType + "\r\n\r\n");
                clientSocket.Send(bHeader);
                clientSocket.Send(bContent);
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                MonitoringServerService.log.Error("Возникла ошибка при отправке ответа клиенту.", ex);
            }
        }
    }
}

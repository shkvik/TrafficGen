using Newtonsoft.Json;
using SuperSocket.SocketEngine.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SNN.HttpServer
{
    //{\"id\":1,\"data\":[1,2,3,4,5,6]}
    public class HttpServer
    {
        private Thread HttpServerGetter;
        private HttpListener Listener;

        private void MainHttpServerGetter(object o)
        {
            if(o is HttpServer server)
            {
                while (true)
                {
                    // Ожидаем входящие подключения
                    HttpListenerContext context = server.Listener.GetContext();

                    // Получаем данные запроса
                    HttpListenerRequest request = context.Request;
                    string url = request.Url.ToString();
                    Console.WriteLine("Received request: " + url);

                    // Отправляем ответ
                    HttpListenerResponse response = context.Response;

                    var msg = new Message();
                    var ts = new TimeSerias();
                    ts.id = 1;

                    ts.data.Add(1);
                    ts.data.Add(2);
                    ts.data.Add(3);
                    ts.data.Add(4);
                    ts.data.Add(5);
                    ts.data.Add(6);

                    msg.serias.Add(ts);

                    string responseString = JsonConvert.SerializeObject(msg);
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();
                }
            }
        }

        public HttpServer()
        {
            Listener = new HttpListener();
            Listener.Prefixes.Add("http://localhost:8081/getData/");
            Listener.Start();

            Console.WriteLine("Http Server started.");

            HttpServerGetter = new Thread(new ParameterizedThreadStart(MainHttpServerGetter));
            HttpServerGetter.Start(this);
        }
    }
}

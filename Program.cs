using System;
using System.Net;
using System.Net.Configuration;
using System.Text;
using System.Threading;
using EasyModbus;
using Newtonsoft.Json;
using SNN.HttpServer;
using SNN.Modbus;
using TrafficGen.WebSocket;

namespace TrafficGen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var webSocketServer = new WSServer();

            var modbusGenerator = new MBGenerator();

            var httpServer = new HttpServer();

        }
    }
}

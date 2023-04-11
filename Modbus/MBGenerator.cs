using EasyModbus;
using Newtonsoft.Json.Linq;
using SNN.Modbus.Json;
using SNN.WebSocket;
using SuperSocket.SocketEngine.Configuration;
using SuperWebSocket;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SNN.Modbus
{
    public class MBGenerator
    {
        private ModbusServer Server;

        private ModbusServer Server2;

        private ModbusClient Client;

        private ModbusClient Client2;

        private Thread GenerateModbusTraffic;

        private List<Connection> Connections;

        private StreamHandler StreamHandler;

        private void MainGenerateModbusTraffic(object o)
        {
            if (o is MBGenerator generator)
            {
                short value = 0;
                int counter = 0;
                while (true)
                {

                    Thread.Sleep(1000);
                    Console.WriteLine(Connections[2].Storage.HoldingRegisters.Guid);
                    for (int i = 0; i < 200; i++)
                    {
                        if(Connections != null)
                        {

                            Console.Write($"[{Connections[2].Storage.Coils.TimeSeriesList[1].TimeSerias[i]}]");
                        }
                    }
                    Console.WriteLine("----");
                    counter++;
                }

            }
        }

        public MBGenerator()
        {
            StreamHandler = new StreamHandler();

            Server = new EasyModbus.ModbusServer();
            Server.LocalIPAddress = new IPAddress(new byte[] { 127, 0, 0, 228 });
            Server.Port = 502;
            Server.Listen();

            Server2 = new EasyModbus.ModbusServer();
            Server2.LocalIPAddress = new IPAddress(new byte[] { 127, 0, 0, 229 });
            Server2.Port = 502;
            Server2.Listen();


            Client = new EasyModbus.ModbusClient("127.0.0.228", 502);
            Client.Connect("127.0.0.228", 502);


            GetPort(Client);
            

            Client2 = new EasyModbus.ModbusClient("127.0.0.229", 502);
            Client2.Connect();
            GetPort(Client2);


            Connections = new List<Connection>();

            for (int i = 0; i < 10; i++)
            {
                Connections.Add(new Connection(new byte[] { 127, 0, 0, Convert.ToByte(110 + i) }, Mode.ReadWrite));
            }

            GenerateModbusTraffic = new Thread(new ParameterizedThreadStart(MainGenerateModbusTraffic));
            GenerateModbusTraffic.Start(this);
        }


        public static string GetPort(ModbusClient client)
        {
            Type myType = typeof(ModbusClient);

            FieldInfo myField = myType.
                GetField("tcpClient", BindingFlags.NonPublic | BindingFlags.Instance);

            TcpClient reflectionClient = (TcpClient)myField.GetValue(client);
            int port = ((System.Net.IPEndPoint)reflectionClient.Client.LocalEndPoint).Port;
            string ip = ((System.Net.IPEndPoint) reflectionClient.Client.LocalEndPoint).Address.ToString();

            return $"{ip}:{port}";
        }
    }
}

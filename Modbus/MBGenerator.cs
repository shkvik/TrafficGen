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

        private Thread GenerateModbusTraffic { get; set; }
        private List<Connection> Connections { get; set; }

        public MBGenerator()
        {

            Connections = new List<Connection>();

            for (int i = 0; i < 10; i++)
            {
                Connections.Add(new Connection(
                    new byte[] { 127, 0, 0, Convert.ToByte(110 + i) }, Mode.OnlyRead));
            }

            GenerateModbusTraffic = new Thread(
                new ParameterizedThreadStart(MainGenerateModbusTraffic));

            GenerateModbusTraffic.Start(this);
        }

        private void MainGenerateModbusTraffic(object o)
        {
            if (o is MBGenerator generator)
            {
                int counter = 0;
                Connections[2].Storage.PrintAllGuids();

                

                while (true)
                {

                    Thread.Sleep(1000);
                    if (false)
                    {
                        Console.WriteLine(Storage
                            .GetTsSequenseByGuid(Connections[2].Storage
                                .ActivityCoils.Guid.ToString()));
                    }

                    counter++;
                }

            }
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

        public IEnumerable<Connection> GetConnections()
        {
            foreach(var item in Connections)
            {
                yield return item;
            }
        }
    }
}

using EasyModbus;
using Newtonsoft.Json.Linq;
using SNN.Modbus.Json;
using SuperSocket.SocketEngine.Configuration;
using SuperWebSocket;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;


namespace SNN.Modbus
{
    public class MBGenerator
    {
        private ModbusServer Server;

        private ModbusServer Server2;

        private ModbusClient Client;

        private ModbusClient Client2;

        private Thread GenerateModbusTraffic;


        private Storage storage;

        private Connection Connection;

        private void MainGenerateModbusTraffic(object o)
        {
            if (o is MBGenerator generator)
            {
                short value = 0;
                while (true)
                {
                    //Client.ReadHoldingRegisters(0, 2);

                    //generator.Server.holdingRegisters.localArray[1] = value;
                    //generator.Server.discreteInputs.localArray[1] = true;
                    //generator.Server.inputRegisters.localArray[1] = 228;
                    //generator.Server.coils.localArray[1] = true;


                    //storage.PushHoldingRegister(1, generator.Server.inputRegisters.localArray[1]);


                    //generator.Client.WriteSingleRegister(0, value);
                    //generator.Client.ReadHoldingRegisters(0, 2);
                    //generator.Client.ReadInputRegisters(0, 2);
                    //generator.Client.ReadDiscreteInputs(0, 2);
                    //generator.Client.ReadCoils(0, 2);

                    //generator.Client2.WriteSingleRegister(0, value);
                    //generator.Client2.ReadHoldingRegisters(0, 2);
                    //generator.Client2.ReadInputRegisters(0, 2);
                    //generator.Client2.ReadDiscreteInputs(0, 2);
                    //generator.Client2.ReadCoils(0, 2);

                    //Thread.Sleep(1000);
                    //value = Convert.ToInt16(value + 1);
                }

            }
        }

        public MBGenerator()
        {
            

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

            storage = new Storage(GetPort(Client), "502");

            Connection = new Connection(new byte[] { 127,0,0,111 }, Mode.ReadWrite);

            var Connection2 = new Connection(new byte[] { 127, 0, 0, 112 }, Mode.ReadWrite);
            var Connection3 = new Connection(new byte[] { 127, 0, 0, 113 }, Mode.ReadWrite);
            var Connection4 = new Connection(new byte[] { 127, 0, 0, 114 }, Mode.ReadWrite);
            var Connection5 = new Connection(new byte[] { 127, 0, 0, 115 }, Mode.ReadWrite);
            var Connection6 = new Connection(new byte[] { 127, 0, 0, 116 }, Mode.ReadWrite);
            var Connection7 = new Connection(new byte[] { 127, 0, 0, 117 }, Mode.ReadWrite);
            var Connection8 = new Connection(new byte[] { 127, 0, 0, 118 }, Mode.ReadWrite);
            var Connection9 = new Connection(new byte[] { 127, 0, 0, 119 }, Mode.ReadWrite);

            GenerateModbusTraffic = new Thread(new ParameterizedThreadStart(MainGenerateModbusTraffic));
            GenerateModbusTraffic.Start(this);
        }

        public void PrepareSendData()
        {
            //var data = new DataJson();

            var discreteInputs = new DiscreteInputs();
            //discreteInputs.Registers
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

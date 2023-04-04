using EasyModbus;
using Newtonsoft.Json.Linq;
using SuperSocket.SocketEngine.Configuration;
using SuperWebSocket;
using System;
using System.Threading;


namespace SNN.Modbus
{
    public class MBGenerator
    {
        private ModbusServer Server;
        private ModbusClient Client;

        private Thread GenerateModbusTraffic;

        private void MainGenerateModbusTraffic(object o)
        {
            if (o is MBGenerator generator)
            {
                short value = 0;
                while (true)
                {
                    Client.ReadHoldingRegisters(0, 2);

                    generator.Server.holdingRegisters.localArray[1] = value;
                    generator.Server.holdingRegisters.localArray[2] = value;
                    generator.Server.discreteInputs.localArray[1] = true;
                    generator.Server.inputRegisters.localArray[1] = 228;
                    generator.Server.coils.localArray[1] = true;
                    generator.Server.coils.localArray[2] = true;


                    generator.Client.WriteSingleRegister(0, value);
                    generator.Client.ReadHoldingRegisters(0, 2);
                    generator.Client.ReadInputRegisters(0, 2);
                    generator.Client.ReadDiscreteInputs(0, 2);
                    generator.Client.ReadCoils(0, 2);

                    Thread.Sleep(1000);
                    value = Convert.ToInt16(value + 1);
                }

            }
        }

        public MBGenerator()
        {
            Server = new EasyModbus.ModbusServer();
            Server.Port = 502;
            Server.Listen();

            Client = new EasyModbus.ModbusClient("127.0.0.10", 502);
            Client.Connect();

            GenerateModbusTraffic = new Thread(new ParameterizedThreadStart(MainGenerateModbusTraffic));
            GenerateModbusTraffic.Start(this);
        }
    }
}

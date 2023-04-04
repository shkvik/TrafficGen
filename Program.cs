using System;
using System.Threading;
using EasyModbus;
using TrafficGen.WebSocket;

namespace TrafficGen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var Server = new WSServer();
            Server.Start();

            var server = new ModbusServer();

            server.Port = 502;
            server.Listen();

            var client = new EasyModbus.ModbusClient("127.0.0.10", 502);
            
            client.Connect();
            short value = 0;
            while (true)
            {
                
                client.ReadHoldingRegisters(0, 2);

                server.holdingRegisters.localArray[1] = value;
                server.holdingRegisters.localArray[2] = value;
                server.discreteInputs.localArray[1] = true;
                server.inputRegisters.localArray[1] = 228;
                server.coils.localArray[1] = true;
                server.coils.localArray[2] = true;
                client.WriteSingleRegister(0, value);
                client.ReadHoldingRegisters(0, 2);
                client.ReadInputRegisters(0, 2);
                client.ReadDiscreteInputs(0, 2);
                client.ReadCoils(0,2);
                //var collection = client.ReadHoldingRegisters(0, 10);
                //foreach(var item in collection)
                //{
                //    Console.Write(item);
                //}
                //Console.WriteLine();
                Thread.Sleep(1000);
                value = Convert.ToInt16(value + 1);
            }
        }
    }
}

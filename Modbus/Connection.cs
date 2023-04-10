﻿using CsvHelper;
using EasyModbus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrafficGen;

namespace SNN.Modbus
{
    public class Sequence
    {
        public float Value { get; set; }
    }

    [Flags]
    public enum Mode
    {
        OnlyRead, OnlyWrite, ReadWrite,
    }

    public enum ReadFunction
    {
        ReadCoils,
        DiscreteInputs,
        HoldingRegisters,
        InputRegisters
    }

    class Connection
    {
        private ModbusServer ModbusServer;

        private ModbusClient ModbusClient;

        public Guid Guid { get; private set; }
        public string Client { get; set; }
        public string Server { get; set; }

        public int DefaultStartingAddress = 0;
        public int DefaultQuantity = 4;
        public int DefaultDelay = 1000;

        public delegate bool[] ReadBoolean(int startingAddress, int quantity);
        public delegate int[] ReadInt(int startingAddress, int quantity);

        private Dictionary<string, Thread> FunctionProcessing;
        public Storage Storage;

        public Connection(byte[] serverIP, Mode mode = Mode.OnlyRead)
        {
            ModbusServer = new ModbusServer();
            ModbusServer.LocalIPAddress = new IPAddress(serverIP);
            ModbusServer.Port = 502;
            ModbusServer.Listen();

            ModbusClient = new EasyModbus.ModbusClient(BytesToString(serverIP), 502);
            ModbusClient.ConnectionTimeout = 2000;
            ModbusClient.Connect(BytesToString(serverIP), 502);

            FunctionProcessing = new Dictionary<string, Thread>();

            Server = $"{serverIP}:502";
            Client = GetClientAddress(ModbusClient);

            Storage = new Storage(Client, Server);
            Guid = Guid.NewGuid();


            switch (mode)
            {
                case Mode.OnlyRead: ReadWorking(); break;
                case Mode.OnlyWrite: WriteWorking(); break;
                case Mode.ReadWrite: WriteWorking(); ReadWorking(); break;
            }
        }

        public void ReadWorking()
        {
            string[] functionsNames =
            {
                "ReadCoils",
                "ReadDiscreteInputs",
                "ReadHoldingRegisters",
                "ReadInputRegisters"
            };
            
            FunctionProcessing.Add("ReadCoils", new Thread(new ParameterizedThreadStart(MainReadCoils)));
            FunctionProcessing.Add("ReadDiscreteInputs", new Thread(new ParameterizedThreadStart(MainReadDiscreteInputs)));
            FunctionProcessing.Add("ReadHoldingRegisters", new Thread(new ParameterizedThreadStart(MainReadHoldingRegisters)));
            FunctionProcessing.Add("ReadInputRegisters", new Thread(new ParameterizedThreadStart(MainReadInputRegisters)));

            foreach (string functionName in functionsNames)
                FunctionProcessing[functionName].Start(this);

        }

        public void WriteWorking()
        {
            string[] functionsNames =
            {
                "WriteSingleCoil",
                "WriteSingleRegister",
            };

            FunctionProcessing.Add("WriteSingleCoil", new Thread(new ParameterizedThreadStart(MainWriteSingleCoil)));
            FunctionProcessing.Add("WriteSingleRegister", new Thread(new ParameterizedThreadStart(MainWriteSingleRegister)));
        
            foreach (var functionName in functionsNames)
                FunctionProcessing[functionName].Start(this);

        }

        public void MainReadCoils(object o)
        {
            if(o is Connection connection)
            {
                ReadValues(connection, (ReadBoolean)ModbusClient.ReadCoils, ReadFunction.ReadCoils);
            }
        }

        public void MainReadDiscreteInputs(object o)
        {
            if (o is Connection connection)
            {
                ReadValues(connection, (ReadBoolean)ModbusClient.ReadDiscreteInputs, ReadFunction.DiscreteInputs);
            }
        }

        public void MainReadHoldingRegisters(object o)
        {
            if (o is Connection connection)
            {
                ReadValues(connection, (ReadInt)ModbusClient.ReadHoldingRegisters, ReadFunction.HoldingRegisters);
            }
        }

        public void MainReadInputRegisters(object o)
        {
            if (o is Connection connection)
            {
                ReadValues(connection, (ReadInt)ModbusClient.ReadInputRegisters, ReadFunction.InputRegisters);
            }
        }

        public void MainWriteSingleCoil(object o)
        {
            if (o is Connection connection)
            {
                WriteValues(connection, (Action<int,bool>)ModbusClient.WriteSingleCoil, singleCoil: true);
            }
        }

        public void MainWriteSingleRegister(object o)
        {
            if (o is Connection connection)
            {
                
                WriteValues(connection, (Action<int, int>)ModbusClient.WriteSingleRegister, singleRegister: 20);
            }
        }

        public void MainWriteMultipleCoils(object o)
        {
            if (o is Connection connection)
            {
                WriteValues(connection, (Action<int, bool[]>)ModbusClient.WriteMultipleCoils, multipleCoils: new bool[] { true });
            }
        }

        public void MainWriteMultipleRegisters(object o)
        {
            if (o is Connection connection)
            {
                WriteValues(connection, (Action<int, int[]>)ModbusClient.WriteMultipleRegisters, multipleRegisters: new int[] { 132 });
            }
        }

        private void ReadValues(Connection connection, object readFunction, ReadFunction function)
        {
            var mutex = new Mutex();
            Random rand = new Random();
            while (true)
            {
                if (connection.ModbusClient.Connected)
                {
                    mutex.WaitOne();
                    try
                    {
                        UseReadDelegate(readFunction);
                    }
                    catch (Exception error)
                    {
                        if (Program.Debug)
                        {
                            Console.WriteLine(error.Message);
                        }
                    }

                    Thread.Sleep(DefaultDelay);
                    mutex.ReleaseMutex();
                }

                var ReadCoils = rand.Next(0, 40);
                var ReadHoldingRegisters = rand.Next(0, 40);
                var ReadInputRegisters = rand.Next(0, 40);
                var ReadDiscreteInputs = rand.Next(0, 40);

                switch (function)
                {
                    case ReadFunction.ReadCoils:
                        Storage.PushActivityFunction(Function.ReadCoils, ReadCoils);
                        FillCoils(); 
                        break;

                    case ReadFunction.HoldingRegisters:
                        Storage.PushActivityFunction(Function.ReadHoldingRegisters, ReadHoldingRegisters);
                        FillHoldingRegister(); 
                        break;

                    case ReadFunction.InputRegisters:
                        Storage.PushActivityFunction(Function.ReadInputRegisters, ReadInputRegisters);
                        FillInputRegister(); 
                        break;

                    case ReadFunction.DiscreteInputs:
                        Storage.PushActivityFunction(Function.ReadDiscreteInputs, ReadDiscreteInputs);
                        FillDiscreteInputs(); 
                        break;
                }

                Storage.UpdateActivityFunctions();

            }
        }
        private void FillCoils(int quantity = 4)
        {
            var result = new List<bool>();

            for (int i = 0; i < quantity; i++)
            {
                result.Add(ModbusServer.coils.localArray[i + 1]); 
            }

            Storage.PushCoil(result);

        }

        private void FillDiscreteInputs(int quantity = 4)
        {
            var result = new List<bool>();

            for (int i = 0; i < quantity; i++)
            {
                result.Add(ModbusServer.discreteInputs.localArray[i + 1]);
            }

            Storage.PushDiscreteInput(result);
        }

        private void FillInputRegister(int quantity = 4)
        {

            var result = new List<short>();

            for (int i = 0; i < quantity; i++)
            {
                result.Add(ModbusServer.inputRegisters.localArray[i + 1]);
            }

            Storage.PushInputRegister(result);
        }

        private void FillHoldingRegister(int quantity = 4)
        {

            var result = new List<short>();

            for (int i = 0; i < quantity; i++)
            {
                result.Add(ModbusServer.holdingRegisters.localArray[i + 1]);
            }

            Storage.PushHoldingRegister(result);

        }

        private void WriteValues(
            Connection connection,
            object writeFunction,
            bool? singleCoil = null,
            int? singleRegister = null,
            bool[] multipleCoils = null,
            int[] multipleRegisters = null)
        {
            var mutex = new Mutex();

            while (true)
            {
                if (connection.ModbusClient.Connected)
                {
                    mutex.WaitOne();
                    try
                    {
                        Random rand = new Random();
                        UseWriteDelegate(
                            writeFunction,
                            singleCoil,
                            singleRegister: Convert.ToInt16(rand.Next(0, 65000)),
                            multipleCoils,
                            multipleRegisters
                        );
                    }
                    catch (Exception error)
                    {
                        if (Program.Debug)
                        {
                            Console.WriteLine(error.Message);
                        }
                    }

                    Thread.Sleep(DefaultDelay);
                    mutex.ReleaseMutex();
                }
            }
        }


        private void UseWriteDelegate(
            object o,
            bool? singleCoil = null,
            int? singleRegister = null,
            bool[] multipleCoils = null,
            int[] multipleRegisters = null
        )
        {

            if (o is Action<int,bool> writeSingleCoil)
                if(singleCoil != null)
                    writeSingleCoil.Invoke(DefaultStartingAddress, (bool)singleCoil);
               
            if (o is Action<int,int> writeSingleRegister)
                if (singleRegister != null)
                    writeSingleRegister.Invoke(DefaultStartingAddress, (int)singleRegister);

            if (o is Action<int, bool[]> writeMultipleCoils)
            {
                if (multipleCoils != null)
                {
                    try
                    {
                        writeMultipleCoils.Invoke(DefaultStartingAddress, multipleCoils);
                    }
                    catch (Exception error)
                    {
                        if (Program.Debug)
                        {
                            Console.WriteLine(error.Message);
                        }
                    }
                }
            }
                
            if (o is Action<int, int[]> writeMultipleRegisters)
            {
                if (multipleRegisters != null)
                {
                    try
                    {
                        writeMultipleRegisters.Invoke(DefaultStartingAddress, multipleRegisters);
                    }
                    catch (Exception error)
                    {
                        if (Program.Debug)
                        {
                            Console.WriteLine(error.Message);
                        }
                    }
                }
            }

        }


        private void UseReadDelegate(object o)
        {
            if (o is ReadBoolean readBoolean)
                readBoolean.Invoke(DefaultStartingAddress, DefaultQuantity);
                

            if (o is ReadInt readInt)
                readInt.Invoke(DefaultStartingAddress, DefaultQuantity);

        }


        public List<Sequence> GetSequenceFromCsv(string path)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Sequence>();
                return records.ToList();
            }
        }

        public static string GetClientAddress(ModbusClient client)
        {
            Type myType = typeof(ModbusClient);

            FieldInfo myField = myType.
                GetField("tcpClient", BindingFlags.NonPublic | BindingFlags.Instance);

            TcpClient reflectionClient = (TcpClient)myField.GetValue(client);
            int port = ((System.Net.IPEndPoint)reflectionClient.Client.LocalEndPoint).Port;
            string ip = ((System.Net.IPEndPoint)reflectionClient.Client.LocalEndPoint).Address.ToString();

            return $"{ip}:{port}";
        }

        private string BytesToString(byte[] bytes)
        {
            string result = string.Empty;

            for(int i = 0; i < 3; i++)
                result += $"{bytes[i]}.";

            result += $"{bytes[3]}";

            return result;
        }
        private void GenerateTestCsv()
        {

            var records = new List<Sequence>();

            for(int i = 0; i < 100; i++)
            {
                records.Add(new Sequence() { Value = i });
            }

            using (var writer = new StreamWriter("../../file.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
        }
    }
}

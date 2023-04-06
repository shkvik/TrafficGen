using CsvHelper;
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

    class Connection
    {
        private ModbusServer ModbusServer;

        private ModbusClient ModbusClient;

        public int Id { get; set; }
        public string Client { get; set; }
        public string Server { get; set; }

        public int DefaultStartingAddress = 0;
        public int DefaultQuantity = 4;
        public int DefaultDelay = 10;

        public delegate bool[] ReadBoolean(int startingAddress, int quantity);
        public delegate int[] ReadInt(int startingAddress, int quantity);

        private Dictionary<string, Thread> FunctionProcessing;


        public Connection(byte[] serverIP, Mode mode = Mode.OnlyRead)
        {
            ModbusServer = new ModbusServer();
            ModbusServer.LocalIPAddress = new IPAddress(serverIP);
            ModbusServer.Port = 502;
            ModbusServer.Listen();

            ModbusClient = new EasyModbus.ModbusClient(BytesToString(serverIP), 502);
            ModbusClient.Connect(BytesToString(serverIP), 502);

            Server = $"{serverIP}:502";
            Client = GetClientAddress(ModbusClient);

            switch (mode)
            {
                case Mode.OnlyRead: ReadWorking(); break;
                case Mode.OnlyWrite: WriteWorking(); break;
                case Mode.ReadWrite: WriteWorking(); ReadWorking(); break;
            }
        }

        public void ReadWorking()
        {
            FunctionProcessing = new Dictionary<string, Thread>
            {
                { "ReadCoils", new Thread(new ParameterizedThreadStart(MainReadCoils)) },
                { "ReadDiscreteInputs", new Thread(new ParameterizedThreadStart(MainReadDiscreteInputs)) },
                { "ReadHoldingRegisters", new Thread(new ParameterizedThreadStart(MainReadHoldingRegisters)) },
                { "ReadInputRegisters", new Thread(new ParameterizedThreadStart(MainReadInputRegisters)) }
            };

            foreach(var item in FunctionProcessing)
                item.Value.Start(this);
            
        }

        public void WriteWorking()
        {
            FunctionProcessing = new Dictionary<string, Thread>
            {
                { "WriteSingleCoil", new Thread(new ParameterizedThreadStart(MainWriteSingleCoil)) },
                { "WriteSingleRegister", new Thread(new ParameterizedThreadStart(MainWriteSingleRegister)) },
                //Опасная хуета
                //{ "WriteMultipleCoils", new Thread(new ParameterizedThreadStart(MainWriteMultipleCoils)) },
                //{ "WriteMultipleRegister", new Thread(new ParameterizedThreadStart(MainWriteMultipleRegisters)) }
            };

            foreach (var item in FunctionProcessing)
                item.Value.Start(this);

        }

        public void MainReadCoils(object o)
        {
            if(o is Connection connection)
            {
                ReadValues(connection, (ReadBoolean)ModbusClient.ReadCoils);
            }
        }

        public void MainReadDiscreteInputs(object o)
        {
            if (o is Connection connection)
            {
                ReadValues(connection, (ReadBoolean)ModbusClient.ReadDiscreteInputs);
            }
        }

        public void MainReadHoldingRegisters(object o)
        {
            if (o is Connection connection)
            {
                ReadValues(connection, (ReadInt)ModbusClient.ReadHoldingRegisters);
            }
        }

        public void MainReadInputRegisters(object o)
        {
            if (o is Connection connection)
            {
                ReadValues(connection, (ReadInt)ModbusClient.ReadInputRegisters);
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

        private void ReadValues(Connection connection, object readFunction)
        {
            var mutex = new Mutex();

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
                        Console.WriteLine(error.Message);
                    }

                    Thread.Sleep(DefaultDelay);
                    mutex.ReleaseMutex();
                }
            }
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
                        UseWriteDelegate(
                            writeFunction,
                            singleCoil,
                            singleRegister,
                            multipleCoils,
                            multipleRegisters
                        );
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error.Message);
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
                        Console.WriteLine(error.Message);
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
                        Console.WriteLine(error.Message);
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

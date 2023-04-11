using SNN.HttpServer;
using SNN.Modbus.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TrafficGen;

namespace SNN.Modbus
{
    public enum Function
    {
        ReadCoils,
        ReadDiscreteInputs,
        ReadHoldingRegisters,
        ReadInputRegisters,
        WriteSingleCoil,
        WriteSingleRegister,
        WriteMultipleCoils,
        WriteMultipleRegiste,
    }


    public interface IBuffer<T>
    {
        List<T> GetTimeSeries(int index = 0);
    }

    public class Buffer<T> : IBuffer<T>
    {
        private int BufferSize = 200;
        private int CurrentCount;

        public Guid Guid { get; private set; }
        public List<T> TimeSerias { get; private set; }

        public Buffer()
        {
            InitBuffer();
        }

        private void InitBuffer()
        {
            this.Guid = Guid.NewGuid();
            this.TimeSerias = new List<T>(BufferSize);
            this.CurrentCount = 0;

            for (int i = 0; i < BufferSize; i++)
                TimeSerias.Add(default(T));

        }

        public void Push(T value)
        {
            CheckUpdateCounter();

            if (CurrentCount > BufferSize - 1)
            {
                TimeSerias.RemoveAt(0);
                TimeSerias.Add(value);
            }
            else
            {
                TimeSerias[CurrentCount] = value;
            }

            CurrentCount++;
        }


        public T GetLastValue()
        {
            return CurrentCount < BufferSize ? TimeSerias[CurrentCount] : TimeSerias[BufferSize - 1];            
        }

        private void CheckUpdateCounter()
        {
            if(CurrentCount == int.MaxValue)
                CurrentCount = 0;
        }

        public List<T> GetTimeSeries(int index= 0)
        {
            return this.TimeSerias;
        }

        public T this[int index]
        {
            get { return TimeSerias[index]; }
            set { TimeSerias[index] = value; }
        }

        public static implicit operator List<T>(Buffer<T> bufferFunction)
        {
            return bufferFunction.TimeSerias;
        }

    }

    public class BufferCollection<T> : IBuffer<T>
    {
        private int Columns = 0;
        public List<Buffer<T>> TimeSeriesList { get; private set; }
        public Guid Guid { get; private set; }

        public BufferCollection()
        {
            InitBuffer();
        }

        public void Push(List<T> values)
        {
            for (int i = 0; i < Columns; i++)
                TimeSeriesList[i].Push(values[i]);
        }

        public void AddColumn()
        {
            TimeSeriesList.Add(new Buffer<T>());
            Columns = TimeSeriesList.Count;
        }

        private void InitBuffer()
        {
            this.Guid = Guid.NewGuid();
            this.TimeSeriesList = new List<Buffer<T>>();
        }

        public Guid GetTimeSeriasGuid(int index)
        {
            return TimeSeriesList[index].Guid;
        }

        public List<T> GetTimeSeries(int index)
        {
            return TimeSeriesList[index].TimeSerias;
        }
    }


    class Storage
    {

        public string Client { get; private set; }
        public string Server { get; private set; }
        public Guid Guid { get; private set; }


        public Buffer<int> ActivityFunctions;
        public Buffer<int> ActivityDiscreteInputs;
        public Buffer<int> ActivityCoils;
        public Buffer<int> ActivityInputRegisters;
        public Buffer<int> ActivityHoldingRegisters;


        public Buffer<int> ReadCoils;
        public Buffer<int> ReadDiscreteInputs;
        public Buffer<int> ReadHoldingRegisters;
        public Buffer<int> ReadInputRegisters;
        public Buffer<int> WriteSingleCoil;
        public Buffer<int> WriteSingleRegister;
        public Buffer<int> WriteMultipleCoils;
        public Buffer<int> WriteMultipleRegister;


        public BufferCollection<short>  HoldingRegisters;
        public BufferCollection<bool>   DiscreteInputs;
        public BufferCollection<short>  InputRegisters;
        public BufferCollection<bool>   Coils;

        public Dictionary<string, List<int>>    BufferDictActivity;
        public Dictionary<string, List<short>>  BufferDictFloat;
        public Dictionary<string, List<bool>>   BufferDictLogic;

        public Storage(string client, string server) 
        {
            Client = client;
            Server = server;
            StorageInit();
        }

        private void StorageInit()
        {
            ActivityFunctions           = new Buffer<int>();
            ActivityDiscreteInputs      = new Buffer<int>();
            ActivityCoils               = new Buffer<int>();
            ActivityInputRegisters      = new Buffer<int>();
            ActivityHoldingRegisters    = new Buffer<int>();


            ReadCoils               = new Buffer<int>();
            ReadDiscreteInputs      = new Buffer<int>();
            ReadHoldingRegisters    = new Buffer<int>();
            ReadInputRegisters      = new Buffer<int>();
            WriteSingleCoil         = new Buffer<int>();
            WriteSingleRegister     = new Buffer<int>();
            WriteMultipleCoils      = new Buffer<int>();
            WriteMultipleRegister   = new Buffer<int>();


            HoldingRegisters = new BufferCollection<short>();
            DiscreteInputs   = new BufferCollection<bool>();
            InputRegisters   = new BufferCollection<short>();
            Coils            = new BufferCollection<bool>();


            BufferDictActivity  = new Dictionary<string, List<int>>();
            BufferDictFloat     = new Dictionary<string, List<short>>();
            BufferDictLogic     = new Dictionary<string, List<bool>>();

            Guid = Guid.NewGuid();

        }

        public void PushHoldingRegister(List<short> value)
        {
            Push(this.HoldingRegisters, value);
        }

        public void PushDiscreteInput(List<bool> value)
        {
            Push(this.DiscreteInputs, value);
        }

        public void PushInputRegister(List<short> value)
        {
            Push(this.InputRegisters, value);
        }

        public void PushCoil(List<bool> value)
        {
            Push(this.Coils, value);
        }

        public void PushActivityFunction(Function function, int value)
        {
            string bufferName = function.ToString();
            FieldInfo fieldInfo = this.GetType().GetField(bufferName);
            var buffer = (Buffer<int>)fieldInfo.GetValue(this);
            try
            {

            }
            catch(Exception error)
            {
                if (Program.Debug)
                {
                    Console.WriteLine("class Storage : PushActivityFunction");
                    Console.WriteLine(error.Message);
                }
            }
            buffer.Push(value);
        }

        public void UpdateActivityFunctions()
        {
            ActivityFunctions.Push(
                ActivityHoldingRegisters.GetLastValue() +
                ActivityDiscreteInputs.GetLastValue() +
                ActivityInputRegisters.GetLastValue() +
                ActivityCoils.GetLastValue()
            );
        }


        private void PasteActivityMap()
        {
            BufferDictActivity.Add(ActivityFunctions.Guid.ToString(),       ActivityFunctions.GetTimeSeries());
            BufferDictActivity.Add(ActivityDiscreteInputs.Guid.ToString(),  ActivityDiscreteInputs.GetTimeSeries());
            BufferDictActivity.Add(ActivityCoils.Guid.ToString(),           ActivityCoils.GetTimeSeries());
            BufferDictActivity.Add(ActivityInputRegisters.Guid.ToString(),  ActivityInputRegisters.GetTimeSeries());
            BufferDictActivity.Add(ActivityHoldingRegisters.Guid.ToString(),ActivityFunctions.GetTimeSeries());
        }

        private void PasteFunctionsMap()
        {
            BufferDictActivity.Add(ReadCoils.Guid.ToString(), ReadCoils.GetTimeSeries());
            BufferDictActivity.Add(ReadDiscreteInputs.Guid.ToString(), ReadDiscreteInputs.GetTimeSeries());
            BufferDictActivity.Add(ReadHoldingRegisters.Guid.ToString(), ReadHoldingRegisters.GetTimeSeries());
            BufferDictActivity.Add(ReadInputRegisters.Guid.ToString(), ReadInputRegisters.GetTimeSeries());
            BufferDictActivity.Add(WriteSingleCoil.Guid.ToString(), WriteSingleCoil.GetTimeSeries());
            BufferDictActivity.Add(WriteSingleRegister.Guid.ToString(), WriteSingleRegister.GetTimeSeries());
            BufferDictActivity.Add(WriteMultipleCoils.Guid.ToString(), WriteMultipleCoils.GetTimeSeries());
            BufferDictActivity.Add(WriteMultipleRegister.Guid.ToString(), WriteMultipleRegister.GetTimeSeries());
        }

        private void PastLogicRegistersMap()
        {
            foreach(var item in DiscreteInputs.TimeSeriesList)
            {
                BufferDictLogic.Add(item.Guid.ToString(), item.GetTimeSeries());
            }

            foreach (var item in Coils.TimeSeriesList)
            {
                BufferDictLogic.Add(item.Guid.ToString(), item.GetTimeSeries());
            }
        }



        private void PastFloatRegisterMap() 
        {
            foreach (var item in HoldingRegisters.TimeSeriesList)
            {
                BufferDictFloat.Add(item.Guid.ToString(), item.GetTimeSeries());
            }

            foreach (var item in InputRegisters.TimeSeriesList)
            {
                BufferDictFloat.Add(item.Guid.ToString(), item.GetTimeSeries());
            }
        }

        public void CreateMap()
        {
            PasteActivityMap();
            PasteFunctionsMap();
            PastLogicRegistersMap();
            PastFloatRegisterMap();
        }

        private void Push<T>(BufferCollection<T> buffer, List<T> value)
        {
            try
            {
                buffer.Push(value);    
            }
            catch(Exception error)
            {
                if (Program.Debug)
                {
                    Console.WriteLine(error.Message);
                }
            }
            
        }
    }
}

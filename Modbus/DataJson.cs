using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace SNN.Modbus.Json
{
    public class TimeSerias<Type>
    {
        public int Id;
        public List<Type> Values = new List<Type>(); 
    } 

    public class DiscreteInputs     
    {
        public List<TimeSerias<bool>> Registers = new List<TimeSerias<bool>>();
    }

    public class Coils              
    {
        public List<TimeSerias<bool>> Registers = new List<TimeSerias<bool>>();
    }

    public class InputRegisters     
    { 
        public List<TimeSerias<int>>  Registers = new List<TimeSerias<int>>();  
    }

    public class HoldingRegisters   
    {
        public List<TimeSerias<int>> Registers = new List<TimeSerias<int>>();
    }

    public class FunctionsActivity
    {
        public int ReadCoils;
        public int ReadDiscreteInputs;
        public int ReadHoldingRegisters;
        public int ReadInputRegisters;
        public int WriteSingleCoil;
        public int WriteSingleRegister;
        public int WriteMultipleCoils;
        public int WriteMultipleRegister;
    }

    public class ModbusData 
    {
        public FunctionsActivity FunctionsActivity = new FunctionsActivity();

        public List<TimeSerias<short>>    HoldingRegisters = new List<TimeSerias<short>>();
        public List<TimeSerias<bool>>   DiscreteInputs = new List<TimeSerias<bool>>();
        public List<TimeSerias<short>>    InputRegisters = new List<TimeSerias<short>>();
        public List<TimeSerias<bool>>   Coils = new List<TimeSerias<bool>>();
    }

    public class ConnectionPacket
    {
        public long TimeStamp;
        public string Client;
        public string Server;
        public string Protocol;
        public ModbusData Modbus = new ModbusData();
    }

    public class DataJson
    {
        public List<ConnectionPacket> packets = new List<ConnectionPacket>();
    }
}

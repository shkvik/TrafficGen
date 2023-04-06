using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace SNN.Modbus.Json
{
    public class TimeSerias16b   
    {
        public int Id;
        public List<int> TimeSerias = new List<int>(); 
    } 
    public class TimeSeriasBool  { public int Id; public List<bool> TimeSerias = new List<bool>(); }


    public class DiscreteInputs     
    {
        public List<TimeSeriasBool> Registers = new List<TimeSeriasBool>();
    }
    public class Coils              { public List<TimeSeriasBool> Registers = new List<TimeSeriasBool>(); }
    public class InputRegisters     { public List<TimeSerias16b>  Registers = new List<TimeSerias16b>();  }
    public class HoldingRegisters   { public List<TimeSerias16b>  Registers = new List<TimeSerias16b>();  }


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

    public class Modbus 
    {
        public FunctionsActivity FunctionsActivity = new FunctionsActivity();

        public HoldingRegisters HoldingRegisters = new HoldingRegisters();
        public DiscreteInputs DiscreteInputs = new DiscreteInputs();
        public InputRegisters InputRegisters = new InputRegisters();
        public Coils Coils = new Coils();
    }

    public class ConnectionPacket
    {
        public int Id;
        public string Source;
        public string Destination;
        public string Protocol;
        public Modbus Modbus = new Modbus();
    }

    public class DataJson
    {
        public List<ConnectionPacket> packets = new List<ConnectionPacket>();
    }
}

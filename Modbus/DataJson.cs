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

        public List<TimeSerias<short>>      HoldingRegisters = new List<TimeSerias<short>>();
        public List<TimeSerias<bool>>       DiscreteInputs = new List<TimeSerias<bool>>();
        public List<TimeSerias<short>>      InputRegisters = new List<TimeSerias<short>>();
        public List<TimeSerias<bool>>       Coils = new List<TimeSerias<bool>>();
    }


    public class TimeSeriasFrame
    {
        public int train { get; set; }
        public string status { get; set; }
        public List<int> ts_original { get; set; }
        public List<int> ts_predict { get; set; }
    }


    public class ConnectionsPage
    {
        public string guid;
        public string timeSeriasGuid;
        public string client;
        public string server;
        public string protocol;
        public string status;
        public int trained;
    }

    public class FunctionData
    {
        public string code;
        public string ts_guid;
        public string name;
        public string type;
        public string status;
        public int train;
        public string access;
    }

    public class RegisterData
    {
        public int id;
        public string ts_guid;
        public string name;
        public string status;
        public int train;

    }

    public class ConnectionDataPage
    {
        public string activityGuid;

        public List<FunctionData> functions;
        public List<RegisterData> holdingRegisters;
        public List<RegisterData> discreteInputs;
        public List<RegisterData> inputRegisters;
        public List<RegisterData> coils;
    }

    public class ConnectionsJson
    {
        public List<ConnectionDataPage> connections = new List<ConnectionDataPage>();
    }
}

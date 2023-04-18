using Newtonsoft.Json;
using SNN.Modbus;
using SNN.Modbus.Json;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TrafficGen;

namespace SNN.WebSocket
{

    public enum Action
    {
        Error,
        GetConections,
        GetConnectionData,
        TimeSeriasStream
    }

    public class WSHandler
    {
        
        private readonly WebSocketServer _server;
        private readonly MBGenerator _generator;

        public WSHandler(WebSocketServer server, MBGenerator generator)
        {
            _server = server;
            _generator = generator;
        }
        private void GetRequset(string msg)
        {
            try
            {

            }
            catch(Exception error) 
            {
                if (Program.Debug)
                {
                    Console.WriteLine(error.Message);
                }
            }
        }

        public Action ParseRequest(string msg)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<JsonRpcRequest>(msg);
                return (Action)Enum.Parse(typeof(Action), request.Method);
            }
            catch (Exception error)
            {
                if (Program.Debug)
                {
                    Console.WriteLine($"WSHandler : ParseRequest {error.Message}");
                }
            }
            return Action.Error;

        }

        public IEnumerable<ConnectionsPage> GetConnectionsGuidFromStorage()
        {
            foreach(var item in _generator.GetConnections())
            {
                var show = _generator.GetConnection(item.Guid.ToString())
                        .Storage.ActivityFunctions.Guid.ToString();

                yield return  new ConnectionsPage()
                {
                    guid = item.Guid.ToString(),
                    timeSeriasGuid = _generator.GetConnection(item.Guid.ToString())
                        .Storage.ActivityFunctions.Guid.ToString(),

                    client = item.Client,
                    server = item.Server,
                    protocol = "Modbus",
                    status = "Learning",
                    trained = 50
                };
            }
        }

        public FunctionData GetFunctionData<T>(
            Buffer<T>   buffer, 
            string      code,
            string      name,
            string      status,
            int         train,
            string      access)
        {

            return new FunctionData()
            {
                code = code,
                ts_guid = buffer.Guid.ToString(),
                name = name,
                type = typeof(T) == typeof(short) ? "16 bit" : "Descrete",
                status= status,
                train = train,
                access = access
            };
        }


        public List<FunctionData> GetFunctionsData(string ConnectionGuid)
        {
            var list = new List<FunctionData>();

            var readCoils            = _generator.GetConnection(ConnectionGuid).Storage.ReadCoils;
            var readDiscreteInputs   = _generator.GetConnection(ConnectionGuid).Storage.ReadDiscreteInputs;
            var readHoldingRegisters = _generator.GetConnection(ConnectionGuid).Storage.ReadHoldingRegisters;
            var readInputRegisters   = _generator.GetConnection(ConnectionGuid).Storage.ReadInputRegisters;

            list.Add(GetFunctionData(buffer: readCoils,name: "Read Coils",code: "0x01", status: "Learning",
                train: 70, access: "Read"));

            list.Add(GetFunctionData(buffer: readDiscreteInputs, name: "Read Discrete Inputs", code: "0x02",
                status: "Learning", train: 70, access: "Read"));
                
            list.Add(GetFunctionData(buffer: readHoldingRegisters, name: "Read Holding Registers", code: "0x03",
                status: "Learning", train: 70, access: "Read"));
                
            list.Add(GetFunctionData(buffer: readInputRegisters, name: "Read Input Registers", code: "0x04",
                status: "Learning", train: 70, access: "Read"));

            return list;
        }




        public List<RegisterData> GetRegisterData<T>(BufferCollection<T> collection)
        {
            return collection.TimeSeriesList.Select((item, index) => new RegisterData
            {
                id = index,
                ts_guid = item.Guid.ToString(),
                name = "Untitled",
                status = "Learning",
                train = 70,

            }).ToList();
                        
        }

        public ConnectionDataPage GetConnectionDataGuidsFromStorage(string ConnectionGuid)
        {

            return new ConnectionDataPage()
            {
                activityGuid = _generator.GetConnection(ConnectionGuid)
                    .Storage.ActivityFunctions.Guid.ToString(),

                functions = GetFunctionsData(ConnectionGuid),

                holdingRegisters = GetRegisterData(_generator.GetConnection(ConnectionGuid)
                    .Storage.HoldingRegisters),
                        
                discreteInputs = GetRegisterData(_generator.GetConnection(ConnectionGuid)
                    .Storage.DiscreteInputs),

                inputRegisters = GetRegisterData(_generator.GetConnection(ConnectionGuid)
                    .Storage.InputRegisters),

                coils = GetRegisterData(_generator.GetConnection(ConnectionGuid)
                    .Storage.Coils)
            };
        }
    }
}

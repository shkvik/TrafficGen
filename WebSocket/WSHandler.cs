using Newtonsoft.Json;
using SNN.Modbus;
using SNN.Modbus.Json;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrafficGen;

namespace SNN.WebSocket
{

    public enum Action
    {
        Error,
        ReturnConections,
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
                yield return  new ConnectionsPage()
                {
                    guid = item.Guid.ToString(),
                    client = item.Client,
                    server = item.Server,
                    protocol = "Modbus",
                    status = "Learning",
                    trained = 50
                };
            }
        }
    }
}

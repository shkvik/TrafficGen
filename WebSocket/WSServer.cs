using Newtonsoft.Json;
using SNN.Modbus;
using SNN.Modbus.Json;
using SNN.WebSocket;
using SuperSocket.SocketBase;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static System.Collections.Specialized.BitVector32;


namespace TrafficGen.WebSocket
{
    public class WSServer
    {
        

        private WebSocketServer Server;

        private Dictionary<string, Thread> TimeSeriesStreamSessions;

        private static object syncObject = new object();

        private WSHandler Handler { get; set; }

        public WSServer(MBGenerator generator)
        {
            Server = new WebSocketServer();
            Server.Setup(8080);

            Server.NewSessionConnected += OnNewSessionConnected;
            Server.NewMessageReceived += OnNewMessageReceived;
            Server.SessionClosed += OnSessionClosed;

            Server.Start();


            Thread.Sleep(4000);
            TimeSeriesStreamSessions = new Dictionary<string, Thread>();

            Handler = new WSHandler(Server, generator);

        }

        private void MainTimeSeriesStream(object o)
        {
            if (o is Tuple<WebSocketSession, string> threadArgs)
            {
                var flag = false;

                while (true)
                {
                    lock(syncObject)
                    {
                        try
                        {

                            var show = JsonConvert.SerializeObject(new JsonRpcResponse()
                            {
                                Result = new TimeSeriasFrame() 
                                {
                                    train = 40,
                                    status = "Learning",
                                    ts_original = Storage.GetTsSequenseByGuid(threadArgs.Item2)
                                } 
                            });

                            var predict = StateHandler.GetPredict(threadArgs.Item2);

                            threadArgs.Item1.Send(JsonConvert.SerializeObject(new JsonRpcResponse()
                            {
                                Result = new TimeSeriasFrame()
                                {
                                    train = 40,
                                    status = flag ? "Learning" : "Alarm",
                                    ts_original = Storage.GetTsSequenseByGuid(threadArgs.Item2),
                                    ts_predict = predict != null ? predict.Select(x => Convert.ToInt32(x)).ToList() : null
                        }
                            }));

                            //flag = !flag;
                        }
                        catch(Exception error)
                        {
                            if (Program.Debug)
                            {
                                Console.WriteLine(error.Message);
                            }
                        }
                        
                    }
                    
                    Thread.Sleep(500);
                }

            }
        }

        private void OnNewSessionConnected(WebSocketSession session)
        {
            Console.WriteLine($"Session {session.SessionID} connected");
        }

        private void OnNewMessageReceived(WebSocketSession session, string message)
        {
            switch (Handler.ParseRequest(message))
            {
                case SNN.WebSocket.Action.Error:

                    break;

                case SNN.WebSocket.Action.GetConections: 
                    SendConnectionsGuids(session);
                    break;

                case SNN.WebSocket.Action.GetConnectionData:
                    SendConnectionDataGuids(session, message); 
                    break;

                case SNN.WebSocket.Action.TimeSeriasStream:
                    OpenNewStream(session, message);
                    break;

                default:
                    break;
            }
            Console.WriteLine($"Received message: {message}");
        }


        private void OnSessionClosed(WebSocketSession session, CloseReason reason)
        {
            session.Close();
            TimeSeriesStreamSessions.Remove(session.SessionID);

            Console.WriteLine($"Session {session.SessionID} closed: {reason}");
            Console.WriteLine($"Thread {session.SessionID} removed");
            Console.WriteLine("Clients: ");

            foreach (var item in TimeSeriesStreamSessions)
            {
                Console.WriteLine(item.Key);
            }

        }

        private void OpenNewStream(WebSocketSession session, string message)
        {
            var request = JsonConvert.DeserializeObject<JsonRpcRequest>(message);
            var guid = (string)request.Params.First();

            var threadArgs = new Tuple<WebSocketSession, string>(session, guid);

            TimeSeriesStreamSessions.Add(session.SessionID,
                new Thread(new ParameterizedThreadStart(MainTimeSeriesStream)));

            TimeSeriesStreamSessions[session.SessionID].Start(threadArgs);

            Console.WriteLine($"Session {session.SessionID} connected");
        }

        private void SendConnectionDataGuids(WebSocketSession session, string message)
        {
            var connectionGuid = (string)JsonConvert.
                DeserializeObject<JsonRpcRequest>(message).Params.First();

            session.Send(JsonConvert.SerializeObject(Handler
                .GetConnectionDataGuidsFromStorage(connectionGuid)));
        }

        private void SendConnectionsGuids(WebSocketSession session)
        {
            session.Send(JsonConvert.SerializeObject(Handler.
                GetConnectionsGuidFromStorage()));
        }

    }
}

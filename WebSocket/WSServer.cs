using Newtonsoft.Json;
using SNN.Modbus;
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
                while (true)
                {
                    lock(syncObject)
                    {
                        try
                        {
                            threadArgs.Item1.Send(JsonConvert.SerializeObject(new JsonRpcResponse()
                            {
                                Result = Storage.GetTsSequenseByGuid(threadArgs.Item2)
                            }));
                        }
                        catch(Exception error)
                        {
                            Console.WriteLine(error.Message);
                        }
                        
                    }
                    
                    Thread.Sleep(2000);
                }

            }
        }

        private void OnNewSessionConnected(WebSocketSession session)
        {
            TimeSeriesStreamSessions.Add(session.SessionID, new Thread(new ParameterizedThreadStart(MainTimeSeriesStream)));
            TimeSeriesStreamSessions[session.SessionID].Start(session);

            Console.WriteLine($"Session {session.SessionID} connected");
        }

        private void OnNewMessageReceived(WebSocketSession session, string message)
        {
            switch (Handler.ParseRequest(message))
            {
                case SNN.WebSocket.Action.Error:

                    break;

                case SNN.WebSocket.Action.ReturnConections: 
                    SendConnectionsGuids(session);
                    break;

                case SNN.WebSocket.Action.TimeSeriasStream:

                    break;

                default:
                    break;
            }
            Console.WriteLine($"Received message: {message}");
        }


        private void OnSessionClosed(WebSocketSession session, CloseReason reason)
        {
            TimeSeriesStreamSessions.Remove(session.SessionID);

            Console.WriteLine($"Session {session.SessionID} closed: {reason}");
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

            TimeSeriesStreamSessions.Add(session.SessionID, new Thread(new ParameterizedThreadStart(MainTimeSeriesStream)));
            TimeSeriesStreamSessions[session.SessionID].Start(threadArgs);

            Console.WriteLine($"Session {session.SessionID} connected");
        }

        private void SendConnectionsGuids(WebSocketSession session)
        {
            session.Send(JsonConvert.SerializeObject(Handler.GetConnectionsGuidFromStorage()));
        }

    }
}

using Newtonsoft.Json;
using SNN.Modbus;
using SNN.WebSocket;
using SuperSocket.SocketBase;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;


namespace TrafficGen.WebSocket
{
    public class WSServer
    {
        private WebSocketServer Server;

        private Dictionary<string, Thread> SessionsSendingData;

        private static object syncObject = new object();

        public StreamHandler Stream;

        public WSServer()
        {
            Server = new WebSocketServer();
            Server.Setup(8080);

            Server.NewSessionConnected += OnNewSessionConnected;
            Server.NewMessageReceived += OnNewMessageReceived;
            Server.SessionClosed += OnSessionClosed;

            Server.Start();


            Stream = new StreamHandler();
            Thread.Sleep(4000);
            SessionsSendingData = new Dictionary<string, Thread>();
            


        }

        private void MainSendingData(object o)
        {
            if (o is WebSocketSession session)
            {

                while (true)
                {
                    lock(syncObject)
                    {
                        try
                        {
                            //Stream.PasteFrameToBuffer(
                            //    registers: Connection.Storage.HoldingRegisters,
                            //    buffer: Stream.BufferHoldingRegisters
                            //);

                            var message = Stream.BuildJsonPacket(
                                "client",
                                "server"
                            );

                            session.Send(message);
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
            SessionsSendingData.Add(session.SessionID, new Thread(new ParameterizedThreadStart(MainSendingData)));
            SessionsSendingData[session.SessionID].Start(session);

            Console.WriteLine($"Session {session.SessionID} connected");
        }

        private void OnNewMessageReceived(WebSocketSession session, string message)
        {
            Console.WriteLine($"Received message: {message}");
        }

        private void OnSessionClosed(WebSocketSession session, CloseReason reason)
        {
            SessionsSendingData.Remove(session.SessionID);

            Console.WriteLine($"Session {session.SessionID} closed: {reason}");
            Console.WriteLine("Clients: ");

            foreach (var item in SessionsSendingData)
            {
                Console.WriteLine(item.Key);
            }
        }
        private void PreparePacket()
        {

            //string responseString = JsonConvert.SerializeObject(msg)
        }

    }
}

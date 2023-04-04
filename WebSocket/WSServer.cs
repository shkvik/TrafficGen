using SuperSocket.SocketBase;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TrafficGen.WebSocket
{
    public class WSServer
    {
        private WebSocketServer Server;

        private Dictionary<string, Thread> SessionsSendingData;

        private static object syncObject = new object();

        public WSServer()
        {
            Server = new WebSocketServer();
            Server.Setup(8080);

            Server.NewSessionConnected += OnNewSessionConnected;
            Server.NewMessageReceived += OnNewMessageReceived;
            Server.SessionClosed += OnSessionClosed;

            Server.Start();

            SessionsSendingData= new Dictionary<string, Thread>();
        }

        private void MainSendingData(object o)
        {
            if (o is WebSocketSession session)
            {

                while (true)
                {
                    lock(syncObject)
                    {
                        session.Send("Hey you");
                    }
                    
                    Thread.Sleep(1000);
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

    }
}

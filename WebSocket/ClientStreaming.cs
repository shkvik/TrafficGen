using SNN.Modbus;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SNN.WebSocket
{
    [Flags]
    public enum StreamType
    {
        FLOAT, LOGIC
    }

    public class ClientRequestMsg
    {
        public StreamType type;
        public Guid tsGUID;
    }

    public class ClientStreaming
    {
        private WebSocketSession _session;
        private Thread _streaming;
        private int _streamDelay;

        public ClientStreaming(WebSocketSession session, ClientRequestMsg msg)
        {
            _session = session;
        }
        
        public void InitSessionStream()
        {
            _streaming = new Thread(new ParameterizedThreadStart(MainStreaming));
            _streaming.Start();
        }

        private void MainStreaming(object o)
        {
            if (o is WebSocketSession session)
            {
                while (true)
                {
                    try
                    {
                        //session.Send(message);
                    }
                    catch (Exception error)
                    {

                    }

                    Thread.Sleep(_streamDelay);
                }

            }
        }

        private void FindTimeSerias(Guid tsGuid)
        {

        }
    }
}

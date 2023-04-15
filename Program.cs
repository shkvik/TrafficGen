using SNN.HttpServer;
using SNN.Modbus;
using TrafficGen.WebSocket;


namespace TrafficGen
{
    internal class Program
    {
        public static bool Debug = true;

        static void Main(string[] args)
        {

            var modbusGenerator = new MBGenerator();

            var httpServer = new HttpServer();

            var webSocketServer = new WSServer(modbusGenerator);

        }
    }
}

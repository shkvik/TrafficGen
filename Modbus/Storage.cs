using SNN.HttpServer;
using SNN.Modbus.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNN.Modbus
{
    


    //Контроль 200 значений для регистра
    class Storage
    {

        public string Client;
        public string Server;


        public Dictionary<int, List<short>> HoldingRegisters;
        public Dictionary<int, List<bool>> DiscreteInputs;
        public Dictionary<int, List<short>> InputRegisters;
        public Dictionary<int, List<bool>> Coils;


        public Storage(string client, string server) 
        {
            Client = client;
            Server = server;
        }

        public void PushHoldingRegister(int id, short value)
        {
            PushShort(HoldingRegisters, id, value);
        }


        public void PushDiscreteInput(int id, bool value)
        {
            PushBool(DiscreteInputs, id, value);
        }

        public void PushInputRegister(int id, short value)
        {
            PushShort(InputRegisters, id, value);
        }

        public void PushCoil(int id, bool value)
        {
            PushBool(Coils, id, value);
        }






        private void PushBool(Dictionary<int, List<bool>> dict, int id, bool value)
        {
            if (dict == null)
            {
                dict = new Dictionary<int, List<bool>>();
            }
            if (!dict.ContainsKey(id))
            {
                dict = new Dictionary<int, List<bool>>();
                dict.Add(id, new List<bool>() { value });
            }
            else
                dict[id].Add(value);
        }

        private void PushShort(Dictionary<int, List<short>> dict, int id, short value)
        {
            if(dict == null)
            {
                dict = new Dictionary<int, List<short>>();
            }
            if (!dict.ContainsKey(id))
            {
                dict.Add(id, new List<short>() { value });
            }
            else
                dict[id].Add(value);
        }
    }
}

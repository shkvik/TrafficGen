using SNN.HttpServer;
using SNN.Modbus.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrafficGen;

namespace SNN.Modbus
{
    


    //Контроль 200 значений для регистра
    class Storage
    {

        public string Client;
        public string Server;

        public int FrameQuantity;

        public List<List<short>> HoldingRegisters;
        public List<List<bool>> DiscreteInputs;
        public List<List<short>> InputRegisters;
        public List<List<bool>> Coils;


        public Storage(string client, string server, int quantity = 4) 
        {
            Client = client;
            Server = server;

            FrameQuantity = quantity;

            HoldingRegisters = new List<List<short>>();
            DiscreteInputs = new List<List<bool>>();
            InputRegisters = new List<List<short>>();
            Coils = new List<List<bool>>();
        }

        public void PushHoldingRegister(List<short> values)
        {
            PushShort(this.HoldingRegisters, values);
        }

        public void PushDiscreteInput(List<bool> values)
        {
            PushBool(this.DiscreteInputs, values);
        }

        public void PushInputRegister(List<short> values)
        {
            PushShort(this.InputRegisters, values);
        }

        public void PushCoil(List<bool> values)
        {
            PushBool(this.Coils, values);
        }



        private void PushBool(List<List<bool>> dict, List<bool> values)
        {
            try
            {
                dict.Add(values);
            }
            catch (Exception error) 
            {
                if (Program.Debug)
                {
                    Console.WriteLine(error.Message);
                }
            }

                
        }

        private void PushShort(List<List<short>> dict, List<short> values)
        {
            try
            {
                dict.Add(values);       
            }
            catch(Exception error)
            {
                if (Program.Debug)
                {
                    Console.WriteLine(error.Message);
                }
            }
            
        }
    }
}

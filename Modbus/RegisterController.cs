using EasyModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SNN.Modbus
{
    class RegisterController
    {
        //new SequenceTransporter<short>("../../sinus/sin_0.csv");
        private int CsvCount = 10;
        private ModbusServer Server { get; }
        private Thread UpdateRegistersValues { get; set; }
        private List<SequenceTransporter<short>> SinusSequenseList { get; set; }


        public RegisterController(ModbusServer server)
        {
            Server = server;
            SequenseInit();
            ThreadInit();
        }

        private void ThreadInit()
        {
            UpdateRegistersValues = new Thread(new ParameterizedThreadStart(MainUpdateRegistersValues));
            UpdateRegistersValues.Start(this);
        }

        private void MainUpdateRegistersValues(object o)
        {
            if(o is RegisterController controller)
            {
                while(true)
                {
                    Server.inputRegisters.localArray[1] = SinusSequenseList[1].GetNextValue();
                    Server.inputRegisters.localArray[2] = SinusSequenseList[2].GetNextValue();
                    Server.inputRegisters.localArray[2] = SinusSequenseList[3].GetNextValue();
                    Server.inputRegisters.localArray[2] = SinusSequenseList[4].GetNextValue();

                    Server.holdingRegisters.localArray[1] = SinusSequenseList[1].GetNextValue();
                    Server.holdingRegisters.localArray[2] = SinusSequenseList[2].GetNextValue();
                    Server.holdingRegisters.localArray[3] = SinusSequenseList[3].GetNextValue();
                    Server.holdingRegisters.localArray[4] = SinusSequenseList[4].GetNextValue();

                    Thread.Sleep(1000);
                }
            }
        }

        private void SequenseInit()
        {
            SinusSequenseList = new List<SequenceTransporter<short>>();
            for (int i = 0; i < CsvCount; i++)
            {
                SinusSequenseList.Add(new SequenceTransporter<short>($"../../sinus/sin_{i}.csv"));
            }
        }
    }
}

using EasyModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SNN.Modbus
{
    public class RegisterController
    {
        //new SequenceTransporter<short>("../../sinus/sin_0.csv");
        private int CsvCount = 10;
        private ModbusServer Server { get; }
        private Thread UpdateRegistersValues { get; set; }
        public List<SequenceTransporter<short>> SinusSequenseList { get; private set; }


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
                var counter = 0;
                var test = false;
                while(true)
                {
                    Server.inputRegisters.localArray[1] = SinusSequenseList[0].GetNextValue();
                    Server.inputRegisters.localArray[2] = SinusSequenseList[1].GetNextValue();
                    Server.inputRegisters.localArray[3] = SinusSequenseList[2].GetNextValue();
                    Server.inputRegisters.localArray[4] = SinusSequenseList[3].GetNextValue();

                    Server.holdingRegisters.localArray[1] = SinusSequenseList[4].GetNextValue();
                    Server.holdingRegisters.localArray[2] = SinusSequenseList[5].GetNextValue();
                    Server.holdingRegisters.localArray[3] = SinusSequenseList[6].GetNextValue();
                    Server.holdingRegisters.localArray[4] = SinusSequenseList[7].GetNextValue();

                    Server.coils.localArray[1] = test;
                    Server.coils.localArray[2] = test;
                    Server.coils.localArray[3] = test;
                    Server.coils.localArray[4] = test;

                    Server.discreteInputs.localArray[1] = test;
                    Server.discreteInputs.localArray[2] = test;
                    Server.discreteInputs.localArray[3] = test;
                    Server.discreteInputs.localArray[4] = test;

                    if(counter > 8)
                    {
                        counter = 0;
                    }

                    if (counter > 4)
                    {
                        test = false;
                        
                    }
                    else
                    {
                        test = true;
                    }
                    counter++;
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

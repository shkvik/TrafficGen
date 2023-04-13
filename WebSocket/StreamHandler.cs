//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Newtonsoft.Json;
//using SNN.Modbus.Json;

//namespace SNN.WebSocket
//{
//    public class StreamHandler
//    {

//        private int BufferRows = 200;
//        private int BufferColumns = 8;

//        public int BufferHoldingRegistersLastIndex = 0;
//        public List<TimeSerias<short>> BufferHoldingRegisters;


//        public StreamHandler() 
//        {
//            BufferHoldingRegisters = new List<TimeSerias<short>>();
//            InitBuffer(BufferHoldingRegisters);
//        }


//        public string BuildJsonPacket(string client, string server)
//        {
//            var frameModbus = new ModbusData()
//            {
//                HoldingRegisters = BufferHoldingRegisters
//            };

//            var packet = new ConnectionPacket()
//            {
//                TimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
//                Client = client,
//                Server = server,
//                Protocol = "Modbus",
//                Modbus = frameModbus
//            };

//            return JsonConvert.SerializeObject(packet); ;
//        }


//        /// <summary>
//        /// Опасная штука
//        /// </summary>
//        /// <typeparam name="Type"></typeparam>
//        /// <param name="registers"> 200 значений максимум</param>
//        /// <param name="buffer">  </param>
//        public void PasteFrameToBuffer<Type>(List<List<Type>> registers, List<TimeSerias<Type>> buffer)
//        {
//            var maxPacketSize = 200;
//            var dbgColumns = 4;

            

//            var reallocateBuffer = false;

//            var frame = RepackStorage(registers);

//            //Если фрейм содержит больше 200, то удалить первые значения
//            if (frame[0].Values.Count > 200)
//            {
//                var remaining = frame[0].Values.Count - maxPacketSize;

//                for(int i = 0;i < dbgColumns; i++)
//                {
//                    for(int j = 0;j < remaining; j++)
//                    {
//                        frame[i].Values.RemoveAt(0);
//                    }
//                }
//            }

//            var commonCount = BufferHoldingRegistersLastIndex + frame[0].Values.Count;

//            var howHave = BufferRows - BufferHoldingRegistersLastIndex;

//            if (commonCount > BufferRows) //Если больше 200
//            {
//                var remaining = commonCount - BufferRows;//Получаем остаток 

//                if (howHave != 0) //Есть ли свободное место
//                {
//                    if (howHave < remaining) //Если свободного пространства меньше, чем остатка для следущего фрема
//                    {
//                        var needDeleteBack = remaining - howHave;
                        
//                        for (int i = 0; i < dbgColumns; i++)// Освобождаем место из начала списка 
//                        {
//                            for(int j = 0;j < needDeleteBack; j++)
//                            {
//                                buffer[i].Values.RemoveAt(0);
//                            }
                            
//                        }
//                        BufferHoldingRegistersLastIndex = buffer[0].Values.Count;
//                        reallocateBuffer = true;
//                    }
//                }
//                else
//                {
//                    for (int i = 0; i < dbgColumns;/* When Debug BufferColumns;*/ i++)// Освобождаем место из начала списка 
//                    {
//                        for(int j = 0;j < remaining; j++)
//                        {
//                            buffer[i].Values.RemoveAt(0);
//                        }
//                    }
//                    BufferHoldingRegistersLastIndex = buffer[0].Values.Count;
//                    reallocateBuffer = true;
//                }
//            }

//            var stepsRows = frame[0].Values.Count;
//            var stepsColumns = dbgColumns;


//            if (!reallocateBuffer)
//            {
//                for (int i = 0; i < stepsColumns; i++)
//                {
//                    for (int j = 0; j < stepsRows; j++)
//                    {
//                        buffer[i].Values[j + BufferHoldingRegistersLastIndex] = frame[i].Values[j];
//                    }
//                }
//                BufferHoldingRegistersLastIndex = buffer[0].Values.Count;
//            }
//            else
//            {
//                for (int i = 0; i < stepsColumns; i++)
//                {
//                    for (int j = 0; j < stepsRows; j++)
//                    {
//                        buffer[i].Values.Add(frame[i].Values[j]);
//                    }
//                }
//                BufferHoldingRegistersLastIndex = buffer[0].Values.Count;
//            }
            
//        }

//        private List<List<int>> TestStorageFrame(int rows, int columns = 8)
//        {
//            var result = new List<List<int>>();

//            for(int i = 0; i < columns;i++)
//            {
//                result.Add(new List<int>());
//                for(int j = 1;j< rows + 1; j++)
//                {
//                    result[i].Add(j);
//                }
//            }

//            return result;
//        }

//        private void InitBuffer<Type>(List<TimeSerias<Type>> buffer)
//        {
//            for(int i = 0; i < BufferColumns; i++)
//            {
//                buffer.Add(new TimeSerias<Type>() { Id = i });

//                for(int j = 0; j < BufferRows; j++)
//                    if(buffer is List<TimeSerias<short>> integerBuffer)
//                        integerBuffer[i].Values.Add(0);

//                    if(buffer is List<TimeSerias<bool>> boolBuffer)
//                        boolBuffer[i].Values.Add(false);

//            }
//        }

//        public List<TimeSerias<Type>> RepackStorage<Type>(List<List<Type>> registers)
//        {
//            var result = new List<TimeSerias<Type>>();
//            try
//            {
//                for (int i = 0; i < registers[0].Count; i++)
//                {
//                    result.Add(new TimeSerias<Type>() { Id = i });
//                    for (int j = 0; j < registers.Count; j++)
//                    {
//                        result[i].Values.Add(registers[j][i]);
//                    }
//                }
//            }
//            catch(Exception e)
//            {
//                Console.WriteLine(e.Message);
//            }
            

//            return result;
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SNN.Modbus
{
    public enum Status
    {
        Learning,
        Inactivate,
        Alarm,
        Normal
    }

    public class FrameController<T> where T : struct
    {
        private readonly Buffer<T> _originalBuffer;
        private readonly SequenceTransporter<T> _sequenceTransporter;

        private List<T> PredictBuffer { get; set; }
        public Status Status { get; set; }
        public int Trained { get; set; }

        public FrameController(Buffer<T> buffer, SequenceTransporter<T> sequenceTransporter) 
        {
            _originalBuffer = buffer;
            _sequenceTransporter = sequenceTransporter;
            PredictBuffer = _sequenceTransporter.GetPredict();
        }

        public List<T> GetPredict()
        {

            int bufferSize = 200;
            var bufferCount = _originalBuffer.CurrentCount;
            var clearCount = bufferCount % bufferSize;

            if(clearCount == 0)
            {
                PredictBuffer = _sequenceTransporter.GetPredict();
            }

            var result = new List<T>(PredictBuffer);

            for (int i = 0; i < clearCount - 1; i++)
                result[i] = default(T);

            return result;

        }

    }

    public class StateHandler
    {
        private readonly Storage _storage;
        private readonly RegisterController _registerController;

        private static Dictionary<string, FrameController<short>> FramesStorage = new Dictionary<string, FrameController<short>>();

        public StateHandler(Storage storage, RegisterController registerController) 
        {
            _storage = storage;
            _registerController = registerController;

            InitFramesStorage();
        }
        
        public static List<short> GetPredict(string guid)
        {
            if (FramesStorage.ContainsKey(guid))
            {
                return FramesStorage[guid].GetPredict();
            }
            else
            {
                return default(List<short>);
            }       
        }

        private void InitFramesStorage()
        {
            foreach (var (item, index) in _storage.InputRegisters
                .TimeSeriesList.Select((item, index)=> (item, index)))
            {
                FramesStorage.Add(item.Guid.ToString(),
                    new FrameController<short>(item, _registerController.SinusSequenseList[index]));
            }

            foreach (var (item, index) in _storage.HoldingRegisters
                .TimeSeriesList.Select((item, index) => (item, index + 4)))
            {
                FramesStorage.Add(item.Guid.ToString(),
                    new FrameController<short>(item, _registerController.SinusSequenseList[index]));
            }
        }

    }
}

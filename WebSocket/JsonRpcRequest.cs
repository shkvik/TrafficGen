using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNN.WebSocket
{
    public class JsonRpcRequest
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get => "2.0"; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public List<object> Params { get; set; }

        [JsonProperty("id")]
        public long Id { get => DateTime.Now.Ticks; }
    }

    public class JsonRpcResponse
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get => "2.0"; }

        [JsonProperty("result")]
        public object Result { get; set; }

        [JsonProperty("id")]
        public long Id 
        {
            get
            {
                // Вычисляем разницу между текущей датой и временем и 1 января 1970 года
                TimeSpan timeSpan = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                // Получаем количество секунд, прошедших с 1 января 1970 года
                return (long)timeSpan.TotalSeconds;
            }
                
        }
    }
}

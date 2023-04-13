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
        public string Result { get; set; }

        [JsonProperty("id")]
        public long Id { get => DateTime.Now.Ticks; }
    }
}

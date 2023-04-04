using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNN.HttpServer
{
    public class TimeSerias
    {
        public int id;
        public List<int> data = new List<int>();
    }

    public class Message
    {
        public List<TimeSerias> serias = new List<TimeSerias>();
    }
}

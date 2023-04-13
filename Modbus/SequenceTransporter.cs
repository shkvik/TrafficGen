using CsvHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrafficGen;

namespace SNN.Modbus
{

    public class SequenceTransporter<T> where T : struct
    {
        private int CurrentValue = 0;


        private bool ShowSequence { get => false; }
        public List<T> Sequence { get; private set; }
        
        public SequenceTransporter(string path)
        {
            GetTimeSeriasFromCsv(path);
        }

        public T GetNextValue()
        {
            return CurrentValue < Sequence.Count ? 
                Sequence[CurrentValue++] : default;
        }

        private void GetTimeSeriasFromCsv(string path)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                Sequence = csv.GetRecords<T>().ToList();
                if (ShowSequence) PrintSequence();
            }
        }
        private void PrintSequence()
        {
            foreach(var record in Sequence)
            {
                Console.WriteLine(record.ToString());
            }
        }
        public static void GenerateTestCsv()
        {
            var records = new List<int>();

            for (int i = 0; i < 86400; i++)
            {
                records.Add(i + 1);
            }

            using (var writer = new StreamWriter("../../file.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
        }

        
    }
}

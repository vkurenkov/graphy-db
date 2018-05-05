using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphyDb
{
    public class Program
    {
        private static readonly TraceSource MySource = new TraceSource("TraceGraphyDb");

        private static void Main(string[] args)
        {
            Trace.AutoFlush = true;

            IO.DbWriter.InitializeFiles();
            TraceExample();

            Console.WriteLine("Hello");
            Console.ReadLine();
        }

        static void TraceExample()
        {
            MySource.TraceEvent(TraceEventType.Error, 1,
                "Error message.");
            MySource.TraceEvent(TraceEventType.Warning, 2,
                "Warning message.");
        }
    }
}

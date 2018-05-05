using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GraphyDb
{
    public class Program
    {
        private static TraceSource mySource =
          new TraceSource("TraceGraphyDb");

        static void Main(string[] args)
        {
            Trace.AutoFlush = true;
            IO.DbWriter.InitializeFiles();
            Activity1();
            Console.WriteLine("Hello");
            Console.ReadLine();
        }

        static void Activity1()
        {
            mySource.TraceEvent(TraceEventType.Error, 1,
                "Error message.");
            mySource.TraceEvent(TraceEventType.Warning, 2,
                "Warning message.");
        }
    }
}

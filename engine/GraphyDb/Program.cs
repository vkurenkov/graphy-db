using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphyDb.IO;

namespace GraphyDb
{
    public class Program
    {
        private static readonly TraceSource MySource = new TraceSource("TraceGraphyDb");

        private static void Main(string[] args)
        {
            Trace.AutoFlush = true;

            IO.DbWriter.InitializeDatabase();
            IO.DbWriter.WriteNodeBlock(new NodeBlock(true, 1, 522, 32, 1));
            IO.DbWriter.WriteNodeBlock(new NodeBlock(true, 2, 525, 32123, 2));
            IO.DbWriter.WriteNodeBlock(new NodeBlock(true, 3, 523, 32, 3));


            for (int i = 0; i <= 10; i++)
            {
                IO.NodeBlock l1 = IO.DbWriter.ReadNodeBlock(i);
                Console.WriteLine($"Node {l1.NodeId} -> {l1.NextRelationId} with {l1.NextPropertyId}");

            }

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

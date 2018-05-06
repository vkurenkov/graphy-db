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

            IO.DbWriter.WriteGenericStringBlock(new LabelBlock(true, "Branda", 1));
            IO.DbWriter.WriteGenericStringBlock(new PropertyNameBlock(true, "Chandra", 1));
            IO.DbWriter.WriteGenericStringBlock(new StringBlock(true, "A-ah-aha!", 1));


            IO.LabelBlock l = new LabelBlock(IO.DbWriter.ReadGenericStringBlock(DbWriter.LabelPath, 1));
            IO.PropertyNameBlock p =
                new PropertyNameBlock(IO.DbWriter.ReadGenericStringBlock(DbWriter.PropertyNamePath, 1));
            IO.StringBlock s = new StringBlock(IO.DbWriter.ReadGenericStringBlock(DbWriter.StringPath, 1));

            Console.WriteLine($"Label: \"{l.Data}\", Property: \"{p.Data}\", String: \"{s.Data}\"");

            IO.DbWriter.WritePropertyBlock(new NodePropertyBlock(1, false, PropertyType.Float, 12, 24, 32, 2));
            var np = new NodePropertyBlock(IO.DbWriter.ReadPropertyBlock(DbWriter.NodePropertyPath, 1));

            IO.DbWriter.WritePropertyBlock(new EdgePropertyBlock(1, true, PropertyType.Bool, 12, 24, 32, 2));
            var ep = new EdgePropertyBlock(IO.DbWriter.ReadPropertyBlock(DbWriter.EdgePropertyPath, 1));

            Console.WriteLine($"NodeProperty type {np.PtType}, {np.PropertyName}:{np.Value}");
            Console.WriteLine($"EdgeProperty type {ep.PtType}, {ep.PropertyName}:{ep.Value}");


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
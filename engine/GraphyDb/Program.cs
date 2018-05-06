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

            IO.DbControl.InitializeIO();

            DbWriter.WriteGenericStringBlock(new LabelBlock(true, "Branda", 1));
            DbWriter.WriteGenericStringBlock(new PropertyNameBlock(true, "Chandra", 1));
            DbWriter.WriteGenericStringBlock(new StringBlock(true, "A-ah-aha!", 1));


            IO.LabelBlock l = new LabelBlock(IO.DbReader.ReadGenericStringBlock(DbControl.LabelPath, 1));
            IO.PropertyNameBlock p =
                new PropertyNameBlock(IO.DbReader.ReadGenericStringBlock(DbControl.PropertyNamePath, 1));
            IO.StringBlock s = new StringBlock(IO.DbReader.ReadGenericStringBlock(DbControl.StringPath, 1));

            Console.WriteLine($"Label: \"{l.Data}\", Property: \"{p.Data}\", String: \"{s.Data}\"");

            DbWriter.WritePropertyBlock(new NodePropertyBlock(1, false, PropertyType.Float, 12, new byte[4]{5,6,12,1}, 32, 2));
            var np = new NodePropertyBlock(IO.DbReader.ReadPropertyBlock(DbControl.NodePropertyPath, 1));

            DbWriter.WritePropertyBlock(new EdgePropertyBlock(1, true, PropertyType.Bool, 12, new byte[4]{0,0,0,1}, 32, 2));
            var ep = new EdgePropertyBlock(IO.DbReader.ReadPropertyBlock(DbControl.EdgePropertyPath, 1));

            Console.WriteLine($"NodeProperty type {np.PtType}, {np.PropertyName}:{BitConverter.ToSingle(np.Value,0)}");
            Console.WriteLine($"EdgeProperty type {ep.PtType}, {ep.PropertyName}:{BitConverter.ToBoolean(ep.Value,3)}");


            Console.ReadLine();
            DbControl.DeleteDb();
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
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
            Repeat();
            Console.WriteLine();
            Repeat();
        }
        private static void Repeat()
        {
            IO.DbControl.InitializeIO();

            DbWriter.WriteStringBlock(new LabelBlock(true, "Branda", 1));
            DbWriter.WriteStringBlock(new PropertyNameBlock(true, "Chandra", 1));
            DbWriter.WriteStringBlock(new StringBlock(true, "A-ah-aha!", 1));


            IO.LabelBlock l = new LabelBlock(IO.DbReader.ReadGenericStringBlock(DbControl.LabelPath, 1));
            IO.PropertyNameBlock p =
                new PropertyNameBlock(IO.DbReader.ReadGenericStringBlock(DbControl.PropertyNamePath, 1));
            IO.StringBlock s = new StringBlock(IO.DbReader.ReadGenericStringBlock(DbControl.StringPath, 1));

            Console.WriteLine($"Label: \"{l.Data}\", Property: \"{p.Data}\", String: \"{s.Data}\"");

            DbWriter.WritePropertyBlock(new NodePropertyBlock(1, false, PropertyType.Float, 12, new byte[4] { 5, 6, 12, 1 }, 32, 2));
            var np = new NodePropertyBlock(IO.DbReader.ReadPropertyBlock(DbControl.NodePropertyPath, 1));

            DbWriter.WritePropertyBlock(new RelationPropertyBlock(1, true, PropertyType.Bool, 12, new byte[4] { 0, 0, 0, 1 }, 32, 2));
            var ep = new RelationPropertyBlock(IO.DbReader.ReadPropertyBlock(DbControl.RelationPropertyPath, 1));

            Console.WriteLine($"NodeProperty type {np.PropertyType}, {np.PropertyNameId}:{BitConverter.ToSingle(np.Value, 0)}");
            Console.WriteLine($"RelationProperty type {ep.PropertyType}, {ep.PropertyNameId}:{BitConverter.ToBoolean(ep.Value, 3)}");


            Console.ReadLine();
            DbControl.DeleteDbFiles();
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
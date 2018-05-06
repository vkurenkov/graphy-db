using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GraphyDb.IO
{
    public static class DbControl
    {
        internal static readonly TraceSource TraceSource = new TraceSource("TraceGraphyDb");

        internal const string NodePath = "node.storage.db";
        internal const string EdgePath = "edge.storage.db";
        internal const string LabelPath = "label.storage.db";
        internal const string NodePropertyPath = "node_property.storage.db";
        internal const string EdgePropertyPath = "edge_property.storage.db";
        internal const string PropertyNamePath = "property_name.storage.db";
        internal const string StringPath = "string.storage.db";
        internal const string ConsisterPath = "consister.log";
        internal const string IdStoragePath = "id.storage";
        internal static readonly string DbPath = ConfigurationManager.AppSettings["dbPath"];

        internal static readonly Dictionary<string, int> BlockByteSize = new Dictionary<string, int>()
        {
            {StringPath, 34},
            {PropertyNamePath, 34},
            {NodePropertyPath, 17},
            {EdgePropertyPath, 17},
            {NodePath, 17},
            {EdgePath, 33},
            {LabelPath, 34},
            {IdStoragePath, 4}
        };

        internal static readonly Dictionary<string, int> IdStoreOrderNumber = new Dictionary<string, int>()
        {
            {StringPath, 1},
            {PropertyNamePath, 2},
            {NodePropertyPath, 3},
            {EdgePropertyPath, 4},
            {NodePath, 5},
            {EdgePath, 6},
            {LabelPath, 0}
        };

        private static FileStream idFileStream;
        internal static readonly Dictionary<string, int> IdStorageDictionary = new Dictionary<string, int>();

        // Paths to storage files
        internal static List<String> DbFilePaths = new List<string>
        {
            NodePath,
            EdgePath,
            LabelPath,
            EdgePropertyPath,
            NodePropertyPath,
            PropertyNamePath,
            StringPath
        };

        /// <summary>
        /// Create storage files if missing
        /// </summary>
        public static void InitializeIO()
        {
            try
            {
                if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);
                DbWriter.InitializeDbWriter();
                DbReader.InitializeDbReader();

                // Create new empty IdStorage if not present with next free id.
                // Else initialize .storage.db -> ID mapping
                if (!File.Exists(Path.Combine(DbPath, IdStoragePath)))
                {
                    idFileStream = new FileStream(Path.Combine(DbPath, IdStoragePath),
                        FileMode.Create,
                        FileAccess.ReadWrite, FileShare.Read);
                    foreach (var filePath in DbFilePaths)
                    {
                        idFileStream.Write(BitConverter.GetBytes(1), 0, 4);
                        IdStorageDictionary[filePath] = 1;
                    }

                    idFileStream.Flush();
                }
                else
                {
                    idFileStream = new FileStream(Path.Combine(DbPath, IdStoragePath),
                        FileMode.Open,
                        FileAccess.ReadWrite, FileShare.Read);

                    foreach (var filePath in DbFilePaths)
                    {
                        var blockNumber = IdStoreOrderNumber[filePath];
                        var storedIdBytes = new byte[4];
                        idFileStream.Seek(blockNumber * 4, SeekOrigin.Begin);
                        idFileStream.Read(storedIdBytes, 0, 4);
                        IdStorageDictionary[filePath] = BitConverter.ToInt32(storedIdBytes, 0);
                        Console.WriteLine($"Last Id for {filePath} is {IdStorageDictionary[filePath]}");
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSource.TraceEvent(TraceEventType.Error, 1,
                    $"Database Initialization Falied: {ex}");
            }
        }

        public static void ShutdownIO()
        {
            DbReader.CloseIOStreams();
            DbWriter.CloseIOStreams();
            idFileStream?.Dispose();
            idFileStream = null;
        }

        public static void DeleteDbFiles()
        {
            ShutdownIO();
            foreach (var filePath in DbFilePaths)
            {
                File.Delete(Path.Combine(DbPath, filePath));
            }

            File.Delete(Path.Combine(DbPath, IdStoragePath));
        }

        public static int AllocateId(string filePath)
        {
            var lastId = IdStorageDictionary[filePath];
            IdStorageDictionary[filePath] += 1;
            idFileStream.Seek(IdStoreOrderNumber[filePath] * 4, SeekOrigin.Begin);
            idFileStream.Write(BitConverter.GetBytes(IdStorageDictionary[filePath]), 0, 4);
            return lastId;
        }

        public static void ConsisterMonitor()
        {
            var th = Thread.CurrentThread;
            th.Name = "Consister";
            while (true)
            {
                //TODO Read file, fetch Entities, process entities
                Thread.Sleep(250);
            }
        }
    }
}
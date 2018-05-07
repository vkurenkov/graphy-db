using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GraphyDb.IO
{
    public class DbControl
    {
        internal readonly TraceSource TraceSource = new TraceSource("TraceGraphyDb");


        internal static readonly string NodePath = "node.storage.db";
        internal static readonly string RelationPath = "relation.storage.db";
        internal static readonly string LabelPath = "label.storage.db";
        internal static readonly string NodePropertyPath = "node_property.storage.db";
        internal static readonly string RelationPropertyPath = "relation_property.storage.db";
        internal static readonly string PropertyNamePath = "property_name.storage.db";

        internal static readonly string StringPath = "string.storage.db";

        internal static readonly string IdStoragePath = "id.storage";
        internal readonly string DbPath = ConfigurationManager.AppSettings["dbPath"];
        private bool initializedIOFlag = false;

        internal readonly Dictionary<string, int> BlockByteSize = new Dictionary<string, int>()
        {
            {StringPath, 34},
            {PropertyNamePath, 34},
            {NodePropertyPath, 17},
            {RelationPropertyPath, 17},
            {NodePath, 17},
            {RelationPath, 33},
            {LabelPath, 34},
            {IdStoragePath, 4}
        };

        internal readonly Dictionary<string, int> IdStoreOrderNumber = new Dictionary<string, int>()
        {
            {StringPath, 1},
            {PropertyNamePath, 2},
            {NodePropertyPath, 3},
            {RelationPropertyPath, 4},
            {NodePath, 5},
            {RelationPath, 6},
            {LabelPath, 0}
        };

        private FileStream idFileStream;

        internal readonly ConcurrentDictionary<string, int> IdStorageDictionary =
            new ConcurrentDictionary<string, int>();

        // Paths to storage files
        public List<String> DbFilePaths = new List<string>
        {
            NodePath,
            RelationPath,
            LabelPath,
            RelationPropertyPath,
            NodePropertyPath,
            PropertyNamePath,
            StringPath
        };

        internal ConcurrentDictionary<string, int> LabelInvertedIndex = new ConcurrentDictionary<string, int>();

        internal ConcurrentDictionary<string, int> PropertyNameInvertedIndex =
            new ConcurrentDictionary<string, int>();

        internal Thread ConsisterThread;

        internal readonly Dictionary<string, FileStream>
            FileStreamDictionary = new Dictionary<string, FileStream>();

        private static DbWriter dbWriter;
        private static DbReader dbReader;

        public DbControl()
        {
            InitializeIO();
        }

        public DbWriter GetDbWriter()
        {
            return dbWriter ?? (dbWriter = new DbWriter(this));
        }

        public DbReader DbReader => dbReader() ?? (dbReader = new DbReader(this));


        /// <summary>
        /// Create storage files if missing
        /// </summary>
        public void InitializeIO()
        {
            if (initializedIOFlag) return;
            try
            {
                if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);

                foreach (var filePath in DbFilePaths)
                {
                    FileStreamDictionary[filePath] = new FileStream(Path.Combine(DbPath, filePath),
                        FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                }

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

                // Initialize Inverted Indexes
                for (var i = 1; i < FetchLastId(LabelPath); ++i)
                {
                    var labelBlock = new LabelBlock(GetdbControl.GetDbReader()().ReadGenericStringBlock(LabelPath, i));
                    LabelInvertedIndex[labelBlock.Data] = labelBlock.Id;
                }

                for (var i = 1; i < FetchLastId(PropertyNamePath); ++i)
                {
                    var propertyNameBlock =
                        new PropertyNameBlock(GetdbControl.GetDbReader()().ReadGenericStringBlock(PropertyNamePath, i));
                    PropertyNameInvertedIndex[propertyNameBlock.Data] = propertyNameBlock.Id;
                }
            }
            catch (Exception ex)
            {
                TraceSource.TraceEvent(TraceEventType.Error, 1,
                    $"Database Initialization Falied: {ex}");
            }
            finally
            {
                ConsisterThread = new Thread(EventualConsister.ConsisterMonitor);
                ConsisterThread.Start();
                initializedIOFlag = true;
            }
        }

        public void ShutdownIO()
        {
            EventualConsister.ChangedEntitiesQueue.Add(new KillConsisterEntity());
            ConsisterThread.Join();

            foreach (var filePath in DbFilePaths)
            {
                FileStreamDictionary?[filePath].Dispose();
                FileStreamDictionary[filePath] = null;
            }

            idFileStream?.Dispose();
            idFileStream = null;
            initializedIOFlag = false;
        }

        public void DeleteDbFiles()
        {
            ShutdownIO();
            IdStorageDictionary.Clear();
            PropertyNameInvertedIndex.Clear();
            LabelInvertedIndex.Clear();


            foreach (var filePath in DbFilePaths)
            {
                File.Delete(Path.Combine(DbPath, filePath));
            }

            File.Delete(Path.Combine(DbPath, IdStoragePath));
        }

        public int AllocateId(string filePath)
        {
            var lastId = IdStorageDictionary[filePath];
            IdStorageDictionary[filePath] += 1;
            idFileStream.Seek(IdStoreOrderNumber[filePath] * 4, SeekOrigin.Begin);
            idFileStream.Write(BitConverter.GetBytes(IdStorageDictionary[filePath]), 0, 4);
            return lastId;
        }

        public int FetchLastId(string filePath)
        {
            return IdStorageDictionary[filePath];
        }

        public int FetchLabelId(string label)
        {
            LabelInvertedIndex.TryGetValue(label, out var labelId);
            if (labelId != 0) return labelId;
            var newLabelId = AllocateId(LabelPath);
            LabelInvertedIndex[label] = newLabelId;
            GetDbWriter().WriteStringBlock(new LabelBlock(true, label, newLabelId));
            return newLabelId;
        }

        public int FetchPropertyNameId(string propertyName)
        {
            PropertyNameInvertedIndex.TryGetValue(propertyName, out var propertyId);
            if (propertyId != 0) return propertyId;
            var newPropertyNameId = AllocateId(PropertyNamePath);
            GetDbWriter().WriteStringBlock(new PropertyNameBlock(true, propertyName, newPropertyNameId));
            PropertyNameInvertedIndex[propertyName] = newPropertyNameId;
            return newPropertyNameId;
        }
    }
}
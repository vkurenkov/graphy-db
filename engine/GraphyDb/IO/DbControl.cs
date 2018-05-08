using System;
using System.Collections.Concurrent;
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
        internal const string RelationPath = "relation.storage.db";
        internal const string LabelPath = "label.storage.db";
        internal const string NodePropertyPath = "node_property.storage.db";
        internal const string RelationPropertyPath = "relation_property.storage.db";
        internal const string PropertyNamePath = "property_name.storage.db";

        internal const string StringPath = "string.storage.db";

//        internal const string ConsisterPath = "consister.log";
        internal const string IdStoragePath = "id.storage";
        internal static readonly string DbPath = ConfigurationManager.AppSettings["dbPath"];
        private static bool initializedIOFlag = false;

        internal static readonly Dictionary<string, int> BlockByteSize = new Dictionary<string, int>()
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

        internal static readonly Dictionary<string, int> IdStoreOrderNumber = new Dictionary<string, int>()
        {
            {StringPath, 1},
            {PropertyNamePath, 2},
            {NodePropertyPath, 3},
            {RelationPropertyPath, 4},
            {NodePath, 5},
            {RelationPath, 6},
            {LabelPath, 0}
        };

        private static FileStream idFileStream;

        internal static readonly ConcurrentDictionary<string, int> IdStorageDictionary =
            new ConcurrentDictionary<string, int>();

        // Paths to storage files
        internal static List<String> DbFilePaths = new List<string>
        {
            NodePath,
            RelationPath,
            LabelPath,
            RelationPropertyPath,
            NodePropertyPath,
            PropertyNamePath,
            StringPath
        };

        internal static ConcurrentDictionary<string, int> LabelInvertedIndex = new ConcurrentDictionary<string, int>();

        internal static ConcurrentDictionary<string, int> PropertyNameInvertedIndex =
            new ConcurrentDictionary<string, int>();

        internal static Thread ConsisterThread;

        internal static readonly Dictionary<string, FileStream>
            FileStreamDictionary = new Dictionary<string, FileStream>();

        /// <summary>
        /// Create storage files if missing
        /// </summary>
        public static void InitializeIO()
        {
            if (initializedIOFlag) return;
            try
            {
                if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);

                foreach (var filePath in DbControl.DbFilePaths)
                {
                    FileStreamDictionary[filePath] = new FileStream(Path.Combine(DbControl.DbPath, filePath),
                        FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 5 * 1024 * 1024);
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
//                        Console.WriteLine($"Last Id for {filePath} is {IdStorageDictionary[filePath]}");
                    }
                }

                // Initialize Inverted Indexes
                for (var i = 1; i < FetchLastId(LabelPath); ++i)
                {
                    var labelBlock = new LabelBlock(DbReader.ReadGenericStringBlock(LabelPath, i));
                    LabelInvertedIndex[labelBlock.Data] = labelBlock.Id;
                }

                for (var i = 1; i < FetchLastId(PropertyNamePath); ++i)
                {
                    var propertyNameBlock =
                        new PropertyNameBlock(DbReader.ReadGenericStringBlock(PropertyNamePath, i));
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

        public static void ShutdownIO()
        {
            EventualConsister.ChangedEntitiesQueue.Add(new KillConsisterEntity());
            ConsisterThread.Join();

            IdStorageDictionary.Clear();
            PropertyNameInvertedIndex.Clear();
            LabelInvertedIndex.Clear();

            foreach (var filePath in DbControl.DbFilePaths)
            {
                FileStreamDictionary?[filePath]?.Dispose();
                FileStreamDictionary[filePath] = null;
            }


            idFileStream?.Dispose();
            idFileStream = null;
            initializedIOFlag = false;
        }

        public static void DeleteDbFiles()
        {
            ShutdownIO();            
            Directory.Delete(DbPath, true);
        }

        public static int AllocateId(string filePath)
        {
            var lastId = IdStorageDictionary[filePath];
            IdStorageDictionary[filePath] += 1;
            idFileStream.Seek(IdStoreOrderNumber[filePath] * 4, SeekOrigin.Begin);
            idFileStream.Write(BitConverter.GetBytes(IdStorageDictionary[filePath]), 0, 4);
            return lastId;
        }

        public static int FetchLastId(string filePath)
        {
            return IdStorageDictionary[filePath];
        }

        public static int FetchLabelId(string label)
        {
            LabelInvertedIndex.TryGetValue(label, out var labelId);
            if (labelId != 0) return labelId;
            var newLabelId = AllocateId(LabelPath);
            LabelInvertedIndex[label] = newLabelId;
            DbWriter.WriteStringBlock(new LabelBlock(true, label, newLabelId));
            return newLabelId;
        }

        public static int FetchPropertyNameId(string propertyName)
        {
            PropertyNameInvertedIndex.TryGetValue(propertyName, out var propertyId);
            if (propertyId != 0) return propertyId;
            var newPropertyNameId = AllocateId(PropertyNamePath);
            DbWriter.WriteStringBlock(new PropertyNameBlock(true, propertyName, newPropertyNameId));
            PropertyNameInvertedIndex[propertyName] = newPropertyNameId;
            return newPropertyNameId;
        }
    }
}
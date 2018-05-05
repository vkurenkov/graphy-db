using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;

namespace GraphyDb.IO
{
    public static class DbWriter
    {
        private static readonly TraceSource TraceSource = new TraceSource("TraceGraphyDb");

        private const string NodePath = "node.storage.db";
        private const string EdgePath = "edge.storage.db";
        private const string LabelPath = "label.storage.db";
        private const string PropertyPath = "property.storage.db";
        private const string PropertyNamePath = "property_name.storage.db";
        private const string StringPath = "string.storage.db";
        private const string ConsisterPath = "consister.log";
        private const string IdStoragePath = "id.storage.json";

        private static readonly string DbPath = ConfigurationManager.AppSettings["dbPath"];

        private static readonly Dictionary<string, int> BlockByteSize = new Dictionary<string, int>()
        {
            {StringPath, 66},
            {PropertyNamePath, 66},
            {PropertyPath, 13},
            {NodePath, 17},
            {EdgePath, 33},
            {LabelPath, 66},
            {IdStoragePath, 4}
        };

        private static readonly Dictionary<string, int> IdStoreOrderNumber = new Dictionary<string, int>()
        {
            {StringPath, 1},
            {PropertyNamePath, 2},
            {PropertyPath, 3},
            {NodePath, 4},
            {EdgePath, 5},
            {LabelPath, 0}
        };


        private static Dictionary<string, FileStream> filePoolDictionary = new Dictionary<string, FileStream>();
        private static Dictionary<string, int> idStorageDictionary = new Dictionary<string, int>();

        /// <summary>
        /// Create storage files if missing
        /// </summary>
        public static void InitializeFiles()
        {
            var dbFilePaths = new List<string>
            {
                NodePath,
                EdgePath,
                LabelPath,
                PropertyPath,
                PropertyNamePath,
                StringPath
            };

            try
            {
                foreach (string filePath in dbFilePaths)
                {
                    filePoolDictionary[filePath] = new FileStream(Path.Combine(DbPath, filePath), FileMode.OpenOrCreate,
                        FileAccess.ReadWrite, FileShare.Read);
                }

                // Create new empty IdStorage if not present with next free id.
                if (!File.Exists(Path.Combine(DbPath, IdStoragePath)))
                {
                    filePoolDictionary[IdStoragePath] = new FileStream(Path.Combine(DbPath, IdStoragePath), FileMode.Create,
                        FileAccess.ReadWrite, FileShare.Read);
                    foreach (var file in dbFilePaths)
                    {
                        filePoolDictionary[IdStoragePath].Write(BitConverter.GetBytes(1), 0, 4);
                    }
                    filePoolDictionary[IdStoragePath].Flush();
                }
                else
                {
                    filePoolDictionary[IdStoragePath] = new FileStream(Path.Combine(DbPath, IdStoragePath), FileMode.Open,
                        FileAccess.ReadWrite, FileShare.Read);
                }


                // Initialize .storage.db -> ID mapping
                foreach (string filePath in dbFilePaths)
                {
                    var blockNumber = IdStoreOrderNumber[filePath];
                    var storedIdBytes = new byte[4];
                    ReadBlock(IdStoragePath, blockNumber, storedIdBytes);
                    var lastId = BitConverter.ToInt32(storedIdBytes, 0);
                    Console.WriteLine($"Last Id for {filePath} is {lastId}");
                    idStorageDictionary[filePath] = lastId;
                }
            }
            catch (Exception ex)
            {
                TraceSource.TraceEvent(TraceEventType.Error, 1,
                    $"Database Initialization Falied: {ex}");
            }
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


        /// <summary>
        /// Read specific block from file
        /// </summary>
        /// <param name="filePath">Path to the file with byte-record structure</param>
        /// <param name="blockNumber">Block position from the beggining of the file</param>
        /// <param name="block"> Buffer to which result is written</param>
        private static void ReadBlock(string filePath, int blockNumber, byte[] block)
        {
            int offset = blockNumber * BlockByteSize[filePath];
            filePoolDictionary[filePath].Seek(offset, SeekOrigin.Begin);
            filePoolDictionary[filePath].Read(block, 0, BlockByteSize[filePath]);
        }

        /// <summary>
        /// Write byte block from given byte array
        /// </summary>
        /// <param name="filePath">Path to the file with byte-record structure.</param>
        /// <param name="blockNumber"> Position of block in the file.</param>
        /// <param name="block">buffer from which we write to the file.</param>
        private static void WriteBlock(string filePath, int blockNumber, byte[] block)
        {
            int offset = blockNumber * BlockByteSize[filePath];
            filePoolDictionary[filePath].Seek(offset, SeekOrigin.Begin);
            filePoolDictionary[filePath].Write(block, 0, BlockByteSize[filePath]); //Maybe WriteAsync?
        }
    }
}
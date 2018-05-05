using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GraphyDb.IO
{
    public static class DbWriter
    {
        private static readonly TraceSource TraceSource = new TraceSource("TraceGraphyDb");

        private const string NodePath = "node.storage.db";
        private const string EdgePath = "edge.storage.db";
        private const string LabelPath = "labe.storage.db";
        private const string PropertyPath = "property.storage.db";
        private const string PropertyNamePath = "property_name.storage.db";
        private const string StringPath = "string.storage.db";
        private const string ConsisterPath = "consister.log";
        private const string IdStoragePath = "id.storage.json";

        private static readonly string DbPath = ConfigurationManager.AppSettings["dbPath"];
        private static readonly Dictionary<string, int> BlockByteSize = new Dictionary<string, int>(){
            {StringPath, 66}, {PropertyNamePath, 66}, {PropertyPath, 13},
            {NodePath, 17}, {EdgePath, 33}, {LabelPath, 66}
        };


        private static Dictionary<string, FileStream> filePool = new Dictionary<string, FileStream>();
        private static Dictionary<string, int> fetchId = new Dictionary<string, int>();

        /// <summary>
        /// Create storage files if missing
        /// </summary>
        public static void InitializeFiles()
        {
            List<string> dbFilePaths = new List<string> {NodePath, EdgePath, LabelPath, PropertyPath,
                                              PropertyNamePath, StringPath, ConsisterPath, IdStoragePath};

            try
            {
                foreach (string filePath in dbFilePaths)
                {
                    if (!File.Exists(Path.Combine(DbPath, filePath))) File.Create(Path.Combine(DbPath, filePath));
                    filePool[filePath] = new FileStream(Path.Combine(DbPath, filePath), FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                   
                }
            }
            catch (Exception ex) {
                TraceSource.TraceEvent(TraceEventType.Error, 1,
                    $"File Init Falied: {ex}");
            }
        }

        
        public static void ConsisterMonitor(){
            var th = Thread.CurrentThread;
            th.Name = "Consister";
            while(true){
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
        private static  void ReadBlock(string filePath, int blockNumber, byte[] block)
        {
            int offset = blockNumber * BlockByteSize[filePath];
            filePool[filePath].Seek(offset, SeekOrigin.Begin);
            filePool[filePath].Read(block, 0, BlockByteSize[filePath]);
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
            filePool[filePath].Seek(offset, SeekOrigin.Begin);
            filePool[filePath].Write(block, 0, BlockByteSize[filePath]); //Maybe WriteAsync?
        }
    }
}



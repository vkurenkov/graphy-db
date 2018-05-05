using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;
using System.Net.Mime;
using System.Text;

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
        private const string IdStoragePath = "id.storage";
        private static readonly string DbPath = ConfigurationManager.AppSettings["dbPath"];
        private static readonly Dictionary<string, int> BlockByteSize = new Dictionary<string, int>()
        {
            {StringPath, 66},
            {PropertyNamePath, 66},
            {PropertyPath, 13},
            {NodePath, 13},
            {EdgePath, 33},
            {LabelPath, 34},
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
        private static readonly Dictionary<string, FileStream> FilePoolDictionary = new Dictionary<string, FileStream>();
        private static readonly Dictionary<string, int> IdStorageDictionary = new Dictionary<string, int>();

        /// <summary>
        /// Create storage files if missing
        /// </summary>
        public static void InitializeDatabase()
        {   
            // Paths to storage files
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
                // Create FileStream associated with each file
                foreach (string filePath in dbFilePaths)
                {
                    FilePoolDictionary[filePath] = new FileStream(Path.Combine(DbPath, filePath), FileMode.OpenOrCreate,
                        FileAccess.ReadWrite, FileShare.Read);
                }

                // Create new empty IdStorage if not present with next free id.
                // Else initialize .storage.db -> ID mapping
                if (!File.Exists(Path.Combine(DbPath, IdStoragePath)))
                {
                    FilePoolDictionary[IdStoragePath] = new FileStream(Path.Combine(DbPath, IdStoragePath), FileMode.Create,
                        FileAccess.ReadWrite, FileShare.Read);
                    foreach (var filePath in dbFilePaths)
                    {
                        FilePoolDictionary[IdStoragePath].Write(BitConverter.GetBytes(1), 0, 4);
                        IdStorageDictionary[filePath] = 1;
                    }
                    FilePoolDictionary[IdStoragePath].Flush();
                }
                else
                {   
                    FilePoolDictionary[IdStoragePath] = new FileStream(Path.Combine(DbPath, IdStoragePath), FileMode.Open,
                        FileAccess.ReadWrite, FileShare.Read);
                    foreach (var filePath in dbFilePaths)
                    {
                        var blockNumber = IdStoreOrderNumber[filePath];
                        var storedIdBytes = new byte[4];
                        ReadBlock(IdStoragePath, blockNumber, storedIdBytes);
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


        private static int ConvertBitArrayToInt(BitArray bitArray)
        {
            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");
            int[] tempArray = new int[1];
            bitArray.CopyTo(tempArray, 0);
            return tempArray[0];
        }

        public static void WriteNodeBlock(IO.NodeBlock e)
        {
            var buffer = new byte[BlockByteSize[NodePath]];
            Array.Copy(BitConverter.GetBytes(e.Used), buffer, 1);
            Array.Copy(BitConverter.GetBytes(e.NextRelationId), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(e.NextPropertyId), 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(e.LabelId), 0, buffer, 9, 4);
            WriteBlock(NodePath, e.NodeId, buffer);
        }
        public static IO.NodeBlock ReadNodeBlock(int nodeId)
        {
            var buffer = new byte[BlockByteSize[NodePath]];
            ReadBlock(NodePath, nodeId, buffer);
            var used = BitConverter.ToBoolean(buffer, 0);
            var nextRelationId = BitConverter.ToInt32(buffer.Skip(1).Take(4).ToArray(), 0);
            var nextPropertyId = BitConverter.ToInt32(buffer.Skip(5).Take(4).ToArray(), 0);
            var labelId = BitConverter.ToInt32(buffer.Skip(9).Take(4).ToArray(), 0);
            return new NodeBlock(used, nodeId, nextRelationId, nextPropertyId, labelId);
        }

        public static void WriteEdgeBlock(IO.EdgeBlock e)
        {
            var buffer = new byte[BlockByteSize[EdgePath]];
            Array.Copy(BitConverter.GetBytes(e.Used), buffer, 1);
            Array.Copy(BitConverter.GetBytes(e.FirstNode), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(e.SecondNode), 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstNodePreviousRelation), 0, buffer, 9, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstNodeNextRelation), 0, buffer, 13, 4);
            Array.Copy(BitConverter.GetBytes(e.SecondNodePreviousRelation), 0, buffer, 17, 4);
            Array.Copy(BitConverter.GetBytes(e.SecondNodeNextRelation), 0, buffer, 21, 4);
            Array.Copy(BitConverter.GetBytes(e.NextProperty), 0, buffer, 25, 4);
            Array.Copy(BitConverter.GetBytes(e.LabelId), 0, buffer, 29, 4);
            WriteBlock(EdgePath, e.EdgeId, buffer);
        }
        public static IO.EdgeBlock ReadEdgeBlock(int edgeId)
        {
            var buffer = new byte[BlockByteSize[EdgePath]];
            ReadBlock(EdgePath, edgeId, buffer);
            IO.EdgeBlock e = new EdgeBlock
            {
                Used = BitConverter.ToBoolean(buffer, 0),
                FirstNode = BitConverter.ToInt32(buffer.Skip(1).Take(4).ToArray(), 0),
                SecondNode = BitConverter.ToInt32(buffer.Skip(5).Take(4).ToArray(), 0),
                FirstNodePreviousRelation = BitConverter.ToInt32(buffer.Skip(9).Take(4).ToArray(), 0),
                FirstNodeNextRelation = BitConverter.ToInt32(buffer.Skip(13).Take(4).ToArray(), 0),
                SecondNodePreviousRelation = BitConverter.ToInt32(buffer.Skip(17).Take(4).ToArray(), 0),
                SecondNodeNextRelation = BitConverter.ToInt32(buffer.Skip(21).Take(4).ToArray(), 0),
                NextProperty = BitConverter.ToInt32(buffer.Skip(25).Take(4).ToArray(), 0),
                LabelId = BitConverter.ToInt32(buffer.Skip(29).Take(4).ToArray(), 0)
            };
            return e;
        }

        public static IO.LabelBlock ReadLabelBlock(int labelId)
        {
            var buffer = new byte[BlockByteSize[LabelPath]];
            ReadBlock(LabelPath, labelId, buffer);
            var used = BitConverter.ToBoolean(buffer, 0);
            var bitsUsed = buffer[1];
            var labelName = System.Text.Encoding.UTF8.GetString(buffer.Skip(2).Take(bitsUsed).ToArray());
            return new LabelBlock(used, labelName, labelId);
        }
        public static void WriteLabelBlock(IO.LabelBlock l)
        {   
            var buffer = new byte[BlockByteSize[LabelPath]];
            Array.Copy(BitConverter.GetBytes(l.Used), buffer, 1);
            var strBytes = Encoding.UTF8.GetBytes(l.Data);
            byte[] truncStrArray = new byte[32];
            var truncationIndex = Math.Min(strBytes.Length, truncStrArray.Length);
            Array.Copy(strBytes, truncStrArray, truncationIndex);
            buffer[1] = (byte) strBytes.Length;
            Array.Copy(truncStrArray, 0, buffer, 2, truncationIndex);
            WriteBlock(LabelPath, l.LabelId, buffer);
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
            FilePoolDictionary[filePath].Seek(offset, SeekOrigin.Begin);
            FilePoolDictionary[filePath].Read(block, 0, BlockByteSize[filePath]);
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
            FilePoolDictionary[filePath].Seek(offset, SeekOrigin.Begin);
            FilePoolDictionary[filePath].Write(block, 0, BlockByteSize[filePath]); //Maybe WriteAsync?
            FilePoolDictionary[filePath].Flush();
        }
    }


}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GraphyDb.IO
{
    internal static class DbWriter
    {
        private static readonly Dictionary<string, FileStream>
            WriteFileStreamDictionary = new Dictionary<string, FileStream>();

        internal static void InitializeDbWriter()
        {
            foreach (var filePath in DbControl.DbFilePaths)
            {
                WriteFileStreamDictionary[filePath] = new FileStream(Path.Combine(DbControl.DbPath, filePath), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            }
        }

        public static void WriteNodeBlock(GraphyDb.IO.NodeBlock e)
        {
            var buffer = new byte[DbControl.BlockByteSize[DbControl.NodePath]];
            Array.Copy(BitConverter.GetBytes(e.Used), buffer, 1);
            Array.Copy(BitConverter.GetBytes(e.FirstInRelationId), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstOutRelationId), 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(e.NextPropertyId), 0, buffer, 9, 4);
            Array.Copy(BitConverter.GetBytes(e.LabelId), 0, buffer, 13, 4);
            WriteBlock(DbControl.NodePath, e.NodeId, buffer);
        }

        public static void WriteEdgeBlock(GraphyDb.IO.EdgeBlock e)
        {
            var buffer = new byte[DbControl.BlockByteSize[DbControl.EdgePath]];
            Array.Copy(BitConverter.GetBytes(e.Used), buffer, 1);
            Array.Copy(BitConverter.GetBytes(e.FirstNode), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(e.SecondNode), 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstNodePreviousRelation), 0, buffer, 9, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstNodeNextRelation), 0, buffer, 13, 4);
            Array.Copy(BitConverter.GetBytes(e.SecondNodePreviousRelation), 0, buffer, 17, 4);
            Array.Copy(BitConverter.GetBytes(e.SecondNodeNextRelation), 0, buffer, 21, 4);
            Array.Copy(BitConverter.GetBytes(e.NextProperty), 0, buffer, 25, 4);
            Array.Copy(BitConverter.GetBytes(e.LabelId), 0, buffer, 29, 4);
            WriteBlock(DbControl.EdgePath, e.EdgeId, buffer);
        }

        public static void WriteGenericStringBlock(GraphyDb.IO.GenericStringBlock s)
        {
            var buffer = new byte[DbControl.BlockByteSize[s.StoragePath]];
            Array.Copy(BitConverter.GetBytes(s.Used), buffer, 1);
            var strBytes = Encoding.UTF8.GetBytes(s.Data);
            var truncStrArray = new byte[32];
            var truncationIndex = Math.Min(strBytes.Length, truncStrArray.Length);
            Array.Copy(strBytes, truncStrArray, truncationIndex);
            buffer[1] = (byte) strBytes.Length;
            Array.Copy(truncStrArray, 0, buffer, 2, truncationIndex);
            WriteBlock(s.StoragePath, s.Id, buffer);
        }

        public static void WritePropertyBlock(GraphyDb.IO.PropertyBlock p)
        {
            var buffer = new byte[DbControl.BlockByteSize[p.StoragePath]];
            buffer[0] = (byte) ((p.Used ? 1 : 0) + ((byte) p.PtType << 1));
            Array.Copy(BitConverter.GetBytes(p.PropertyName), 0, buffer, 1, 4);
            Array.Copy(p.Value, 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(p.NextProperty), 0, buffer, 9, 4);
            Array.Copy(BitConverter.GetBytes(p.NodeId), 0, buffer, 13, 4);
            WriteBlock(p.StoragePath, p.Id, buffer);
        }

        /// <summary>
        /// Write byte block from given byte array
        /// </summary>
        /// <param name="filePath">Path to the file with byte-record structure.</param>
        /// <param name="blockNumber"> Position of block in the file.</param>
        /// <param name="block">buffer from which we write to the file.</param>
        private static void WriteBlock(string filePath, int blockNumber, byte[] block)
        {
            int offset = blockNumber * DbControl.BlockByteSize[filePath];
            WriteFileStreamDictionary[filePath].Seek(offset, SeekOrigin.Begin);
            WriteFileStreamDictionary[filePath].Write(block, 0, DbControl.BlockByteSize[filePath]); //Maybe WriteAsync?
            WriteFileStreamDictionary[filePath].Flush();
        }

        public static void CloseIOStreams()
        {
            foreach (var filePath in DbControl.DbFilePaths)
            {
                WriteFileStreamDictionary?[filePath].Dispose();
                WriteFileStreamDictionary[filePath] = null;
            }
        }
    }
}
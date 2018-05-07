using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GraphyDb.IO
{
    internal static class DbWriter
    {
//        private static readonly Dictionary<string, FileStream>
//            WriteFileStreamDictionary = new Dictionary<string, FileStream>();
//
//        internal static void InitializeDbWriter()
//        {
//            foreach (var filePath in DbControl.DbFilePaths)
//            {
//                WriteFileStreamDictionary[filePath] = new FileStream(Path.Combine(DbControl.DbPath, filePath),
//                    FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
//            }
//        }


        public static void InvalidateBlock(string filePath, int id)
        {
            DbControl.FileStreamDictionary[filePath].Seek(id * DbControl.BlockByteSize[filePath], SeekOrigin.Begin);
            var changedFirstByte = (byte) (DbControl.FileStreamDictionary[filePath].ReadByte() & 254);
            DbControl.FileStreamDictionary[filePath]
                .Seek(id * DbControl.BlockByteSize[filePath], SeekOrigin.Begin); //Cause ReadByte advances
            DbControl.FileStreamDictionary[filePath].WriteByte(changedFirstByte);
            DbControl.FileStreamDictionary[filePath].Flush();
        }

        public static void WriteNodeBlock(NodeBlock e)
        {
            var buffer = new byte[DbControl.BlockByteSize[DbControl.NodePath]];
            Array.Copy(BitConverter.GetBytes(e.Used), buffer, 1);
            Array.Copy(BitConverter.GetBytes(e.FirstInRelationId), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstOutRelationId), 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstPropertyId), 0, buffer, 9, 4);
            Array.Copy(BitConverter.GetBytes(e.LabelId), 0, buffer, 13, 4);
            WriteBlock(DbControl.NodePath, e.NodeId, buffer);
        }

        public static void WriteRelationBlock(RelationBlock e)
        {
            var buffer = new byte[DbControl.BlockByteSize[DbControl.RelationPath]];
            Array.Copy(BitConverter.GetBytes(e.Used), buffer, 1);
            Array.Copy(BitConverter.GetBytes(e.FirstNodeId), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(e.SecondNodeId), 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstNodePreviousRelationId), 0, buffer, 9, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstNodeNextRelation), 0, buffer, 13, 4);
            Array.Copy(BitConverter.GetBytes(e.SecondNodePreviousRelationId), 0, buffer, 17, 4);
            Array.Copy(BitConverter.GetBytes(e.SecondNodeNextRelation), 0, buffer, 21, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstPropertyId), 0, buffer, 25, 4);
            Array.Copy(BitConverter.GetBytes(e.LabelId), 0, buffer, 29, 4);
            WriteBlock(DbControl.RelationPath, e.RelationId, buffer);
        }


        public static void WriteStringBlock(GenericStringBlock s)
        {
            string storagePath;
            switch (s)
            {
                case LabelBlock _:
                    storagePath = DbControl.LabelPath;
                    break;
                case StringBlock _:
                    storagePath = DbControl.StringPath;
                    break;
                case PropertyNameBlock _:
                    storagePath = DbControl.PropertyNamePath;
                    break;
                default:
                    throw new NotSupportedException("Unsupported string-like block type.");
            }

            var buffer = new byte[DbControl.BlockByteSize[storagePath]];
            Array.Copy(BitConverter.GetBytes(s.Used), buffer, 1);
            var strBytes = Encoding.UTF8.GetBytes(s.Data);
            var truncStrArray = new byte[32];
            var truncationIndex = Math.Min(strBytes.Length, truncStrArray.Length);
            Array.Copy(strBytes, truncStrArray, truncationIndex);
            buffer[1] = (byte) strBytes.Length;
            Array.Copy(truncStrArray, 0, buffer, 2, truncationIndex);
            WriteBlock(storagePath, s.Id, buffer);
        }

        public static void WritePropertyBlock(PropertyBlock p)
        {
            string storagePath;
            switch (p)
            {
                case NodePropertyBlock _:
                    storagePath = DbControl.NodePropertyPath;
                    break;
                case RelationPropertyBlock _:
                    storagePath = DbControl.RelationPropertyPath;
                    break;
                default:
                    throw new NotSupportedException("Unsupported property-like block type.");
            }

            var buffer = new byte[DbControl.BlockByteSize[storagePath]];
            buffer[0] = (byte) ((p.Used ? 1 : 0) + ((byte) p.PropertyType << 1));
            Array.Copy(BitConverter.GetBytes(p.PropertyNameId), 0, buffer, 1, 4);
            Array.Copy(p.Value, 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(p.NextPropertyId), 0, buffer, 9, 4);
            Array.Copy(BitConverter.GetBytes(p.NodeId), 0, buffer, 13, 4);
            WriteBlock(storagePath, p.PropertyId, buffer);
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
//            DbControl.FileStreamDictionary[filePath].Lock(0, DbControl.FileStreamDictionary[filePath].Length);
            DbControl.FileStreamDictionary[filePath].Seek(offset, SeekOrigin.Begin);
            DbControl.FileStreamDictionary[filePath]
                .Write(block, 0, DbControl.BlockByteSize[filePath]); //Maybe WriteAsync?
            DbControl.FileStreamDictionary[filePath].Flush();
//            DbControl.FileStreamDictionary[filePath].Unlock(0, DbControl.FileStreamDictionary[filePath].Length);        
        }

        //        public static void CloseIOStreams()
        //        {
        //            foreach (var filePath in DbControl.DbFilePaths)
        //            {
        //                WriteFileStreamDictionary?[filePath].Dispose();
        //                WriteFileStreamDictionary[filePath] = null;
        //            }
        //        }
    }
}
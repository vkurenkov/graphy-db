using System;
using System.IO;
using System.Text;

namespace GraphyDb.IO
{
    public class DbWriter

    {
        private readonly DbControl dbControl;

        public DbWriter(DbControl dbControl)
        {
            this.dbControl = dbControl;
        }


        public void InvalidateBlock(string filePath, int id)
        {
            dbControl.FileStreamDictionary[filePath].Seek(id * dbControl.BlockByteSize[filePath], SeekOrigin.Begin);
            var changedFirstByte = (byte) (dbControl.FileStreamDictionary[filePath].ReadByte() & 254);
            dbControl.FileStreamDictionary[filePath]
                .Seek(id * dbControl.BlockByteSize[filePath], SeekOrigin.Begin); //Cause ReadByte advances
            dbControl.FileStreamDictionary[filePath].WriteByte(changedFirstByte);
            dbControl.FileStreamDictionary[filePath].Flush();
        }

        public void WriteNodeBlock(NodeBlock e)
        {
            var buffer = new byte[dbControl.BlockByteSize[DbControl.NodePath]];
            Array.Copy(BitConverter.GetBytes(e.Used), buffer, 1);
            Array.Copy(BitConverter.GetBytes(e.FirstInRelationId), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstOutRelationId), 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(e.FirstPropertyId), 0, buffer, 9, 4);
            Array.Copy(BitConverter.GetBytes(e.LabelId), 0, buffer, 13, 4);
            WriteBlock(dbControl.DbPath, e.NodeId, buffer);
        }

        public void WriteRelationBlock(RelationBlock e)
        {
            var buffer = new byte[dbControl.BlockByteSize[DbControl.RelationPath]];
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


        public void WriteStringBlock(GenericStringBlock s)
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

            var buffer = new byte[dbControl.BlockByteSize[storagePath]];
            Array.Copy(BitConverter.GetBytes(s.Used), buffer, 1);
            var strBytes = Encoding.UTF8.GetBytes(s.Data);
            var truncStrArray = new byte[32];
            var truncationIndex = Math.Min(strBytes.Length, truncStrArray.Length);
            Array.Copy(strBytes, truncStrArray, truncationIndex);
            buffer[1] = (byte) strBytes.Length;
            Array.Copy(truncStrArray, 0, buffer, 2, truncationIndex);
            WriteBlock(storagePath, s.Id, buffer);
        }

        public void WritePropertyBlock(PropertyBlock p)
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

            var buffer = new byte[dbControl.BlockByteSize[storagePath]];
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
        private void WriteBlock(string filePath, int blockNumber, byte[] block)
        {
            int offset = blockNumber * dbControl.BlockByteSize[filePath];
            dbControl.FileStreamDictionary[filePath].Seek(offset, SeekOrigin.Begin);
            dbControl.FileStreamDictionary[filePath]
                .WriteAsync(block, 0, dbControl.BlockByteSize[filePath]); //Maybe WriteAsync?
            dbControl.FileStreamDictionary[filePath].FlushAsync();
        }
    }
}
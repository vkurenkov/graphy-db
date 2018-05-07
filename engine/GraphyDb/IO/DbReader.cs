using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GraphyDb.IO
{
    public class DbReader
    {
        private readonly DbControl dbControl;

        public DbReader(DbControl dbControl)
        {
            this.dbControl = dbControl;
        }

        public NodeBlock ReadNodeBlock(int nodeId)
        {
            var buffer = new byte[dbControl.BlockByteSize[DbControl.NodePath]];
            ReadBlock(DbControl.NodePath, nodeId, buffer);
            var used = BitConverter.ToBoolean(buffer, 0);
            var firstInRelationId = BitConverter.ToInt32(buffer.Skip(1).Take(4).ToArray(), 0);
            var firstOutRelationId = BitConverter.ToInt32(buffer.Skip(5).Take(4).ToArray(), 0);
            var nextPropertyId = BitConverter.ToInt32(buffer.Skip(9).Take(4).ToArray(), 0);
            var labelId = BitConverter.ToInt32(buffer.Skip(13).Take(4).ToArray(), 0);
            return new NodeBlock(used, nodeId, firstInRelationId, firstOutRelationId, nextPropertyId, labelId);
        }

        public RelationBlock ReadRelationBlock(int relationId)
        {
            var buffer = new byte[dbControl.BlockByteSize[DbControl.RelationPath]];
            ReadBlock(DbControl.RelationPath, relationId, buffer);
            RelationBlock e = new RelationBlock
            {
                Used = BitConverter.ToBoolean(buffer, 0),
                FirstNodeId = BitConverter.ToInt32(buffer.Skip(1).Take(4).ToArray(), 0),
                SecondNodeId = BitConverter.ToInt32(buffer.Skip(5).Take(4).ToArray(), 0),
                FirstNodePreviousRelationId = BitConverter.ToInt32(buffer.Skip(9).Take(4).ToArray(), 0),
                FirstNodeNextRelation = BitConverter.ToInt32(buffer.Skip(13).Take(4).ToArray(), 0),
                SecondNodePreviousRelationId = BitConverter.ToInt32(buffer.Skip(17).Take(4).ToArray(), 0),
                SecondNodeNextRelation = BitConverter.ToInt32(buffer.Skip(21).Take(4).ToArray(), 0),
                FirstPropertyId = BitConverter.ToInt32(buffer.Skip(25).Take(4).ToArray(), 0),
                LabelId = BitConverter.ToInt32(buffer.Skip(29).Take(4).ToArray(), 0),
                RelationId = relationId
            };
            return e;
        }

        public GenericStringBlock ReadGenericStringBlock(string storagePath, int id)
        {
            var buffer = new byte[dbControl.BlockByteSize[storagePath]];
            ReadBlock(storagePath, id, buffer);
            var used = BitConverter.ToBoolean(buffer, 0);
            var bitsUsed = buffer[1];
            var text = Encoding.UTF8.GetString(buffer.Skip(2).Take(bitsUsed).ToArray());
            if (storagePath == DbControl.LabelPath) return new LabelBlock(used, text, id);
            if (storagePath == DbControl.StringPath) return new StringBlock(used, text, id);
            if (storagePath == DbControl.PropertyNamePath) return new PropertyNameBlock(used, text, id);
            throw new ArgumentException("Storage path is invalid.");
        }

        public PropertyBlock ReadPropertyBlock(string storagePath, int id)
        {
            var buffer = new byte[dbControl.BlockByteSize[storagePath]];
            ReadBlock(storagePath, id, buffer);
            var used = buffer[0] % 2 == 1;
            var dtype = (PropertyType) (buffer[0] >> 1);
            var propertyName = BitConverter.ToInt32(buffer.Skip(1).Take(4).ToArray(), 0);
            var propertyValue = buffer.Skip(5).Take(4).ToArray();
            var nextProperty = BitConverter.ToInt32(buffer.Skip(9).Take(4).ToArray(), 0);
            var parentId = BitConverter.ToInt32(buffer.Skip(13).Take(4).ToArray(), 0);
            if (storagePath == DbControl.NodePropertyPath)
                return new NodePropertyBlock(id, used, dtype, propertyName, propertyValue, nextProperty, parentId);
            if (storagePath == DbControl.RelationPropertyPath)
                return new RelationPropertyBlock(id, used, dtype, propertyName, propertyValue, nextProperty,
                    parentId);
            throw new ArgumentException("Storage path is invalid.");
        }

        /// <summary>
        /// Read specific block from file
        /// </summary>
        /// <param name="filePath">Path to the file with byte-record structure</param>
        /// <param name="blockNumber">Block position from the beggining of the file</param>
        /// <param name="block"> Buffer to which result is written</param>
        public void ReadBlock(string filePath, int blockNumber, byte[] block)
        {
            var offset = blockNumber * dbControl.BlockByteSize[filePath];
            dbControl.FileStreamDictionary[filePath].Seek(offset, SeekOrigin.Begin);
            dbControl.FileStreamDictionary[filePath].Read(block, 0, dbControl.BlockByteSize[filePath]);
        }
    }
}
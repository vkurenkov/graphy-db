using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphyDb.IO
{
    internal static class DbReader
    {
        public static IO.NodeBlock ReadNodeBlock(int nodeId)
        {
            var buffer = new byte[DbControl.BlockByteSize[DbControl.NodePath]];
            ReadBlock(DbControl.NodePath, nodeId, buffer);
            var used = BitConverter.ToBoolean(buffer, 0);
            var firstInRelationId = BitConverter.ToInt32(buffer.Skip(1).Take(4).ToArray(), 0);
            var firstOutRelationId = BitConverter.ToInt32(buffer.Skip(5).Take(4).ToArray(), 0);
            var nextPropertyId = BitConverter.ToInt32(buffer.Skip(9).Take(4).ToArray(), 0);
            var labelId = BitConverter.ToInt32(buffer.Skip(13).Take(4).ToArray(), 0);
            return new NodeBlock(used, nodeId, firstInRelationId, firstOutRelationId, nextPropertyId, labelId);
        }

        public static IO.EdgeBlock ReadEdgeBlock(int edgeId)
        {
            var buffer = new byte[DbControl.BlockByteSize[DbControl.EdgePath]];
            ReadBlock(DbControl.EdgePath, edgeId, buffer);
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

        public static IO.GenericStringBlock ReadGenericStringBlock(string storagePath, int id)
        {
            var buffer = new byte[DbControl.BlockByteSize[storagePath]];
            ReadBlock(storagePath, id, buffer);
            var used = BitConverter.ToBoolean(buffer, 0);
            var bitsUsed = buffer[1];
            var text = System.Text.Encoding.UTF8.GetString(buffer.Skip(2).Take(bitsUsed).ToArray());
            return new GenericStringBlock(storagePath, used, text, id);
        }

        public static IO.PropertyBlock ReadPropertyBlock(string storagePath, int id)
        {
            var buffer = new byte[DbControl.BlockByteSize[storagePath]];
            ReadBlock(storagePath, id, buffer);
            var used = buffer[0] % 2 == 1;
            var dtype = (PropertyType) (buffer[0] >> 1);
            var propertyName = BitConverter.ToInt32(buffer.Skip(1).Take(4).ToArray(), 0);
            var propertyValue =buffer.Skip(5).Take(4).ToArray();
            var nextProperty = BitConverter.ToInt32(buffer.Skip(9).Take(4).ToArray(), 0);
            var nodeId = BitConverter.ToInt32(buffer.Skip(13).Take(4).ToArray(), 0);
            return new PropertyBlock(storagePath, id, used, dtype, propertyName, propertyValue, nextProperty, nodeId);
        }

        /// <summary>
        /// Read specific block from file
        /// </summary>
        /// <param name="filePath">Path to the file with byte-record structure</param>
        /// <param name="blockNumber">Block position from the beggining of the file</param>
        /// <param name="block"> Buffer to which result is written</param>
        public static void ReadBlock(string filePath, int blockNumber, byte[] block)
        {
            int offset = blockNumber * DbControl.BlockByteSize[filePath];
            DbControl.FileStreamDictionary[filePath].Seek(offset, SeekOrigin.Begin);
            DbControl.FileStreamDictionary[filePath].Read(block, 0, DbControl.BlockByteSize[filePath]);
        }
    }
}

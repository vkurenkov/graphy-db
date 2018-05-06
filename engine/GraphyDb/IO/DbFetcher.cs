using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphyDb.IO
{
    //TODO Make it use its own FileStreams
    internal static class DbFetcher
    {
        internal static readonly Dictionary<string, FileStream>
            FetcherStreamDictionary = new Dictionary<string, FileStream>();

        internal static void InitializeDbFetcher()
        {
            foreach (var filePath in DbControl.DbFilePaths)
            {
                FetcherStreamDictionary[filePath] = new FileStream(Path.Combine(DbControl.DbPath, filePath),
                    FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            }
        }

        internal static void CloseIOStreams()
        {
            foreach (var filePath in DbControl.DbFilePaths)
            {
                FetcherStreamDictionary?[filePath].Dispose();
                FetcherStreamDictionary[filePath] = null;
            }
        }

        public static HashSet<NodeBlock> SelectNodesByLabelAndProperty(int propertyNameId, byte[] propertyValue, int labelId)
        {
            var outputNodes = new HashSet<NodeBlock>();
            var lastPropertyId = DbControl.FetchLastId(DbControl.NodePropertyPath);
            NodePropertyBlock currentNodePropertyBlock = null;
            for (int propBlockIndex = 1; propBlockIndex < lastPropertyId; ++propBlockIndex)
            {
                currentNodePropertyBlock =
                    new NodePropertyBlock(DbReader.ReadPropertyBlock(DbControl.NodePropertyPath, propBlockIndex));
                if (currentNodePropertyBlock.PropertyName == propertyNameId)
                {
                    if (BitConverter.ToInt32(currentNodePropertyBlock.Value,0) == BitConverter.ToInt32(propertyValue, 0))
                    {
                     Console.WriteLine();   
                    }
                }
            }

            return outputNodes;
        }

        public static HashSet<NodeBlock> SelectNodesByLabel(int labelId)
        {
            var outputNodes = new HashSet<NodeBlock>();

            return outputNodes;
        }
    }
}
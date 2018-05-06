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

        public static HashSet<NodeBlock> SelectNodesByLabelAndProperty(Dictionary<int, byte[]> propertyKeyValue,
            int labelId)
        {
            var interimNodes = new Dictionary<int, HashSet<NodeBlock>>();
            foreach (var entry in propertyKeyValue)
                interimNodes[entry.Key] = new HashSet<NodeBlock>();
            var propertyKeys = new HashSet<int>(interimNodes.Keys);
            var lastPropertyId = DbControl.FetchLastId(DbControl.NodePropertyPath);
            for (var propBlockIndex = 1; propBlockIndex < lastPropertyId; ++propBlockIndex)
            {
                var currentNodePropertyBlock =
                    new NodePropertyBlock(DbReader.ReadPropertyBlock(DbControl.NodePropertyPath, propBlockIndex));

                if (!propertyKeys.Contains(currentNodePropertyBlock.PropertyName)) continue;

                if (BitConverter.ToInt32(currentNodePropertyBlock.Value, 0) !=
                    BitConverter.ToInt32(propertyKeyValue[currentNodePropertyBlock.Id], 0)) continue;

                var currentNodeBlock = DbReader.ReadNodeBlock(currentNodePropertyBlock.NodeId);
                if (currentNodeBlock.LabelId != labelId) continue;

                interimNodes[currentNodePropertyBlock.Id].Add(currentNodeBlock);
            }

            var listOfSets = new List<HashSet<NodeBlock>>(interimNodes.Values);

            var outputNodes = listOfSets.Aggregate(
                (h, e) =>
                {
                    h.IntersectWith(e);
                    return h;
                }
            );

            return outputNodes;
        }

        public static HashSet<NodeBlock> SelectNodesByLabel(int labelId)
        {
            var outputNodes = new HashSet<NodeBlock>();

            return outputNodes;
        }
    }
}
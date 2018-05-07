using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphyDb.IO
{
    internal static class DbFetcher
    {
        internal static readonly Dictionary<string, FileStream>
            FetcherStreamDictionary = new Dictionary<string, FileStream>();

        internal static void InitializeDbFetcher()
        {
            foreach (var filePath in DbControl.DbFilePaths)
            {
                FetcherStreamDictionary[filePath] = new FileStream(Path.Combine(DbControl.DbPath, filePath),
                    FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
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


        // todo: review
        public static HashSet<NodeBlock> SelectNodeBlocksByLabelAndProperties(string label,
            Dictionary<string, object> props)
        {
            bool ok = DbControl.LabelInvertedIndex.TryGetValue(label, out var labelId);

            if (!ok)
            {
                return new HashSet<NodeBlock>();
            }


            var rawProps = new Dictionary<int, object>();


            var fromPropNameIdToGoodNodeBlocks = new Dictionary<int, HashSet<NodeBlock>>();


            foreach (var keyValuePair in props)
            {
                ok = DbControl.PropertyNameInvertedIndex.TryGetValue(keyValuePair.Key, out var propertyNameId);
                if (!ok)
                {
                    return new HashSet<NodeBlock>();
                }

                fromPropNameIdToGoodNodeBlocks[propertyNameId] = new HashSet<NodeBlock>();
                rawProps[propertyNameId] = keyValuePair.Value;
            }

            var propertyNameIds = new HashSet<int>(fromPropNameIdToGoodNodeBlocks.Keys);


            var lastPropertyId = DbControl.FetchLastId(DbControl.NodePropertyPath);
            for (var propertyId = 1; propertyId < lastPropertyId; ++propertyId)
            {
                var currentPropertyBlock =
                    new NodePropertyBlock(DbReader.ReadPropertyBlock(DbControl.NodePropertyPath, propertyId));
                if (!currentPropertyBlock.Used)
                    continue;

                if (!propertyNameIds.Contains(currentPropertyBlock.PropertyNameId)) continue;


                switch (currentPropertyBlock.PropertyType)
                {
                    case PropertyType.Int:
                        if (BitConverter.ToInt32(currentPropertyBlock.Value, 0) !=
                            (int) rawProps[currentPropertyBlock.PropertyNameId])
                            continue;
                        break;
                    case PropertyType.String:
                        if (DbReader.ReadGenericStringBlock(DbControl.StringPath,
                                BitConverter.ToInt32(currentPropertyBlock.Value, 0)).Data !=
                            (string) rawProps[currentPropertyBlock.PropertyNameId])
                            continue;
                        break;
                    case PropertyType.Bool:
                        if (BitConverter.ToBoolean(currentPropertyBlock.Value, 3) !=
                            (bool) rawProps[currentPropertyBlock.PropertyNameId])
                            continue;
                        break;
                    case PropertyType.Float:
                        if (BitConverter.ToSingle(currentPropertyBlock.Value, 0) !=
                            (float) rawProps[currentPropertyBlock.PropertyNameId])
                            continue;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                var currentNodeBlock = DbReader.ReadNodeBlock(currentPropertyBlock.NodeId);
                if (currentNodeBlock.LabelId != labelId) continue;

                fromPropNameIdToGoodNodeBlocks[currentPropertyBlock.PropertyNameId].Add(currentNodeBlock);
            }

            var listOfSets = new List<HashSet<NodeBlock>>(fromPropNameIdToGoodNodeBlocks.Values);

            var outputNodes = listOfSets.Aggregate(
                (h, e) =>
                {
                    h.IntersectWith(e);
                    return h;
                }
            );

            return outputNodes;
        }
    }
}
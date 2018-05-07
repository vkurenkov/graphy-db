using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphyDb.IO
{
    internal class DbFetcher
    {
        private readonly DbControl dbControl;

        public DbFetcher(DbControl dbControl)
        {
            this.dbControl = dbControl;
        }

        public List<NodeBlock> SelectAllNodeBlocks()
        {
            var result = new List<NodeBlock>();

            var lastNodeId = dbControl.FetchLastId(DbControl.NodePath);
            // todo: Юра, проверь, может должно быть <=
            for (var nodeId = 1; nodeId < lastNodeId; ++nodeId)
            {
                var candidateNodeBlock = dbControl.DbReader.ReadNodeBlock(nodeId);
                if (candidateNodeBlock.Used)
                    result.Add(candidateNodeBlock);
            }

            return result;
        }


        // todo: review
        public HashSet<NodeBlock> SelectNodeBlocksByLabelAndProperties(string label,
            Dictionary<string, object> props)
        {
            if (label == null && props.Count == 0)
            {
                return new HashSet<NodeBlock>(SelectAllNodeBlocks());
            }


            var labelId = 0;

            if (label != null)
            {
                bool ok = dbControl.LabelInvertedIndex.TryGetValue(label, out labelId);

                if (!ok)
                {
                    return new HashSet<NodeBlock>();
                }
            }


            if (label != null && props.Count == 0)
            {
                var result = new HashSet<NodeBlock>();

                foreach (var nodeBlock in SelectAllNodeBlocks())
                {
                    if (nodeBlock.LabelId == labelId)
                    {
                        result.Add(nodeBlock);
                    }
                }

                return result;
            }


            var rawProps = new Dictionary<int, object>();

            var fromPropNameIdToGoodNodeBlocks = new Dictionary<int, HashSet<NodeBlock>>();

            foreach (var keyValuePair in props)
            {
                bool ok = dbControl.PropertyNameInvertedIndex.TryGetValue(keyValuePair.Key, out var propertyNameId);
                if (!ok)
                {
                    return new HashSet<NodeBlock>();
                }

                fromPropNameIdToGoodNodeBlocks[propertyNameId] = new HashSet<NodeBlock>();
                rawProps[propertyNameId] = keyValuePair.Value;
            }

            var propertyNameIds = new HashSet<int>(fromPropNameIdToGoodNodeBlocks.Keys);


            var lastPropertyId = dbControl.FetchLastId(DbControl.NodePropertyPath);
            // todo: Юра, проверь, может должно быть <=
            for (var propertyId = 1; propertyId < lastPropertyId; ++propertyId)
            {
                var currentPropertyBlock =
                    new NodePropertyBlock(dbControl.DbReader                        .ReadPropertyBlock(DbControl.NodePropertyPath, propertyId));
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
                        if (dbControl.DbReader.ReadGenericStringBlock(DbControl.StringPath,
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

                var currentNodeBlock = dbControl.DbReader.ReadNodeBlock(currentPropertyBlock.NodeId);
                if (labelId != 0 && currentNodeBlock.LabelId != labelId) continue;

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
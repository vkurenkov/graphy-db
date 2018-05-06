using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using GraphyDb.IO;

namespace GraphyDb
{
    public static class EventualConsister
    {
        public static readonly BlockingCollection<Entity> ChangedEntitiesQueue = new BlockingCollection<Entity>();

        public static void ConsisterMonitor()
        {
            var th = Thread.CurrentThread;
            th.Name = "ConsisterMonitor";
            while (true)
            {
                var entity = ChangedEntitiesQueue.Take();
                var entityType = entity.GetType();

                if (((entity.State & EntityState.Deleted) == EntityState.Deleted) &
                    ((entity.State & EntityState.Added) == EntityState.Added)) continue;

                if ((entity.State & EntityState.Deleted) == EntityState.Deleted)
                {
                    if (entityType == typeof(Node))
                    {
                        DbWriter.InvalidateBlock(DbControl.NodePath, ((Node) entity).NodeId);
                    }
                    else if (entityType == typeof(Relation))
                    {
                        DbWriter.InvalidateBlock(DbControl.EdgePath, ((Relation) entity).RelationId);
                    }
                    else if (entityType == typeof(NodeProperty))
                    {
                        DbWriter.InvalidateBlock(DbControl.NodePropertyPath, ((NodeProperty) entity).PropertyId);
                    }
                    else if (entityType == typeof(RelationProperty))
                    {
                        DbWriter.InvalidateBlock(DbControl.EdgePropertyPath, ((RelationProperty) entity).PropertyId);
                    }

                    continue;
                }

                if ((entity.State & EntityState.Added) == EntityState.Added)
                {
                    byte[] byteValue = new byte[4];
                    NodeBlock nodeBlock;
                    EdgeBlock relationBlock;
                    switch (entity)
                    {
                        case Node node:
                            nodeBlock = new NodeBlock(true, node.NodeId, 0, 0, 0, DbControl.FetchLabelId(node.Label));
                            DbWriter.WriteNodeBlock(nodeBlock);
                            break;
                        case Relation relation:
                            //Cast, Create with given information
                            relationBlock = new EdgeBlock
                            {
                                Used = true,
                                FirstNode = relation.From.NodeId,
                                SecondNode = relation.To.NodeId,
                                FirstNodePreviousRelation = 0,
                                SecondNodePreviousRelation = 0,
                                LabelId = DbControl.FetchLabelId(relation.Label),
                                NextProperty = 0
                            };

                            // Read Source, Target nodes to change the links in them and get their current links
                            var nodeFrom = DbReader.ReadNodeBlock(relationBlock.FirstNode);
                            var nodeTo = DbReader.ReadNodeBlock(relationBlock.SecondNode);

                            // Point to the current relations
                            relationBlock.FirstNodeNextRelation = nodeFrom.FirstOutRelationId;
                            relationBlock.SecondNodeNextRelation = nodeTo.FirstInRelationId;

                            // Read Relations to which nodes point to update them
                            if (nodeFrom.FirstOutRelationId != 0)
                            {
                                var nodeFromFirstOutRelationBlock = DbReader.ReadEdgeBlock(nodeFrom.FirstOutRelationId);
                                nodeFromFirstOutRelationBlock.FirstNodePreviousRelation = relation.RelationId;
                                DbWriter.WriteEdgeBlock(nodeFromFirstOutRelationBlock);
                            }

                            if (nodeTo.FirstInRelationId != 0)
                            {
                                var nodeToFirstInRelationBlock = DbReader.ReadEdgeBlock(nodeTo.FirstInRelationId);
                                nodeToFirstInRelationBlock.SecondNodePreviousRelation = relation.RelationId;
                                DbWriter.WriteEdgeBlock(nodeToFirstInRelationBlock);
                            }

                            nodeTo.FirstInRelationId = relation.RelationId;
                            nodeFrom.FirstOutRelationId = relation.RelationId;
                            DbWriter.WriteNodeBlock(nodeTo);
                            DbWriter.WriteNodeBlock(nodeFrom);
                            DbWriter.WriteEdgeBlock(relationBlock);
                            break;
                        case NodeProperty nodeProperty:

                            switch (nodeProperty.PropertyType)
                            {
                                case PropertyType.Int:
                                    byteValue = BitConverter.GetBytes((int) nodeProperty.Value);
                                    break;
                                case PropertyType.Bool:
                                    byteValue = BitConverter.GetBytes((bool) nodeProperty.Value);
                                    break;
                                case PropertyType.Float:
                                    byteValue = BitConverter.GetBytes((float) nodeProperty.Value);
                                    break;
                                case PropertyType.String:
                                    throw new NotImplementedException("String support is not available.");
                            }

                            var nodeId = ((Node) nodeProperty.Parent).NodeId;
                            var nodePropertyBlock =
                                new NodePropertyBlock(nodeProperty.PropertyId, true, nodeProperty.PropertyType,
                                    DbControl.FetchPropertyNameId(nodeProperty.Key),
                                    byteValue, 0, nodeId);
                            nodeBlock = DbReader.ReadNodeBlock(nodeId);
                            nodeBlock.NextPropertyId = nodePropertyBlock.Id;
                            DbWriter.WritePropertyBlock(nodePropertyBlock);
                            DbWriter.WriteNodeBlock(nodeBlock);
                            break;
                        case RelationProperty relationProperty:
//                            byte[] byteValue = new byte[4];

                            switch (relationProperty.PropertyType)
                            {
                                case PropertyType.Int:
                                    byteValue = BitConverter.GetBytes((int) relationProperty.Value);
                                    break;
                                case PropertyType.Bool:
                                    byteValue = BitConverter.GetBytes((bool) relationProperty.Value);
                                    break;
                                case PropertyType.Float:
                                    byteValue = BitConverter.GetBytes((float) relationProperty.Value);
                                    break;
                                case PropertyType.String:
                                    throw new NotImplementedException("String support is not available.");
                            }

                            var relationId = ((Relation) relationProperty.Parent).RelationId;
                            var relationPropertyBlock =
                                new EdgePropertyBlock(relationProperty.PropertyId, true, relationProperty.PropertyType,
                                    DbControl.FetchPropertyNameId(relationProperty.Key),
                                    byteValue, 0, relationId);
                            relationBlock = DbReader.ReadEdgeBlock(relationId);
                            relationBlock.NextProperty = relationPropertyBlock.NextProperty;
                            DbWriter.WritePropertyBlock(relationPropertyBlock);
                            DbWriter.WriteEdgeBlock(relationBlock);
                            break;
                    }

                    continue;
                }

                if ((entity.State & EntityState.Modified) == EntityState.Modified)
                {
                    //                    throw new NotImplementedException("Modification is not available");

                    byte[] byteValue = new byte[4];
                    switch (entity)
                    {
                        case Node node:
                            throw new NotSupportedException(
                                "Node modification is not supported. Update it's properties instead.");
                        case Relation relation:
                            throw new NotSupportedException(
                                "Relation modification is not supported. Update it's properties instead.");
                        case NodeProperty _:
                        case RelationProperty _:
                            var propertyPath = DbControl.EdgePropertyPath;
                            var property = (Property) entity;
                            if (property is NodeProperty) propertyPath = DbControl.NodePropertyPath;

                            var oldPropertyBlock =
                                DbReader.ReadPropertyBlock(propertyPath, property.PropertyId);
                            switch (property.PropertyType)
                            {
                                case PropertyType.String:
                                    //TODO Process strings!
                                    break;
                                default:
                                    switch (property.PropertyType)
                                    {
                                        case PropertyType.Int:
                                            byteValue = BitConverter.GetBytes((int) property.Value);
                                            break;
                                        case PropertyType.Bool:
                                            byteValue = BitConverter.GetBytes((bool) property.Value);
                                            break;
                                        case PropertyType.Float:
                                            byteValue = BitConverter.GetBytes((float) property.Value);
                                            break;
                                        case PropertyType.String:
                                            throw new NotImplementedException("String support is not available.");
                                    }

                                    oldPropertyBlock.Value = byteValue;
                                    DbWriter.WritePropertyBlock(oldPropertyBlock);
                                    break;
                            }

                            break;
                        default:
                            throw new NotSupportedException($"{typeof(entity)} is not supported!");
                    }
                }


                //TODO Read file, fetch Entities, process entities
            }
        }
    }
}
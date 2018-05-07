using System;
using System.Collections.Concurrent;
using System.Threading;
using GraphyDb.IO;

namespace GraphyDb
{
    public class KillConsisterEntity : Entity
    {
    }




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

                if (entityType == typeof(KillConsisterEntity))
                {
                    return;
                }

                if (((entity.State & EntityState.Deleted) == EntityState.Deleted) &
                    ((entity.State & EntityState.Added) == EntityState.Added)) continue;

                if ((entity.State & EntityState.Deleted) == EntityState.Deleted)
                {
                    if (entityType == typeof(Node))
                    {
                        var node = ((Node) entity);
                        DbWriter.InvalidateBlock(DbControl.NodePath, node.NodeId);
                        var nodeBlock = DbReader.ReadNodeBlock(node.NodeId);
                        var nextNodePropertyId = nodeBlock.FirstPropertyId;
                        while (nextNodePropertyId != 0)
                        {
                            var nextPropertyBlock =
                                DbReader.ReadPropertyBlock(DbControl.NodePropertyPath, nextNodePropertyId);
                            DbWriter.InvalidateBlock(DbControl.NodePropertyPath, nextNodePropertyId);
                            if (nextPropertyBlock.PropertyType is PropertyType.String)
                            {
                                DbWriter.InvalidateBlock(DbControl.StringPath,
                                    BitConverter.ToInt32(nextPropertyBlock.Value, 0));
                            }

                            nextNodePropertyId = nextPropertyBlock.NextPropertyId;
                        }

                        var nextOutRelationId = nodeBlock.FirstOutRelationId;
                        while (nextOutRelationId != 0)
                        {
                            var nextOutRelationBLock = DbReader.ReadRelationBlock(nextOutRelationId);
                            DbWriter.InvalidateBlock(DbControl.RelationPath, nextOutRelationId);
                            var nextRelationPropertyId = nextOutRelationBLock.FirstPropertyId;
                            while (nextRelationPropertyId != 0)
                            {
                                var nextPropertyBlock = DbReader.ReadPropertyBlock(DbControl.RelationPropertyPath,
                                    nextRelationPropertyId);
                                DbWriter.InvalidateBlock(DbControl.RelationPropertyPath, nextRelationPropertyId);
                                if (nextPropertyBlock.PropertyType is PropertyType.String)
                                {
                                    DbWriter.InvalidateBlock(DbControl.StringPath,
                                        BitConverter.ToInt32(nextPropertyBlock.Value, 0));
                                }

                                nextRelationPropertyId = nextPropertyBlock.NextPropertyId;
                            }

                            nextOutRelationId = nextOutRelationBLock.FirstNodeNextRelation;
                        }

                        var nextInRelationId = nodeBlock.FirstInRelationId;
                        while (nextInRelationId != 0)
                        {
                            var nextInRelationBlock = DbReader.ReadRelationBlock(nextInRelationId);
                            DbWriter.InvalidateBlock(DbControl.RelationPath, nextInRelationId);
                            var nextRelationPropertyId = nextInRelationBlock.FirstPropertyId;
                            while (nextRelationPropertyId != 0)
                            {
                                var nextPropertyBlock =
                                    DbReader.ReadPropertyBlock(DbControl.RelationPropertyPath, nextRelationPropertyId);
                                DbWriter.InvalidateBlock(DbControl.RelationPropertyPath, nextRelationPropertyId);
                                if (nextPropertyBlock.PropertyType is PropertyType.String)
                                {
                                    DbWriter.InvalidateBlock(DbControl.StringPath,
                                        BitConverter.ToInt32(nextPropertyBlock.Value, 0));
                                }

                                nextRelationPropertyId = nextPropertyBlock.NextPropertyId;
                            }

                            nextInRelationId = nextInRelationBlock.SecondNodeNextRelation;
                        }
                    }
                    else if (entityType == typeof(Relation))
                    {
                        var relation = ((Relation) entity);
                        DbWriter.InvalidateBlock(DbControl.RelationPath, relation.RelationId);
                        var relationBlock = DbReader.ReadRelationBlock(relation.RelationId);
                        var nextPropertyId = relationBlock.FirstPropertyId;
                        while (nextPropertyId != 0)
                        {
                            var nextPropertyBlock =
                                DbReader.ReadPropertyBlock(DbControl.RelationPropertyPath, nextPropertyId);
                            DbWriter.InvalidateBlock(DbControl.NodePropertyPath, nextPropertyId);
                            if (nextPropertyBlock.PropertyType is PropertyType.String)
                            {
                                DbWriter.InvalidateBlock(DbControl.StringPath,
                                    BitConverter.ToInt32(nextPropertyBlock.Value, 0));
                            }

                            nextPropertyId = nextPropertyBlock.NextPropertyId;
                        }
                    }
                    else if (entityType == typeof(NodeProperty))
                    {
                        var nodeProperty = ((NodeProperty) entity);
                        if (nodeProperty.PropertyType is PropertyType.String)
                        {
                            DbWriter.InvalidateBlock(DbControl.StringPath, (int) nodeProperty.Value);
                        }

                        DbWriter.InvalidateBlock(DbControl.NodePropertyPath, nodeProperty.PropertyId);
                    }
                    else if (entityType == typeof(RelationProperty))
                    {
                        var relationProperty = ((RelationProperty) entity);
                        if (relationProperty.PropertyType is PropertyType.String)
                        {
                            DbWriter.InvalidateBlock(DbControl.StringPath, (int) relationProperty.Value);
                        }

                        DbWriter.InvalidateBlock(DbControl.RelationPropertyPath,
                            relationProperty.PropertyId);
                    }
                    else
                    {
                        throw new NotSupportedException("Not supported Entity Type");
                    }

                    continue;
                }

                if ((entity.State & EntityState.Added) == EntityState.Added)
                {
                    NodeBlock nodeBlock;
                    RelationBlock relationBlock;
                    switch (entity)
                    {
                        case Node node:
                            nodeBlock = new NodeBlock(true, node.NodeId, 0, 0, 0, DbControl.FetchLabelId(node.Label));
                            DbWriter.WriteNodeBlock(nodeBlock);
                            break;
                        case Relation relation:
                            //Cast, Create with given information
                            relationBlock = new RelationBlock
                            {
                                Used = true,
                                FirstNodeId = relation.From.NodeId,
                                SecondNodeId = relation.To.NodeId,
                                FirstNodePreviousRelationId = 0,
                                SecondNodePreviousRelationId = 0,
                                LabelId = DbControl.FetchLabelId(relation.Label),
                                FirstPropertyId = 0,
                                RelationId = relation.RelationId
                            };

                            // Read Source, Target nodes to change the links in them and get their current links
                            var fromNodeBlock = DbReader.ReadNodeBlock(relationBlock.FirstNodeId);
                            var toNodeBlock = DbReader.ReadNodeBlock(relationBlock.SecondNodeId);

                            // Point to the current relations
                            relationBlock.FirstNodeNextRelation = fromNodeBlock.FirstOutRelationId;
                            relationBlock.SecondNodeNextRelation = toNodeBlock.FirstInRelationId;

                            // Read Relations to which nodes point to update them
                            if (fromNodeBlock.FirstOutRelationId != 0)
                            {
                                var fromNodeFirstOutRelationBlock =
                                    DbReader.ReadRelationBlock(fromNodeBlock.FirstOutRelationId);
                                fromNodeFirstOutRelationBlock.FirstNodePreviousRelationId = relation.RelationId;
                                DbWriter.WriteRelationBlock(fromNodeFirstOutRelationBlock);
                            }

                            if (toNodeBlock.FirstInRelationId != 0)
                            {
                                var toNodeFirstInRelationBlock =
                                    DbReader.ReadRelationBlock(toNodeBlock.FirstInRelationId);
                                toNodeFirstInRelationBlock.SecondNodePreviousRelationId = relation.RelationId;
                                DbWriter.WriteRelationBlock(toNodeFirstInRelationBlock);
                            }

                            toNodeBlock.FirstInRelationId = relation.RelationId;
                            fromNodeBlock.FirstOutRelationId = relation.RelationId;
                            DbWriter.WriteNodeBlock(toNodeBlock);
                            DbWriter.WriteNodeBlock(fromNodeBlock);
                            DbWriter.WriteRelationBlock(relationBlock);
                            break;
                        case NodeProperty _:
                        case RelationProperty _:
                            var property = (Property) entity;
                            byte[] byteValue = new byte[4];
                            switch (property.PropertyType)
                            {
                                case PropertyType.Int:
                                    byteValue = BitConverter.GetBytes((int) property.Value);
                                    break;
                                case PropertyType.Bool:
                                    byteValue[3] = (byte) ((bool) property.Value ? 1 : 0);
                                    break;
                                case PropertyType.Float:
                                    byteValue = BitConverter.GetBytes((float) property.Value);
                                    break;
                                case PropertyType.String:
                                    // Add to String Storage, get returned pointer to the string storage, write it as the byteValue
                                    var newStringId = DbControl.AllocateId(DbControl.StringPath);
                                    DbWriter.WriteStringBlock(new StringBlock(true, (string) property.Value,
                                        newStringId));
                                    byteValue = BitConverter.GetBytes(newStringId);
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }

                            int parentId;
                            PropertyBlock propertyBlock;
                            switch (property)
                            {
                                case NodeProperty _:
                                    parentId = ((Node) property.Parent).NodeId;
                                    propertyBlock = new NodePropertyBlock(property.PropertyId, true,
                                        property.PropertyType,
                                        DbControl.FetchPropertyNameId(property.Key),
                                        byteValue, 0, parentId);
                                    nodeBlock = DbReader.ReadNodeBlock(parentId);
                                    propertyBlock.NextPropertyId = nodeBlock.FirstPropertyId;
                                    nodeBlock.FirstPropertyId = propertyBlock.PropertyId;
                                    DbWriter.WritePropertyBlock(propertyBlock);
                                    DbWriter.WriteNodeBlock(nodeBlock);
                                    break;
                                case RelationProperty _:
                                    parentId = ((Relation) property.Parent).RelationId;
                                    propertyBlock = new RelationPropertyBlock(property.PropertyId, true,
                                        property.PropertyType,
                                        DbControl.FetchPropertyNameId(property.Key),
                                        byteValue, 0, parentId);
                                    relationBlock = DbReader.ReadRelationBlock(parentId);
                                    propertyBlock.NextPropertyId = relationBlock.FirstPropertyId;
                                    relationBlock.FirstPropertyId = propertyBlock.PropertyId;
                                    DbWriter.WritePropertyBlock(propertyBlock);
                                    DbWriter.WriteRelationBlock(relationBlock);
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }

                            break;
                    }

                    continue;
                }

                if ((entity.State & EntityState.Modified) == EntityState.Modified)
                {
                    switch (entity)
                    {
                        case Node _:
                            throw new NotSupportedException(
                                "Node modification is not supported. Update it's properties instead.");
                        case Relation _:
                            throw new NotSupportedException(
                                "Relation modification is not supported. Update it's properties instead.");
                        case NodeProperty _: //
                        case RelationProperty _:
                            var property = (Property) entity;
                            var propertyPath = (property is NodeProperty)
                                ? DbControl.NodePropertyPath
                                : DbControl.RelationPropertyPath;
                            var oldPropertyBlock = DbReader.ReadPropertyBlock(propertyPath, property.PropertyId);

                            byte[] byteValue = new byte[4];
                            switch (property.PropertyType)
                            {
                                case PropertyType.Int:
                                    byteValue = BitConverter.GetBytes((int) property.Value);
                                    break;
                                case PropertyType.Bool:
                                    byteValue[3] = (byte) ((bool) property.Value ? 1 : 0);
                                    break;
                                case PropertyType.Float:
                                    byteValue = BitConverter.GetBytes((float) property.Value);
                                    break;
                                case PropertyType.String:
                                    DbWriter.InvalidateBlock(DbControl.StringPath,
                                        BitConverter.ToInt32(oldPropertyBlock.Value, 0));
                                    var newStringId = DbControl.AllocateId(DbControl.StringPath);
                                    DbWriter.WriteStringBlock(new StringBlock(true, (string) property.Value,
                                        newStringId));
                                    byteValue = BitConverter.GetBytes(newStringId);
                                    break;
                                default:
                                    throw new NotSupportedException("Such Property dtye is not supported");
                            }

                            oldPropertyBlock.Value = byteValue;
                            DbWriter.WritePropertyBlock(oldPropertyBlock);
                            break;
                    }
                }
            }
        }
    }
}
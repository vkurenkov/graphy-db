using System;
using System.Collections.Concurrent;
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
                        DbWriter.InvalidateBlock(DbControl.RelationPath, ((Relation) entity).RelationId);
                    }
                    else if (entityType == typeof(NodeProperty))
                    {
                        DbWriter.InvalidateBlock(DbControl.NodePropertyPath, ((NodeProperty) entity).PropertyId);
                    }
                    else if (entityType == typeof(RelationProperty))
                    {
                        DbWriter.InvalidateBlock(DbControl.RelationPropertyPath, ((RelationProperty) entity).PropertyId);
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
                                FirstNode = relation.From.NodeId,
                                SecondNode = relation.To.NodeId,
                                FirstNodePreviousRelation = 0,
                                SecondNodePreviousRelation = 0,
                                LabelId = DbControl.FetchLabelId(relation.Label),
                                FirstPropertyId = 0
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
                                var nodeFromFirstOutRelationBlock = DbReader.ReadRelationBlock(nodeFrom.FirstOutRelationId);
                                nodeFromFirstOutRelationBlock.FirstNodePreviousRelation = relation.RelationId;
                                DbWriter.WriteRelationBlock(nodeFromFirstOutRelationBlock);
                            }

                            if (nodeTo.FirstInRelationId != 0)
                            {
                                var nodeToFirstInRelationBlock = DbReader.ReadRelationBlock(nodeTo.FirstInRelationId);
                                nodeToFirstInRelationBlock.SecondNodePreviousRelation = relation.RelationId;
                                DbWriter.WriteRelationBlock(nodeToFirstInRelationBlock);
                            }

                            nodeTo.FirstInRelationId = relation.RelationId;
                            nodeFrom.FirstOutRelationId = relation.RelationId;
                            DbWriter.WriteNodeBlock(nodeTo);
                            DbWriter.WriteNodeBlock(nodeFrom);
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
                                    byteValue = BitConverter.GetBytes((bool) property.Value);
                                    break;
                                case PropertyType.Float:
                                    byteValue = BitConverter.GetBytes((float) property.Value);
                                    break;
                                case PropertyType.String:
                                    throw new NotImplementedException("String support is not available.");
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
                                    nodeBlock.FirstPropertyId = propertyBlock.PropertyId;
                                    DbWriter.WritePropertyBlock(propertyBlock);
                                    DbWriter.WriteNodeBlock(nodeBlock);
                                    break;
                                case RelationProperty _:
                                    parentId = ((Relation) property.Parent).RelationId;
                                    propertyBlock = new NodePropertyBlock(property.PropertyId, true,
                                        property.PropertyType,
                                        DbControl.FetchPropertyNameId(property.Key),
                                        byteValue, 0, parentId);
                                    relationBlock = DbReader.ReadRelationBlock(parentId);
                                    relationBlock.FirstPropertyId = propertyBlock.NextProperty;
                                    DbWriter.WritePropertyBlock(propertyBlock);
                                    DbWriter.WriteRelationBlock(relationBlock);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(property));
                            }

                            break;
                    }

                    continue;
                }

                if ((entity.State & EntityState.Modified) == EntityState.Modified)
                {
                    //                    throw new NotImplementedException("Modification is not available");

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
                            var propertyPath = DbControl.RelationPropertyPath;
                            var property = (Property) entity;
                            if (property is NodeProperty) propertyPath = DbControl.NodePropertyPath;
                            var oldPropertyBlock = DbReader.ReadPropertyBlock(propertyPath, property.PropertyId);

                            byte[] byteValue;
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
                                default:
                                    throw new NotSupportedException("Such Property dtye is not supported");
                            }

                            oldPropertyBlock.Value = byteValue;
                            DbWriter.WritePropertyBlock(oldPropertyBlock);
                            break;
                    }

//                            throw new NotSupportedException($"{typeof(entity)} is not supported!");
//                    break;
                }
            }


            //TODO Read file, fetch Entities, process entities
        }
    }
}
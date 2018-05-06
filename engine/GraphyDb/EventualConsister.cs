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

                //TODO Read file, fetch Entities, process entities
            }
        }
    }
}
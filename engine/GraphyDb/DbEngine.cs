using System;
using System.Linq;
using System.Collections.Generic;
using GraphyDb.IO;

namespace GraphyDb
{
    public class DbEngine
    {
        public List<Entity> ChangedEntities;


        public DbEngine()
        {
            DbControl.InitializeIO();
            ChangedEntities = new List<Entity>();
        }

        public Node AddNode(string label)
        {
            return new Node(label, this, EntityState.Added);
        }

        public Relation AddRelation(Node from, Node to, string label)
        {
            return new Relation(from, to, label, EntityState.Added);
        }

        public void Delete(Entity entity)
        {
            entity.State = EntityState.Deleted;
            entity.Db.ChangedEntities.Add(entity);
        }

        public void SaveChanges()
        {
            foreach (var entity in ChangedEntities.Distinct())
            {
                if ((entity.State & EntityState.Added) == EntityState.Added)
                {
                    var entityType = entity.GetType();

                    if (entityType == typeof(Node))
                    {
                        ((Node)entity).NodeId = DbControl.AllocateId(DbControl.NodePath);
                    }
                    else if (entityType == typeof(Relation))
                    {
                        ((Relation)entity).RelationId = DbControl.AllocateId(DbControl.RelationPath);
                    }
                    else if (entityType == typeof(NodeProperty))
                    {
                        ((NodeProperty)entity).PropertyId = DbControl.AllocateId(DbControl.NodePropertyPath);
                    }
                    else if (entityType == typeof(RelationProperty))
                    {
                        ((RelationProperty)entity).PropertyId = DbControl.AllocateId(DbControl.RelationPropertyPath);
                    }
                }

                EventualConsister.ChangedEntitiesQueue.Add(entity);
            }


            ChangedEntities.Clear();
        }

        public void DropDatabase()
        {
            DbControl.DeleteDbFiles();
        }
    }
}

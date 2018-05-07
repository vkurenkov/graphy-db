using System;
using System.Linq;
using System.Collections.Generic;
using GraphyDb.IO;

namespace GraphyDb
{
    public class DbEngine
    {
        public List<Entity> ChangedEntities;

        private DbControl dbControl;

        public DbEngine()
        {
            dbControl = new DbControl();
            dbControl.InitializeIO();
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
                        ((Node) entity).NodeId = dbControl.AllocateId(DbControl.NodePath);
                    }
                    else if (entityType == typeof(Relation))
                    {
                        ((Relation) entity).RelationId = dbControl.AllocateId(DbControl.RelationPath);
                    }
                    else if (entityType == typeof(NodeProperty))
                    {
                        ((NodeProperty) entity).PropertyId = dbControl.AllocateId(DbControl.NodePropertyPath);
                    }
                    else if (entityType == typeof(RelationProperty))
                    {
                        ((RelationProperty) entity).PropertyId = dbControl.AllocateId(DbControl.RelationPropertyPath);
                    }
                }

                EventualConsister.ChangedEntitiesQueue.Add(entity);
            }


            ChangedEntities.Clear();
        }

        public void DropDatabase()
        {
            dbControl.DeleteDbFiles();
        }
    }
}
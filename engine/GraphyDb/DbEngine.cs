using System.Collections.Generic;

namespace GraphyDb
{
    public class DbEngine
    {

        public List<Entity> ChangedEntities;


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

        }


    }
}

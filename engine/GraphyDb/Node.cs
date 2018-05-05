using System.Collections.Generic;

namespace GraphyDb
{
    public class Node : Entity
    {
        public int NodeId;

        public int LabelId;
        public string Label;

        private readonly Dictionary<string, Property> properties;

        private List<Relation> outRelations;
        private List<Relation> inRelations;

        public UnitOfWork Db;


        public Node(string label, UnitOfWork db, EntityState state)
        {
            NodeId = 0;
            LabelId = 0;
            Label = label;
            Db = db;

            properties = new Dictionary<string, Property>();
            outRelations = new List<Relation>();
            inRelations = new List<Relation>();


            State = state;
            if (state != EntityState.Unchanged)
                Db.ChangedEntities.Add(this);
        }


        public object this[string key]
        {
            get => properties[key].Value;

            set
            {
                if (properties.TryGetValue(key, out var property))
                {
                    // modifying existing property:
                    property.Value = value;
                }
                else
                {
                    // adding new property:
                    properties[key] = new Property(this, key, value);
                }
            }
        }



    }
}
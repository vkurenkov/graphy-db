using System;
using System.Collections.Generic;

namespace GraphyDb
{
    public class Node : Entity, IEquatable<Node>
    {
        public bool Equals(Node other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return NodeId == other.NodeId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Node) obj);
        }

        public override int GetHashCode()
        {
            return NodeId;
        }

        public static bool operator ==(Node left, Node right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Node left, Node right)
        {
            return !Equals(left, right);
        }

        public int NodeId;

        public int LabelId;
        public string Label;

        private readonly Dictionary<string, NodeProperty> properties;

        public List<Relation> OutRelations { get { throw new NotImplementedException(); } }
        public List<Relation> InRelations { get { throw new NotImplementedException(); } }

        public void ResolveRelations()
        {
            throw new NotImplementedException();
        }

        public Node(string label, DbEngine db, EntityState state)
        {
            NodeId = 0;
            LabelId = 0;
            Label = label;
            Db = db;

            properties = new Dictionary<string, NodeProperty>();
//            OutRelations = new List<Relation>();
//            InRelations = new List<Relation>();


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
                    properties[key] = new NodeProperty(this, key, value);
                }
            }
        }



    }
}
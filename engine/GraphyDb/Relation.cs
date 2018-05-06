using System;
using System.Collections.Generic;

namespace GraphyDb
{
    public class Relation : Entity, IEquatable<Relation>
    {
        public bool Equals(Relation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RelationId == other.RelationId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Relation) obj);
        }

        public override int GetHashCode()
        {
            return RelationId;
        }

        public static bool operator ==(Relation left, Relation right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Relation left, Relation right)
        {
            return !Equals(left, right);
        }

        public int RelationId;

        public Node From;
        public Node To;

        public int LabelId;
        public string Label;

        private readonly Dictionary<string, RelationProperty> properties;

        public Relation(Node from, Node to, string label, EntityState state)
        {
            RelationId = 0;

            From = from;
            To = to;

            LabelId = 0;
            Label = label;

            if (from.Db != to.Db)
            {
                throw new ArgumentException();
            }

            Db = from.Db;

            properties = new Dictionary<string, RelationProperty>();

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
                    properties[key] = new RelationProperty(this, key, value);
                }
            }
        }


    }
}
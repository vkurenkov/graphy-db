using System;
using System.Collections.Generic;

namespace GraphyDb
{
    public enum PropertyType
    {
        Int = 0,
        String = 1,
        Bool = 2,
        Float = 3
    }


    public class Property : Entity
    {
        static readonly List<Type> SupportedTypes = new List<Type> {typeof(int), typeof(string), typeof(bool), typeof(float)};


        public int PropertyId;

        public Node Node;

        public string Key;

        public Property(Node node, string key, object value)
        {
            if (!SupportedTypes.Contains(value.GetType()))
            {
                throw new NotSupportedException("Cannot store properties with type " + value.GetType());
            }

            PropertyId = 0;

            Node = node ?? throw new ArgumentNullException(nameof(node));

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key) + " cannot be null or empty string");
            }
            Key = key;

            Value = value;

            State |= EntityState.Added;
            Node.Db.ChangedEntities.Add(this);
        }

        public object Value
        {
            get => value;
            set
            {
                if (!SupportedTypes.Contains(value.GetType()))
                {
                    throw new NotSupportedException("Cannot store properties with type " + value.GetType());
                }

                this.value = value;

                State |= EntityState.Modified;
                Node.Db.ChangedEntities.Add(this);
            }
        }

        private object value;
    }


}
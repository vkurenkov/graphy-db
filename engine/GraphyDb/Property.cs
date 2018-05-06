using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using GraphyDb.IO;

namespace GraphyDb
{
    public enum PropertyType : byte
    {
        Int = 0,
        String = 1,
        Bool = 2,
        Float = 3
    }


    public abstract class Property : Entity
    {
        static readonly List<Type> SupportedTypes =
            new List<Type> {typeof(int), typeof(string), typeof(bool), typeof(float)};


        public int PropertyId;

        internal Entity Parent;

        public string Key;

        protected Property(Entity parent, string key, object value)
        {
            if (!SupportedTypes.Contains(value.GetType()))
            {
                throw new NotSupportedException("Cannot store properties with type " + value.GetType());
            }

            PropertyId = 0;

            Parent = parent ?? throw new ArgumentNullException(nameof(parent));

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key) + " cannot be null or empty string");
            }

            Key = key;

            Value = value;

            Db = parent.Db;

            State |= EntityState.Added;
            Db.ChangedEntities.Add(this);
        }

        protected Property(Entity parent, PropertyBlock propertyBlock)
        {
            PropertyId = propertyBlock.Id;
            Parent = parent;
            Db = parent.Db;

            Key = DbReader.ReadGenericStringBlock(DbControl.PropertyNamePath, propertyBlock.PropertyNameId).Data;

            switch (propertyBlock.PropertyType)
            {
                case PropertyType.Int:
                    value = BitConverter.ToInt32(propertyBlock.Value, 0);
                    break;
                case PropertyType.String:
                    value = DbReader.ReadGenericStringBlock(DbControl.StringPath,
                        BitConverter.ToInt32(propertyBlock.Value, 0));
                    break;
                case PropertyType.Bool:
                    value = BitConverter.ToBoolean(propertyBlock.Value, 3);
                    break;
                case PropertyType.Float:
                    value = BitConverter.ToSingle(propertyBlock.Value, 0);
                    break;
                default:
                    throw new NotSupportedException("Unrecognized Property Type");
            }
        }

        public PropertyType PropertyType
        {
            get
            {
                switch (value)
                {
                    case int _:
                        return PropertyType.Int;
                    case bool _:
                        return PropertyType.Bool;
                    case float _:
                        return PropertyType.Float;
                    case string _:
                        return PropertyType.String;
                }

                throw new NotSupportedException();
            }
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
                Db.ChangedEntities.Add(this);
            }
        }

        private object value;
    }
}
using System;

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
        public int PropertyId;

        public Node Node;

        public string Key;

        public PropertyType PropertyType;
        public object Value { get; set; }
    }






}
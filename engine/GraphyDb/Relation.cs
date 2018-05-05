using System.Collections.Generic;

namespace GraphyDb
{
    public class Relation
    {
        public int RelationId;

        public Node From;
        public Node To;

        public int LabelId;
        public string Label;

        private List<Property> properties;

    }
}
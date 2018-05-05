using System.Collections.Generic;

namespace GraphyDb
{
    public class Node : Entity
    {
        public int NodeId;

        public int LabelId;
        public string Label;

        private List<Property> properties;

        private List<Relation> outRelations;
        private List<Relation> inRelations;

        private UnitOfWork db;
    }
}
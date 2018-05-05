using System;
using System.Collections.Generic;

namespace GraphyDb
{
    public class Relation : Entity
    {
        public int RelationId;

        public Node From;
        public Node To;

        public int LabelId;
        public string Label;

        private Dictionary<string, Property> properties;

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

            properties = new Dictionary<string, Property>();

            State = state;
            if (state != EntityState.Unchanged)
                From.Db.ChangedEntities.Add(this);
        }




    }
}
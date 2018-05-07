using System.Collections.Generic;

namespace GraphyDb
{
    public enum RelationsDirection
    {
        Right,
        Left
    }

    public class RelationSet
    {
        public RelationsDirection Direction;
        public HashSet<Relation> Relations;


        public RelationSet(RelationsDirection direction)
        {
            Direction = direction;
        }

    }
}
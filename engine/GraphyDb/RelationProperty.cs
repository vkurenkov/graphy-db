using GraphyDb.IO;

namespace GraphyDb
{
    public class RelationProperty : Property
    {
        public RelationProperty(Relation relation, string key, object value) : base(relation, key, value)
        {
        }

        public RelationProperty(Relation relation, PropertyBlock propertyBlock) : base(relation, propertyBlock)
        {
        }
    }
}
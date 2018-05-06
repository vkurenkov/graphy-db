using GraphyDb.IO;

namespace GraphyDb
{
    public class NodeProperty : Property
    {
        public NodeProperty(Node node, string key, object value) : base(node, key, value)
        {
        }

        public NodeProperty(Node node, PropertyBlock propertyBlock) : base(node, propertyBlock)
        {
        }
    }
}
using System.Collections.Generic;

namespace GraphyDb
{
    public class NodeSet
    {
        public HashSet<Node> Nodes;

        public NodeSet()
        {
            Nodes = new HashSet<Node>();
        }
    }
}
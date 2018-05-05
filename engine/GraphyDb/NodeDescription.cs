using System.Collections.Generic;

namespace GraphyDb
{
    public class NodeDescription
    {
        public readonly string Label;

        public readonly Dictionary<string, object> Props;


        public NodeDescription(string label, Dictionary<string, object> props)
        {
            Label = label;
            Props = props;
        }

        public NodeDescription(string label)
        {
            Label = label;
            Props = new Dictionary<string, object>();
        }

        
    }
}
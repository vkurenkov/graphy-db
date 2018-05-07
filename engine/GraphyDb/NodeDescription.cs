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


        public static NodeDescription Any()
        {
            return new NodeDescription(null, new Dictionary<string, object>());
        }



        public bool CheckNode(Node node)
        {
            if (Label != null && Label != node.Label)
            {
                return false;
            }

            foreach (var keyValue in Props)
            {
                if (!node.Properties.ContainsKey(keyValue.Key))
                    return false;

                if (node[keyValue.Key] != keyValue.Value)
                    return false;
            }

            return true;

        }



    }
}
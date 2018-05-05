using System.Collections.Generic;

namespace GraphyDb
{
    public class RelationDescription
    {
        public readonly string Label;

        public readonly Dictionary<string, object> Props;


        public RelationDescription(string label, Dictionary<string, object> props)
        {
            Label = label;
            Props = props;
        }

        public RelationDescription(string label)
        {
            Label = label;
            Props = new Dictionary<string, object>();
        }
    }
}
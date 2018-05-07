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

        public static RelationDescription Any()
        {
            return new RelationDescription(null, new Dictionary<string, object>());
        }




        public bool CheckRelation(Relation relation)
        {
            if (Label != null && Label != relation.Label)
            {
                return false;
            }

            foreach (var keyValue in Props)
            {
                if (!relation.Properties.ContainsKey(keyValue.Key))
                    return false;

                if (!relation[keyValue.Key].Equals(keyValue.Value))
                    return false;
            }

            return true;

        }
    }
}
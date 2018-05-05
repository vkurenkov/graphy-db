using System.Collections.Generic;

namespace GraphyDb
{
    public class Query
    {
        private UnitOfWork Db;

        public Query(UnitOfWork db)
        {
            Db = db;
        }

        private List<NodeSet> nodeSets;
        private List<RelationSet> relationSets;

        public NodeSet Match(NodeDescription nodeDescription)
        {
            // todo: do
            NodeSet result = null;
            nodeSets.Add(result);

            return result;
        }


        public RelationSet To(RelationDescription relationDescription)
        {

            // todo: do
            RelationSet result = null;
            relationSets.Add(result);

            return result;
        }

        public RelationSet From(RelationDescription relationDescription)
        {

            // todo: do
            RelationSet result = null;
            relationSets.Add(result);

            return result;
        }



    }
}
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GraphyDb.IO;

namespace GraphyDb
{
    public class Query
    {
        private readonly DbEngine db;

        public Query(DbEngine db)
        {
            this.db = db;
            nodeSets = new List<NodeSet>();
            relationSets = new List<RelationSet>();
        }

        private List<NodeSet> nodeSets;
        private List<RelationSet> relationSets;

        public NodeSet Match(NodeDescription nodeDescription)
        {
            var nodeBlocks = DbFetcher.SelectNodeBlocksByLabelAndProperties(nodeDescription.Label, nodeDescription.Props);


            var nodeSet = new NodeSet();

            foreach (var nodeBlock in nodeBlocks)
            {
                nodeSet.Nodes.Add(new Node(nodeBlock, db));
            }

            nodeSets.Add(nodeSet);

            return nodeSet;
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

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
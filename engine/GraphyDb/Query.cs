using System;
using System.Collections.Generic;
using System.Linq;
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
            if (nodeSets.Count != relationSets.Count + 1)
            {
                throw new Exception("To/From must be executed after Match");
            }

            var lastNodeLayer = nodeSets.Last().Nodes;


            var relationSet = new RelationSet();


            foreach (var node in lastNodeLayer)
            {
                node.PullOutRelations();

                foreach (var outRelation in node.OutRelations)
                {
                    
                }


            }

            





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
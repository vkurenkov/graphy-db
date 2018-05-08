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

        private readonly List<NodeSet> nodeSets;
        private readonly List<RelationSet> relationSets;

        private NodeSet FirstMatch(NodeDescription nodeDescription)
        {
            var nodeBlocks = DbFetcher.SelectNodeBlocksByLabelAndProperties(nodeDescription.Label, nodeDescription.Props);
            
            var nodeSet = new NodeSet();

            foreach (var nodeBlock in nodeBlocks)
            {
                nodeSet.Nodes.Add(new Node(nodeBlock, db));
            }

            return nodeSet;
        }


        private void BackwardCleanup(int relationSetId)
        {
            var relationSet = relationSets[relationSetId];
            var nextLayerNodes = nodeSets[relationSetId + 1].Nodes;

            var allRelationsList = relationSet.Relations.ToList();

            foreach (var candidateRelation in allRelationsList)
            {
                if (!nextLayerNodes.Contains(relationSet.Direction == RelationsDirection.Right? candidateRelation.To : candidateRelation.From))
                {
                    relationSet.Relations.Remove(candidateRelation);
                }
            }


            var newPreviousLayerNodes = new HashSet<Node>();

            foreach (var goodRelation in relationSet.Relations)
            {
                newPreviousLayerNodes.Add(relationSet.Direction == RelationsDirection.Right
                    ? goodRelation.From
                    : goodRelation.To);
            }

            nodeSets[relationSetId].Nodes = newPreviousLayerNodes;


            if (relationSetId > 0)
            {
                BackwardCleanup(relationSetId - 1);
            }
        }



        




        public NodeSet Match(NodeDescription nodeDescription)
        {
            if (nodeSets.Count == 0 && relationSets.Count == 0)
            {
                var nodeSet = FirstMatch(nodeDescription);
                nodeSets.Add(nodeSet);
                return nodeSet;
            }

            if (nodeSets.Count != relationSets.Count)
                throw new Exception("There cannot be 2 consecutive calls to Match(...) for one Query");


            var lastRelationSet = relationSets.Last();

            var newLastNodeSet = new NodeSet();

            
            foreach (var relation in lastRelationSet.Relations)
            {
                var candidateNode = lastRelationSet.Direction == RelationsDirection.Right ? relation.To : relation.From;

                if (nodeDescription.CheckNode(candidateNode))
                {
                    newLastNodeSet.Nodes.Add(candidateNode);
                }

            }
            
            nodeSets.Add(newLastNodeSet);
            return newLastNodeSet;
        }







        public RelationSet To(RelationDescription relationDescription)
        {
            if (nodeSets.Count != relationSets.Count + 1)
            {
                throw new Exception("To/From must be executed after Match");
            }

            var lastNodeLayer = nodeSets.Last().Nodes;


            var goodRelations = new RelationSet(RelationsDirection.Right);


            foreach (var node in lastNodeLayer)
            {
                node.PullOutRelations();

                foreach (var outRelation in node.OutRelations)
                {
                    if (relationDescription.CheckRelation(outRelation))
                    {
                        goodRelations.Relations.Add(outRelation);
                    }
                }
            }

            relationSets.Add(goodRelations);
            return goodRelations;
        }



        public RelationSet From(RelationDescription relationDescription)
        {
            if (nodeSets.Count != relationSets.Count + 1)
            {
                throw new Exception("To/From must be executed after Match");
            }

            var lastNodeLayer = nodeSets.Last().Nodes;


            var goodRelations = new RelationSet(RelationsDirection.Left);


            foreach (var node in lastNodeLayer)
            {
                node.PullInRelations();

                foreach (var inRelation in node.InRelations)
                {
                    if (relationDescription.CheckRelation(inRelation))
                    {
                        goodRelations.Relations.Add(inRelation);
                    }
                }
            }

            relationSets.Add(goodRelations);
            return goodRelations;
        }



        public void Execute()
        {
            if (nodeSets.Count != relationSets.Count + 1)
            {
                throw new Exception("Query cannot end with To/From, please add one more Match");
            }

            if (relationSets.Count > 0)
                BackwardCleanup(relationSets.Count-1);
        }
    }
}
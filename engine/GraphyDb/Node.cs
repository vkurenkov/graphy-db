using System;
using System.Collections.Generic;
using GraphyDb.IO;

namespace GraphyDb
{
    public class Node : Entity, IEquatable<Node>
    {
        public int NodeId;

        public int LabelId;
        public string Label;

        public readonly Dictionary<string, NodeProperty> Properties;

        public List<Relation> OutRelations;
        public List<Relation> InRelations;

        private readonly NodeBlock nodeBlock;

        public Node(string label, DbEngine db, EntityState state)
        {
            NodeId = 0;
            LabelId = 0;
            Label = label;
            Db = db;

            Properties = new Dictionary<string, NodeProperty>();
            OutRelations = new List<Relation>();
            InRelations = new List<Relation>();


            State = state;
            if (state != EntityState.Unchanged)
                Db.ChangedEntities.Add(this);

            nodeBlock = null;
        }


        public Node(NodeBlock nodeBlock, DbEngine db, EntityState state = EntityState.Unchanged)
        {
            NodeId = nodeBlock.NodeId;
            LabelId = nodeBlock.LabelId;

            Label = IO.dbControl.GetDbReader().ReadGenericStringBlock(DbControl.LabelPath, LabelId).Data;
            Db = db;


            Properties = new Dictionary<string, NodeProperty>();


            var propertyBlock = dbControl.GetDbReader().ReadPropertyBlock(DbControl.NodePropertyPath, nodeBlock.FirstPropertyId);

            while (propertyBlock.PropertyId != 0)
            {
                if (!propertyBlock.Used)
                {
                    propertyBlock = dbControl.GetDbReader().ReadPropertyBlock(DbControl.NodePropertyPath, propertyBlock.NextPropertyId);
                    continue;
                }

                var property = new NodeProperty(this, propertyBlock);
                Properties.Add(property.Key, property);

                propertyBlock = dbControl.GetDbReader().ReadPropertyBlock(DbControl.NodePropertyPath, propertyBlock.NextPropertyId);
            }


            OutRelations = null;
            InRelations = null;
            this.nodeBlock = nodeBlock;
        }


        public void PullOutRelations()
        {
            OutRelations = new List<Relation>();

            var outRelationBlock = dbControl.GetDbReader().ReadRelationBlock(nodeBlock.FirstOutRelationId);

            while (outRelationBlock.RelationId != 0)
            {
                if (!outRelationBlock.Used)
                {
                    outRelationBlock = dbControl.GetDbReader().ReadRelationBlock(outRelationBlock.FirstNodeNextRelation);
                    continue;
                }

                var relation = new Relation(this, null, outRelationBlock);

                OutRelations.Add(relation);

                outRelationBlock = dbControl.GetDbReader().ReadRelationBlock(outRelationBlock.FirstNodeNextRelation);
            }

        }

        public void PullInRelations()
        {
            InRelations = new List<Relation>();

            var inRelationBlock = dbControl.GetDbReader().ReadRelationBlock(nodeBlock.FirstInRelationId);

            while (inRelationBlock.RelationId != 0)
            {
                if (!inRelationBlock.Used)
                {
                    inRelationBlock = dbControl.GetDbReader().ReadRelationBlock(inRelationBlock.SecondNodeNextRelation);
                    continue;
                }

                var relation = new Relation(null, this, inRelationBlock);

                InRelations.Add(relation);

                inRelationBlock = dbControl.GetDbReader().ReadRelationBlock(inRelationBlock.SecondNodeNextRelation);
            }

        }



        public void DeleteProperty(string key)
        {
            Properties.TryGetValue(key, out var property);
            property?.Delete();
            Properties.Remove(key);
        }




        public object this[string key]
        {
            get => Properties[key].Value;

            set
            {
                if (Properties.TryGetValue(key, out var property))
                {
                    // modifying existing property:
                    property.Value = value;
                }
                else
                {
                    // adding new property:
                    Properties[key] = new NodeProperty(this, key, value);
                }
            }
        }


        public bool Equals(Node other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return NodeId == other.NodeId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Node)obj);
        }

        public override int GetHashCode()
        {
            return NodeId;
        }

        public static bool operator ==(Node left, Node right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Node left, Node right)
        {
            return !Equals(left, right);
        }

    }
}
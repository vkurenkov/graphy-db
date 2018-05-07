using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphyDb;
using System.Linq;

namespace GraphyDbTests
{
    [TestClass]
    public class QueryTests
    {
        /// <summary>
        /// Used for Thread.Sleep due to eventual consistency properties of the database
        /// </summary>
        private const int ConsistencyDelayMs = 200;

        [TestInitialize]
        public void DropDatabase()
        {
            var engine = new DbEngine();
            engine.DropDatabase();
        }

        [TestCategory("Querying"), TestMethod]
        public void Match_Node_By_Label()
        {
            const string Label = "Primitive";
            var engine = new DbEngine();

            engine.AddNode(Label);
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First().Label, Label);
        }

        [TestCategory("Querying"), TestMethod]
        public void Match_Two_Nodes_By_Label()
        {
            const string Label = "Primitive";
            var engine = new DbEngine();

            engine.AddNode(Label);
            engine.AddNode(Label);
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.Distinct().Count(), 2);
        }

        [TestCategory("Querying"), TestMethod]
        public void Match_Node_By_Label_And_Property()
        {
            const string Label = "Primitive";
            const string PropertyName = "PropertyName";
            const string PropertyValue = "value";
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            node[PropertyName] = PropertyValue;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var targetNodeProps = new Dictionary<string, object>();
            targetNodeProps.Add(PropertyName, PropertyValue);
            var foundNodes = query.Match(new NodeDescription(Label, targetNodeProps));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First().Label, Label);
        }

        [TestCategory("Querying"), TestMethod]
        public void Match_Node_By_Label_And_2_Properties()
        {
            const string Label = "Primitive";
            const string Property1Name = "PropertyName";
            const string Property1Value = "value";
            const string Property2Name = "PropertyName";
            const string Property2Value = "value";
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            node[Property1Name] = Property1Value;
            node[Property2Name] = Property2Value;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var targetNodeProps = new Dictionary<string, object>();
            targetNodeProps.Add(Property1Name, Property1Value);
            targetNodeProps.Add(Property2Name, Property1Value);
            var foundNodes = query.Match(new NodeDescription(Label, targetNodeProps));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First().Label, Label);
        }

        [TestCategory("Querying"), TestMethod]
        public void Match_Node_By_Label_And_Relation()
        {
            const string Node1Label = "Primitive";
            const string Node2Label = "Primitive1";
            const string RelationLabel = "Relation";
            const string PropertyName = "PropertyName";
            const string PropertyValue = "value";
            var engine = new DbEngine();

            var node1 = engine.AddNode(Node1Label);
            var node2 = engine.AddNode(Node2Label);
            var relation = engine.AddRelation(node1, node2, RelationLabel);
            engine.AddRelation(node1, node2, RelationLabel + "Other");
            relation[PropertyName] = PropertyValue;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var nodes1 = query.Match(new NodeDescription(Node1Label));

            var targetRelationsProps = new Dictionary<string, object>();
            targetRelationsProps.Add(PropertyName, PropertyValue);
            var relations = query.To(new RelationDescription(RelationLabel, targetRelationsProps));

            var nodes2 = query.Match(new NodeDescription(Node2Label));
            query.Execute();
            Assert.AreEqual(relations.Relations.Count, 1);
            Assert.AreEqual(nodes1.Nodes.First().Label, Node1Label);
        }
    }
}

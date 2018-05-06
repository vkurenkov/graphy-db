using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphyDb;
using System.Linq;

namespace GraphyDbTests
{
    [TestClass]
    public class DbEngineTests
    {
        /// <summary>
        /// Used for Thread.Sleep due to eventual consistency properties of the database
        /// </summary>
        private const int ConsistencyDelayMs = 1000;

        [TestInitialize]
        public void DropDatabase()
        {
            var engine = new DbEngine();
            engine.DropDatabase();
        }

        [TestMethod]
        public void Add_Node_No_Properties()
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

        [TestMethod]
        public void Add_Node_With_Property()
        {
            const string Label = "Primitive";
            const string Color = "Red";
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            node["Color"] = Color;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First()["Color"], Color);
        }

        [TestMethod]
        public void Delete_Node()
        {
            const string Label = "Primitive";
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);
            engine.Delete(node);
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.Count, 0);
        }

        [TestMethod]
        public void Delete_Node_Should_Remove_Its_Relations()
        {
            const string RelationLabel = "Left";
            const string FromNodeLabel = "Primitive";
            const string ToNodeLabel = "Primitive1";
            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(FromNodeLabel);
            var nodeTo = engine.AddNode(ToNodeLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, RelationLabel);
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);
            engine.Delete(nodeFrom);
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var nodes = query.Match(new NodeDescription(ToNodeLabel));
            query.Execute();
            var node = nodes.Nodes.First();
            node.ResolveRelations();
            Assert.AreEqual(node.InRelations.Count, 0);
        }

        [TestMethod]
        public void Add_Relation_No_Properties()
        {
            const string LeftLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            engine.AddRelation(nodeFrom, nodeTo, LeftLabel);
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes1 = query.Match(new NodeDescription(NodeFromLabel));
            var foundRelations = query.To(new RelationDescription(LeftLabel));
            var foundNodes2 = query.Match(new NodeDescription(NodeToLabel));
            query.Execute();
            Assert.AreEqual(foundRelations.Relations.First().Label, LeftLabel);
        }

        [TestMethod]
        public void Add_Relation_With_Property()
        {
            const string LeftLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, LeftLabel);
            relation["Distance"] = 0.5f; // f is neccessary ??
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes1 = query.Match(new NodeDescription(NodeFromLabel));
            var foundRelations = query.To(new RelationDescription(LeftLabel));
            var foundNodes2 = query.Match(new NodeDescription(NodeToLabel));
            query.Execute();
            Assert.AreEqual(foundRelations.Relations.First()["Distance"], 0.5f);
        }

        [TestMethod]
        public void Delete_Relation()
        {
            const string RelationLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, RelationLabel);
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);
            engine.Delete(relation);
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var nodes = query.Match(new NodeDescription(NodeFromLabel));
            query.Execute();
            var node = nodes.Nodes.First();
            node.ResolveRelations();

            var query1 = new Query(engine);
            var nodes1 = query1.Match(new NodeDescription(NodeToLabel));
            query1.Execute();
            var node1 = nodes1.Nodes.First();
            node1.ResolveRelations();

            Assert.AreEqual(node.OutRelations.Count, 0);
            Assert.AreEqual(node1.InRelations.Count, 1);
        }

        [TestMethod]
        public void Edit_Node_Property()
        {

        }

        [TestMethod]
        public void Edit_Relation_Property()
        {

        }
    }
}

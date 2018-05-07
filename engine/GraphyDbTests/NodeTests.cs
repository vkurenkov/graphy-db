using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphyDb;
using System.Linq;

namespace GraphyDbTests
{
    [TestClass]
    public class NodeTests
    {
        /// <summary>
        /// Used for Thread.Sleep due to eventual consistency properties of the database
        /// </summary>
        private const int ConsistencyDelayMs = 0;

        [TestInitialize]
        public void DropDatabase()
        {
            var engine = new DbEngine();
            engine.DropDatabase();
        }

        #region Node Operations
        [TestCategory("Nodes"), TestMethod]
        public void Add_Node()
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

        [TestCategory("Nodes"), TestMethod]
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

        [TestCategory("Nodes"), TestCategory("Relations"), TestMethod]
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
            node.PullInRelations();
            Assert.AreEqual(node.InRelations.Count, 0);
        }
        #endregion

        #region Node Properties
        [TestCategory("Properties"), TestCategory("Nodes"), TestMethod]
        public void Add_String_Property_To_Node()
        {
            const string Label = "Primitive";
            const string PropertyName = "StringProperty";
            const string PropertyValue = "some value";
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            node[PropertyName] = PropertyValue;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First()[PropertyName], PropertyValue);
        }

        [TestCategory("Properties"), TestCategory("Nodes"), TestMethod]
        public void Add_Float_Property_To_Node()
        {
            const string Label = "Primitive";
            const string PropertyName = "FloatProperty";
            const float PropertyValue = 5.01f;
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            node[PropertyName] = PropertyValue;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First()[PropertyName], PropertyValue);
        }

        [TestCategory("Properties"), TestCategory("Nodes"), TestMethod]
        public void Add_Int_Property_To_Node()
        {
            const string Label = "Primitive";
            const string PropertyName = "IntProperty";
            const int PropertyValue = 5;
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            node[PropertyName] = PropertyValue;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First()[PropertyName], PropertyValue);
        }

        [TestCategory("Properties"), TestCategory("Nodes"), TestMethod]
        public void Add_Boolean_Property_To_Node()
        {
            const string Label = "Primitive";
            const string PropertyName = "BooleanProperty";
            const bool PropertyValue = true;
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            node[PropertyName] = PropertyValue;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First()[PropertyName], PropertyValue);
        }

        [TestCategory("Properties"), TestCategory("Nodes"), TestMethod]
        public void Delete_Property_From_Node()
        {
            const string NodeLabel = "Primitive";
            const string PropertyName = "Property1";
            const string PropertyValue = "PropertyValue";
            var engine = new DbEngine();

            var node = engine.AddNode(NodeLabel);
            node[PropertyName] = PropertyValue;
            engine.SaveChanges();
            node.DeleteProperty(PropertyName);
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var nodes = query.Match(new NodeDescription(NodeLabel));
            Assert.IsTrue(!nodes.Nodes.First().Properties.Keys.Contains(PropertyName));
        }

        [TestCategory("Nodes"), TestCategory("Properties"), TestMethod]
        public void Edit_Int_Property_Node()
        {
            const string Label = "Primitive";
            const string PropertyName = "IntProperty";
            const int PropertyValueBefore = 1;
            const int PropertyValueAfter = 5;
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            node[PropertyName] = PropertyValueBefore;
            node[PropertyName] = PropertyValueAfter;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First()[PropertyName], PropertyValueAfter);
        }

        [TestCategory("Nodes"), TestCategory("Properties"), TestMethod]
        public void Edit_Float_Property_Node()
        {
            const string Label = "Primitive";
            const string PropertyName = "FloatProperty";
            const float PropertyValueBefore = 1.21f;
            const float PropertyValueAfter = 5.21f;
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            node[PropertyName] = PropertyValueBefore;
            node[PropertyName] = PropertyValueAfter;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First()[PropertyName], PropertyValueAfter);
        }

        [TestCategory("Nodes"), TestCategory("Properties"), TestMethod]
        public void Edit_String_Property_Node()
        {
            const string Label = "Primitive";
            const string PropertyName = "FloatProperty";
            const string PropertyValueBefore = "before";
            const string PropertyValueAfter = "after";
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            node[PropertyName] = PropertyValueBefore;
            node[PropertyName] = PropertyValueAfter;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First()[PropertyName], PropertyValueAfter);
        }

        [TestCategory("Nodes"), TestCategory("Properties"), TestMethod]
        public void Edit_Boolean_Property_Node()
        {
            const string Label = "Primitive";
            const string PropertyName = "FloatProperty";
            const bool PropertyValueBefore = true;
            const bool PropertyValueAfter = false;
            var engine = new DbEngine();

            var node = engine.AddNode(Label);
            node[PropertyName] = PropertyValueBefore;
            node[PropertyName] = PropertyValueAfter;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes = query.Match(new NodeDescription(Label));
            query.Execute();
            Assert.AreEqual(foundNodes.Nodes.First()[PropertyName], PropertyValueAfter);
        }
        #endregion
    }
}

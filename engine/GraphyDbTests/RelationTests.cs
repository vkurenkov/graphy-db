using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GraphyDb;
using System.Linq;

namespace GraphyDbTests
{
    [TestClass]
    public class RelationTests
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

        #region Relation Operations
        [TestCategory("Relations"), TestMethod]
        public void Add_Relation()
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

        [TestCategory("Relations"), TestMethod]
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
            node.PullOutRelations();

            var query1 = new Query(engine);
            var nodes1 = query1.Match(new NodeDescription(NodeToLabel));
            query1.Execute();
            var node1 = nodes1.Nodes.First();
            node1.PullInRelations();

            Assert.AreEqual(node.OutRelations.Count, 0);
            Assert.AreEqual(node1.InRelations.Count, 1);
        }
        #endregion

        #region Relation Properties
        [TestCategory("Properties"), TestCategory("Relations"), TestMethod]
        public void Add_Float_Property_To_Relation()
        {
            const string LeftLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            const string PropertyName = "FloatProperty";
            const float PropertyValue = 5.01f;

            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, LeftLabel);
            relation[PropertyName] = PropertyValue;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes1 = query.Match(new NodeDescription(NodeFromLabel));
            var foundRelations = query.To(new RelationDescription(LeftLabel));
            var foundNodes2 = query.Match(new NodeDescription(NodeToLabel));
            query.Execute();
            Assert.AreEqual(foundRelations.Relations.First()[PropertyName], PropertyValue);
        }

        [TestCategory("Properties"), TestCategory("Relations"), TestMethod]
        public void Add_String_Property_To_Relation()
        {
            const string LeftLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            const string PropertyName = "StringProperty";
            const string PropertyValue = "PropertyValue";

            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, LeftLabel);
            relation[PropertyName] = PropertyValue;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes1 = query.Match(new NodeDescription(NodeFromLabel));
            var foundRelations = query.To(new RelationDescription(LeftLabel));
            var foundNodes2 = query.Match(new NodeDescription(NodeToLabel));
            query.Execute();
            Assert.AreEqual(foundRelations.Relations.First()[PropertyName], PropertyValue);
        }

        [TestCategory("Properties"), TestCategory("Relations"), TestMethod]
        public void Add_Int_Property_To_Relation()
        {
            const string LeftLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            const string PropertyName = "IntProperty";
            const int PropertyValue = 5;

            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, LeftLabel);
            relation[PropertyName] = PropertyValue;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes1 = query.Match(new NodeDescription(NodeFromLabel));
            var foundRelations = query.To(new RelationDescription(LeftLabel));
            var foundNodes2 = query.Match(new NodeDescription(NodeToLabel));
            query.Execute();
            Assert.AreEqual(foundRelations.Relations.First()[PropertyName], PropertyValue);
        }

        [TestCategory("Properties"), TestCategory("Relations"), TestMethod]
        public void Add_Boolean_Property_To_Relation()
        {
            const string LeftLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            const string PropertyName = "BooleanProperty";
            const bool PropertyValue = true;

            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, LeftLabel);
            relation[PropertyName] = PropertyValue;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes1 = query.Match(new NodeDescription(NodeFromLabel));
            var foundRelations = query.To(new RelationDescription(LeftLabel));
            var foundNodes2 = query.Match(new NodeDescription(NodeToLabel));
            query.Execute();
            Assert.AreEqual(foundRelations.Relations.First()[PropertyName], PropertyValue);
        }

        [TestCategory("Properties"), TestCategory("Relations"), TestMethod]
        public void Delete_Property_From_Relation()
        {
            const string LeftLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            const string PropertyName = "IntProperty";
            const int PropertyValue = 2;

            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, LeftLabel);
            relation[PropertyName] = PropertyValue;
            relation.DeleteProperty(PropertyName);
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes1 = query.Match(new NodeDescription(NodeFromLabel));
            var foundRelations = query.To(new RelationDescription(LeftLabel));
            var foundNodes2 = query.Match(new NodeDescription(NodeToLabel));
            query.Execute();
            Assert.IsTrue(!foundRelations.Relations.First().Properties.Keys.Contains(PropertyName));
        }

        [TestCategory("Properties"), TestCategory("Relations"), TestMethod]
        public void Edit_Int_Property_Relation()
        {
            const string LeftLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            const string PropertyName = "IntProperty";
            const int PropertyValueAfter = 1;
            const int PropertyValueBefore = 5;

            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, LeftLabel);
            relation[PropertyName] = PropertyValueBefore;
            relation[PropertyName] = PropertyValueAfter;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes1 = query.Match(new NodeDescription(NodeFromLabel));
            var foundRelations = query.To(new RelationDescription(LeftLabel));
            var foundNodes2 = query.Match(new NodeDescription(NodeToLabel));
            query.Execute();
            Assert.AreEqual(foundRelations.Relations.First()[PropertyName], PropertyValueAfter);
        }

        [TestCategory("Properties"), TestCategory("Relations"), TestMethod]
        public void Edit_Float_Property_Relation()
        {
            const string LeftLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            const string PropertyName = "FloatProperty";
            const float PropertyValueAfter = 1.21f;
            const float PropertyValueBefore = 5.21f;

            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, LeftLabel);
            relation[PropertyName] = PropertyValueBefore;
            relation[PropertyName] = PropertyValueAfter;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes1 = query.Match(new NodeDescription(NodeFromLabel));
            var foundRelations = query.To(new RelationDescription(LeftLabel));
            var foundNodes2 = query.Match(new NodeDescription(NodeToLabel));
            query.Execute();
            Assert.AreEqual(foundRelations.Relations.First()[PropertyName], PropertyValueAfter);
        }

        [TestCategory("Properties"), TestCategory("Relations"), TestMethod]
        public void Edit_String_Property_Relation()
        {
            const string LeftLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            const string PropertyName = "StringProperty";
            const string PropertyValueAfter = "before";
            const string PropertyValueBefore = "after";

            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, LeftLabel);
            relation[PropertyName] = PropertyValueBefore;
            relation[PropertyName] = PropertyValueAfter;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes1 = query.Match(new NodeDescription(NodeFromLabel));
            var foundRelations = query.To(new RelationDescription(LeftLabel));
            var foundNodes2 = query.Match(new NodeDescription(NodeToLabel));
            query.Execute();
            Assert.AreEqual(foundRelations.Relations.First()[PropertyName], PropertyValueAfter);
        }

        [TestCategory("Properties"), TestCategory("Relations"), TestMethod]
        public void Edit_Boolean_Property_Relation()
        {
            const string LeftLabel = "Left";
            const string NodeFromLabel = "Primitive";
            const string NodeToLabel = "Primitive1";
            const string PropertyName = "BooleanProperty";
            const bool PropertyValueAfter = true;
            const bool PropertyValueBefore = false;

            var engine = new DbEngine();

            var nodeFrom = engine.AddNode(NodeFromLabel);
            var nodeTo = engine.AddNode(NodeToLabel);
            var relation = engine.AddRelation(nodeFrom, nodeTo, LeftLabel);
            relation[PropertyName] = PropertyValueBefore;
            relation[PropertyName] = PropertyValueAfter;
            engine.SaveChanges();
            Thread.Sleep(ConsistencyDelayMs);

            var query = new Query(engine);
            var foundNodes1 = query.Match(new NodeDescription(NodeFromLabel));
            var foundRelations = query.To(new RelationDescription(LeftLabel));
            var foundNodes2 = query.Match(new NodeDescription(NodeToLabel));
            query.Execute();
            Assert.AreEqual(foundRelations.Relations.First()[PropertyName], PropertyValueAfter);
        }
        #endregion
    }
}

using System;
using System.Threading;
using System.Collections.Generic;
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

        #region Querying
        [TestCategory("Querying"), TestMethod]
        public void Match_Node_By_Label()
        {
            // The same logic applies
            Add_Node();
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
        #endregion Querying
    }
}

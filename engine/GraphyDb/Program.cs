using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using GraphyDb.IO;

namespace GraphyDb
{
    public class Program
    {
        private static readonly TraceSource MySource = new TraceSource("TraceGraphyDb");

        private static void Main(string[] args)
        {
//            var nodesList = new List<Node>();
//            for (var i = 0; i < 100; ++i) nodesList.Add(engine.AddNode("type1"));
//            for (var i = 0; i < 100; ++i) nodesList.Add(engine.AddNode("type2"));
//            for (var i = 0; i < 100; ++i) nodesList.Add(engine.AddNode("type3"));
//            for (var i = 0; i < 300; ++i) nodesList[i]["prop1"] = 10;
//            for (var i = 0; i < 300; ++i) nodesList[i]["prop2"] = true;
//            for (var i = 0; i < 300; ++i) nodesList[i]["prop3"] = 11.2;
//            for (var i = 0; i < 300; ++i) nodesList[i]["prop4"] = $"Alakazam{i}";


            var sw = new Stopwatch();
            var timesToRun = 20.0;
            for (var i = 0; i < timesToRun; ++i)
            {
                var engine = new DbEngine();
                engine.DropDatabase();
                Thread.Sleep(10);
                engine = new DbEngine();
                TestFullyConnectedHalfGraph(engine, 15);
                sw.Start();
                var query = new Query(engine);
                var nodeMatch1 = query.Match(NodeDescription.Any());
                var relationMatch = query.From(RelationDescription.Any());
                var node2Match = query.Match(NodeDescription.Any());
                query.Execute();
                sw.Stop();
                Console.WriteLine(
                    $"Iteration {i}: {sw.Elapsed.Milliseconds}ms, {nodeMatch1.Nodes.Count}:" +
                    $"{relationMatch.Relations.Count}:{node2Match.Nodes.Count}");
                Thread.Sleep(10);
                engine.DropDatabase();
            }

            Console.WriteLine($"{sw.Elapsed.TotalMilliseconds / timesToRun} ms per iteration");
            Console.ReadLine();
        }

        private static void Add100Nodes(DbEngine dbEngine, int run)
        {
            var nodesList = new List<Node>();
            for (var i = 0; i < 100; i++)
            {
                nodesList.Add(dbEngine.AddNode("node"));
            }

            dbEngine.SaveChanges();
        }

        private static void Add100Relations(DbEngine dbEngine, int run)
        {
            var node = dbEngine.AddNode("first");
            for (var i = 0; i < 100; i++)
            {
                dbEngine.AddRelation(node, node, "edge");
            }

            dbEngine.SaveChanges();
        }

        private static void TestFullyConnectedGraph(DbEngine dbEngine, int nodesNum)
        {
            var nodesList = new List<Node>();
            for (var i = 0; i < nodesNum; i++)
            {
                nodesList.Add(dbEngine.AddNode("node"));
            }

            for (var i = 0; i < nodesNum; i++)
            {
                for (var j = 0; i < nodesNum; i++)
                {
                    var relation = dbEngine.AddRelation(nodesList[i], nodesList[j], "edge");
                }
            }

            dbEngine.SaveChanges();
        }

        private static void TestFullyConnectedHalfGraph(DbEngine dbEngine, int nodesNum)
        {
            var nodesList = new List<Node>();
            for (var i = 0; i < nodesNum; i++)
            {
                nodesList.Add(dbEngine.AddNode($"node{i % 2}"));
            }

            for (var i = 0; i < nodesNum; i++)
            {
                for (var j = 0; j < nodesNum; j++)
                {
                    var relation = dbEngine.AddRelation(nodesList[i], nodesList[j], $"edge{i % 2}{j % 2}");
                }
            }

            dbEngine.SaveChanges();
        }
    }
}
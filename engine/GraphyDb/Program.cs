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
            var timesToRun = 5.0;
            for (var i = 0; i < timesToRun; ++i)
            {
                var engine = new DbEngine();
                sw.Start();
                TestFullyConnectedGraphWithUniqueLabels(engine, 50);
                sw.Stop();
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

        private static void TestFullyConnectedGraphWithUniqueLabels(DbEngine dbEngine, int nodesNum)
        {
            var nodesList = new List<Node>();
            for (var i = 0; i < nodesNum; i++)
            {
                nodesList.Add(dbEngine.AddNode($"node{i}"));
            }

            for (var i = 0; i < nodesNum; i++)
            {
                for (var j = 0; i < nodesNum; i++)
                {
                    var relation = dbEngine.AddRelation(nodesList[i], nodesList[j], $"edge{i}{j}");
                }
            }

            dbEngine.SaveChanges();
        }
    }
}
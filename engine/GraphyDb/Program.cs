using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using System.Threading;
using GraphyDb.IO;

namespace GraphyDb
{
    public class Program
    {
        private static readonly TraceSource MySource = new TraceSource("TraceGraphyDb");

        private static void Main(string[] args)
        {
            // Sample benchmark which builds a graph => query it
            var queryStopWatch = new Stopwatch();
            var lastTimeQuery = queryStopWatch.Elapsed.TotalMilliseconds;
            var buildStopwatch = new Stopwatch();
            var lastTimeBuild = queryStopWatch.Elapsed.TotalMilliseconds;
            var timesToRun = 10.0;
            for (var iteration = 0; iteration < timesToRun; ++iteration)
            {
                var engine = new DbEngine();
                buildStopwatch.Start();
                Primitives2DBuildBenchmark(engine,
                    10); // Genereate fully connected graph where each node is connected with others by 2 relations and has 4 properties.
                buildStopwatch.Stop();
                queryStopWatch.Start();
                Primitives2DQueryBenchmark(engine); 
                queryStopWatch.Stop();
                Console.WriteLine(
                    $"Iteration {iteration}\t Build: {buildStopwatch.Elapsed.TotalMilliseconds - lastTimeBuild:##.000}ms\t Query: {queryStopWatch.Elapsed.TotalMilliseconds - lastTimeQuery:##.000}ms");
                lastTimeQuery = queryStopWatch.Elapsed.TotalMilliseconds;
                lastTimeBuild = buildStopwatch.Elapsed.TotalMilliseconds;
                engine.DropDatabase(); // Clean files before the next measure
            }

            Console.WriteLine(
                $"Build: {buildStopwatch.Elapsed.TotalMilliseconds / timesToRun:##.000} ms per iteration");
            Console.WriteLine(
                $"Query: {queryStopWatch.Elapsed.TotalMilliseconds / timesToRun:##.000} ms per iteration");
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

        private static void Add100NodesWith100Properties(DbEngine dbEngine, int run)
        {
            var nodesList = new List<Node>();
            for (var i = 0; i < 100; i++)
            {
                nodesList.Add(dbEngine.AddNode("node"));
                for (var j = 0; j < 25; j++)
                {
                    nodesList[i]["floatProp"] = 1.23 * i;
                    nodesList[i]["strProp"] = $"44{i}";
                    nodesList[i]["intProp"] = 123 * i - i;
                    nodesList[i]["boolProp"] = i % 2;
                }
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

        private static void Primitives2DBuildBenchmark(DbEngine dbEngine, int nodesNum)
        {
            Random rnd = new Random();
            var shapeProp = new List<string>() {"box", "circle", "triangle"};
            var colorProp = new List<string>() {"green", "red", "purple"};
            var nodesList = new List<Node>();
            for (var i = 0; i < nodesNum; i++)
            {
                nodesList.Add(dbEngine.AddNode($"node"));
                nodesList[i]["shape"] = shapeProp[rnd.Next(shapeProp.Count)];
                nodesList[i]["color"] = colorProp[rnd.Next(colorProp.Count)];
                nodesList[i]["x"] = (float) (rnd.NextDouble() + rnd.NextDouble() * 10 - rnd.NextDouble() * 10);
                nodesList[i]["y"] = (float) (rnd.NextDouble() + rnd.NextDouble() * 10 - rnd.NextDouble() * 10);
            }

            for (var i = 0; i < nodesNum; i++)
            {
                for (var j = 0; j < nodesNum; j++)
                {
                    var relation1 = dbEngine.AddRelation(nodesList[i], nodesList[j],
                        $"xAxis{rnd.Next(3)}"); // less, greater, equal
                    var relation2 = dbEngine.AddRelation(nodesList[i], nodesList[j],
                        $"yAxis{rnd.Next(3)}"); // less, greater, equal
                }
            }

            dbEngine.SaveChanges();
        }

        private static void Primitives2DQueryBenchmark(DbEngine dbEngine)
        {
            var query = new Query(dbEngine);
            var nodeMatch1 =
                query.Match(new NodeDescription("node", new Dictionary<string, object>() {{"shape", "box"}}));
            var relationMatch = query.From(new RelationDescription("xAxis0"));
            var node2Match =
                query.Match(new NodeDescription("node", new Dictionary<string, object>() {{"shape", "circle"}}));
            query.Execute();
        }
    }
}
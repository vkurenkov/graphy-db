using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using GraphyDb;

class Program
{
    private static DbEngine GraphDatabase = new DbEngine();
    private static HashSet<PrimitiveObject> SeenObjects = new HashSet<PrimitiveObject>();
    private static TraceSource Logger = new TraceSource("Logger");

    private static void Main(string[] args)
    {
        ExtractSeenObjects();

        var port = 9001;
        var server = new UdpClient(port);
        Logger.TraceInformation("Server is started.");

        for (int i = 0; ; i++)
        {
            var client = new IPEndPoint(IPAddress.Any, port);
            var data = server.Receive(ref client);
            HandleMessage(data, server, client);
        }
    }

    private static void HandleMessage(byte[] message, UdpClient server, IPEndPoint client)
    {
        // Check whether we can handle the message
        if (message[0] >= Enum.GetNames(typeof(MessageType)).Length)
        {
            Logger.TraceInformation("Message type (" + message[0].ToString() + " is not supported.)");
            return;
        }

        // First byte is the message type
        var messageType = (MessageType)message[0];
        var decodedMessage = Encoding.UTF8.GetString(message.Skip(1).ToArray());

        if (messageType == MessageType.ParseObjects)
        {
            //Logger.TraceInformation("Parse objects message is received.");
            var deserializedMessage = JsonConvert.DeserializeObject<ParseObjectsMessage>(decodedMessage);
            PutObjectsToDatabase(deserializedMessage);
        }
        else if(messageType == MessageType.GetSideObjects)
        {
            Logger.TraceInformation("Get side objects message is received.");
            var deserializedMessage = JsonConvert.DeserializeObject<GetSideObjects>(decodedMessage);
            var sideObjects = GetSideObjects(deserializedMessage);

            if(sideObjects == null)
            {
                var answer = Encoding.UTF8.GetBytes("Internal Server Error");
                server.Send(answer, answer.Length, client);
            }
            else
            {
                var answer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sideObjects));
                server.Send(answer, answer.Length, client);
            }

            Console.Write(deserializedMessage);
        }
    }

    private static void PutObjectsToDatabase(ParseObjectsMessage message)
    {
        var duration = new Stopwatch();
        duration.Start();

        int numNewObjects = 0;
        foreach(var obj in message.Objects)
        {
            if (!SeenObjects.Contains(obj))
            {
                var node = GraphDatabase.AddNode("Primitive");
                node["X"] = obj.X;
                node["Y"] = obj.Y;
                node["Shape"] = obj.Shape;
                node["Color"] = obj.Color;
                CreateRelations(node);

                SeenObjects.Add(obj);
                numNewObjects++;
            }
        }

        duration.Stop();
        if (numNewObjects != 0)
        {
            Logger.TraceInformation(numNewObjects.ToString() + " new primitives added to the graph database.");
            Logger.TraceInformation("It took " + duration.Elapsed);
        }
    }
    private static void CreateRelations(Node node)
    {
        var nodeX = (float)node["X"];
        var nodeY = (float)node["Y"];

        var query = new Query(GraphDatabase);
        var seenNodes = query.Match(new NodeDescription("Primitive"));
        query.Execute();
        foreach (var seenNode in seenNodes.Nodes)
        {
            var seenNodeProps = seenNode.Properties;
            var seenNodeX = (float)seenNodeProps["X"].Value;
            var seenNodeY = (float)seenNodeProps["Y"].Value;

            if (seenNodeX < nodeX)
            {
                GraphDatabase.AddRelation(seenNode, node, "Right");
                GraphDatabase.AddRelation(node, seenNode, "Left");
            }
            else if(seenNodeX > nodeX)
            {
                GraphDatabase.AddRelation(seenNode, node, "Left");
                GraphDatabase.AddRelation(node, seenNode, "Right");
            }
            else
            {
                GraphDatabase.AddRelation(seenNode, node, "EqualX");
                GraphDatabase.AddRelation(node, node, "EqualX");
            }

            if (seenNodeY < nodeY)
            {
                GraphDatabase.AddRelation(seenNode, node, "Up");
                GraphDatabase.AddRelation(node, seenNode, "Down");
            }
            else if (seenNodeY > nodeY)
            {
                GraphDatabase.AddRelation(seenNode, node, "Down");
                GraphDatabase.AddRelation(node, seenNode, "Up");
            }
            else
            {
                GraphDatabase.AddRelation(seenNode, node, "EqualY");
                GraphDatabase.AddRelation(node, node, "EqualY");
            }
        }

        GraphDatabase.SaveChanges();
    }
    private static void ExtractSeenObjects()
    {
        var query = new Query(GraphDatabase);
        var seenNodes = query.Match(NodeDescription.Any());
        query.Execute();

        foreach(var seenNode in seenNodes.Nodes)
        {
            SeenObjects.Add(new PrimitiveObject
            {
                Color = (string)seenNode["Color"],
                Shape = (string)seenNode["Shape"],
                X = (float)seenNode["X"],
                Y = (float)seenNode["Y"]
            });
        }
    }

    private static List<PrimitiveObject> GetSideObjects(GetSideObjects message)
    {
        /* Form properties based on the provided message */
        var targetNodeProps = new Dictionary<String, object>();
        if (!String.IsNullOrWhiteSpace(message.Color))
            targetNodeProps.Add("Color", message.Color);
        if (!String.IsNullOrWhiteSpace(message.Shape))
            targetNodeProps.Add("Shape", message.Shape);
        if (!float.IsNaN(message.PositionX))
            targetNodeProps.Add("X", message.PositionX);
        if (!float.IsNaN(message.PositionY))
            targetNodeProps.Add("Y", message.PositionY);

        string side;
        if (message.Side == 0) side = "Left";
        else if (message.Side == 1) side = "Right";
        else if (message.Side == 2) side = "Up";
        else side = "Down";

        /* Run a query on the graph database */
        var query = new Query(GraphDatabase);
        query.Match(new NodeDescription("Primitive", targetNodeProps));
        query.To(new RelationDescription(side));
        var foundNodes = query.Match(new NodeDescription("Primitive"));
        query.Execute();

        var sideObjects = new List<PrimitiveObject>();
        foreach(var obj in foundNodes.Nodes)
        {
            sideObjects.Add(new PrimitiveObject()
            {
                Shape = (string)obj.Properties["Shape"].Value,
                Color = (string)obj.Properties["Color"].Value,
                X = (float)obj.Properties["X"].Value,
                Y = (float)obj.Properties["Y"].Value
            });
        }

        return sideObjects;
    }
}
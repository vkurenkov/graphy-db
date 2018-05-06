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
    private static bool GraphIsOn = false;
    private static HashSet<PrimitiveObject> SeenObjects = new HashSet<PrimitiveObject>();
    private static TraceSource Logger = new TraceSource("Logger");

    private static void Main(string[] args)
    {
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
            Logger.TraceInformation("Parse objects message is received.");
            var deserializedMessage = JsonConvert.DeserializeObject<ParseObjectsMessage>(decodedMessage);
            Logger.TraceData(TraceEventType.Information, 0, deserializedMessage.Objects[0]);
            PutObjectsToDatabase(deserializedMessage);
        }
        else if(messageType == MessageType.GetSideObjects)
        {
            Logger.TraceInformation("Get side objects message is received.");
            var deserializedMessage = JsonConvert.DeserializeObject<GetSideObjects>(decodedMessage);
            var neighbors = GetNeighbors(deserializedMessage);

            if(neighbors == null)
            {
                var answer = Encoding.UTF8.GetBytes("Okay, I've received your message but there was an internal error!");
                server.Send(answer, answer.Length, client);
            }
            else
            {

            }

            Console.Write(deserializedMessage);
        }
        else if(messageType == MessageType.Between)
        {
            throw new NotImplementedException();
        }
    }

    private static void PutObjectsToDatabase(ParseObjectsMessage message)
    {
        if (!GraphIsOn) return;

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

        GraphDatabase.SaveChanges();
        Logger.TraceInformation(numNewObjects.ToString() + " new primitives added to the graph database.");
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

    private static List<PrimitiveObject> GetNeighbors(GetSideObjects message)
    {
        if (!GraphIsOn) return null;
        throw new NotImplementedException();
    }
}
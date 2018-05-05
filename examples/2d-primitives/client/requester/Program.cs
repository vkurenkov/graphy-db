using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

class Program
{
    private const string Hostname = "127.0.0.1";
    private const int Port = 9001;
    private const string NoValue = "";

    private static void Main(string[] args)
    {
        var client = new UdpClient();
        var remote = new IPEndPoint(IPAddress.Parse(Hostname), Port);

        while(true)
        {
            Console.WriteLine("\n############################# REQUEST #############################");
            Console.WriteLine("What kind of request do you want to send? (0 - GetNeighbors, 1 - Between)");
            var line = Console.ReadLine();
            if (String.IsNullOrEmpty(line))
            {
                Console.WriteLine("Please, specify the request type. Abort.");
                continue;
            }

            var requestType = Convert.ToInt32(line);
            if (requestType == 0) SendGetNeighbors(client, remote);
            else if (requestType == 1) SendBetween(client, remote);
            else Console.WriteLine("Unknown request type. Abort.");
        }
    }

    private static void SendGetNeighbors(UdpClient client, IPEndPoint remote)
    {
        Console.WriteLine("What's the color of your figure? (You can press enter if it is not relevant)");
        var color = Console.ReadLine();
        Console.WriteLine("What's the shape of your figure? (You can press enter if it is not relevant)");
        var shape = Console.ReadLine();
        if (color == NoValue && shape == NoValue)
        {
            Console.WriteLine("At least one of the parameters must be specified. Abort.");
            return;
        }

        Console.WriteLine("In what range should the neighbors lie?");
        var rangeLine = Console.ReadLine();
        if(String.IsNullOrEmpty(rangeLine))
        {
            Console.WriteLine("Range must be specified. Abort.");
            return;
        }
        var range = Convert.ToInt32(rangeLine);
        if(range <= 0)
        {
            Console.WriteLine("Range must lie in (0; +inf). Abort.");
            return;
        }


        var message = new GetNeighborsMessage();
        message.Shape = shape;
        message.Color = color;
        message.Range = range;

        var jsonMessage = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        var messageBytes = new byte[jsonMessage.Length + 1];
        messageBytes[0] = (byte)MessageType.GetNeighbors;
        Array.Copy(jsonMessage, 0, messageBytes, 1, jsonMessage.Length);

        var stopWatch = new Stopwatch();

        client.Send(messageBytes, messageBytes.Length, remote);
        Console.WriteLine("Request is sent.");
        Console.WriteLine("Waiting for an answer...");

        stopWatch.Start();
        var answerBytes = client.Receive(ref remote);
        stopWatch.Stop();

        Console.WriteLine("Answer is received after " + stopWatch.Elapsed);
    }
    private static void SendBetween(UdpClient client, IPEndPoint remote)
    {
        throw new NotImplementedException();
    }
}

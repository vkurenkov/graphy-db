using System;
using System.Globalization;
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
            Console.WriteLine("What kind of request do you want to send? (0 - GetSideObjects, 1 - Between)");
            var line = Console.ReadLine();
            if (String.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("Please, specify the request type. Abort.");
                continue;
            }

            var requestType = Convert.ToInt32(line);
            if (requestType == 0) SendSideObjects(client, remote);
            else if (requestType == 1) SendBetween(client, remote);
            else Console.WriteLine("Unknown request type. Abort.");
        }
    }

    private static void SendSideObjects(UdpClient client, IPEndPoint remote)
    {
        Console.WriteLine("What's the color of your figure? (You can press enter if it is not relevant)");
        var color = Console.ReadLine();
        Console.WriteLine("What's the shape of your figure? (You can press enter if it is not relevant)");
        var shape = Console.ReadLine();
        Console.WriteLine("What's the X position of your figure? (You can press enter if it is not relevant)");
        var positionX = Console.ReadLine();
        Console.WriteLine("What's the Y position of your figure? (You can press enter if it is not relevant)");
        var positionY = Console.ReadLine();
        if (color == NoValue && shape == NoValue && positionX == NoValue && positionY == NoValue)
        {
            Console.WriteLine("At least one of the parameters must be specified. Abort.");
            return;
        }

        Console.WriteLine("At which side we should look for objects? (0 - Left, 1 - Right, 2 - Up, 3 - Down)");
        var sideLine = Console.ReadLine();
        if(String.IsNullOrWhiteSpace(sideLine))
        {
            Console.WriteLine("Side must be specified. Abort.");
            return;
        }
        var side = Convert.ToByte(sideLine);

        var message = new GetSideObjects();
        message.Side = side;
        if (!String.IsNullOrWhiteSpace(color)) message.Color = color;
        if (!String.IsNullOrWhiteSpace(shape)) message.Shape = shape;
        if (!String.IsNullOrWhiteSpace(positionX)) message.PositionX = float.Parse(positionX, CultureInfo.InvariantCulture);

        else message.PositionX = float.NaN;
        if (!String.IsNullOrEmpty(positionY)) message.PositionY = float.Parse(positionY, CultureInfo.InvariantCulture);
        else message.PositionY = float.NaN;

        var jsonMessage = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        var messageBytes = new byte[jsonMessage.Length + 1];
        messageBytes[0] = (byte)MessageType.GetSideObjects;
        Array.Copy(jsonMessage, 0, messageBytes, 1, jsonMessage.Length);

        var stopWatch = new Stopwatch();

        client.Send(messageBytes, messageBytes.Length, remote);
        Console.WriteLine("Request is sent.");
        Console.WriteLine("Waiting for an answer...");

        try
        {
            stopWatch.Start();
            var answerBytes = client.Receive(ref remote);
            stopWatch.Stop();
        }
        catch(Exception e)
        {
            Console.WriteLine("Request is not handled");
            Console.WriteLine(e);
        }
    }
    private static void SendBetween(UdpClient client, IPEndPoint remote)
    {
        throw new NotImplementedException();
    }
}

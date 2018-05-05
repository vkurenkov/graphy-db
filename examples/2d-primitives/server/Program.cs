using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

class Program
{
    private static void Main(string[] args)
    {
        var port = 9001;
        var server = new UdpClient(port);

        for (int i = 0; ; i++)
        {
            var client = new IPEndPoint(IPAddress.Any, port);
            var data = server.Receive(ref client);
            HandleMessage(data);
        }
    }

    private static void HandleMessage(byte[] message)
    {
        if (message[0] >= Enum.GetNames(typeof(MessageType)).Length)
        {
            Console.WriteLine("Message type (" + message[0].ToString() + " is not supported.)");
            return;
        }

        // First byte is the message type
        var messageType = (MessageType)message[0];
        var decodedMessage = Encoding.UTF8.GetString(message.Skip(1).ToArray());

        if (messageType == MessageType.ParseObjects)
        {
            var deserializedMessage = JsonConvert.DeserializeObject<ParseObjectsMessage>(decodedMessage);
            Console.WriteLine(deserializedMessage.Objects[0]);
        }
        else if(messageType == MessageType.GetNeighbors)
        {
            throw new NotImplementedException();
        }
        else if(messageType == MessageType.Between)
        {
            throw new NotImplementedException();
        }
    }
}
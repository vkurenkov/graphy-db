using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;

public class UDPSend : MonoBehaviour
{
	public float m_sendingRate = 0.03f;
	public ObjectsManager m_objectsManager;

    private void FixedUpdate()
    {

		if (Time.time - m_lastSendTime < m_sendingRate) {
			return;
		}

		string jsonString = buildJSONFromObjects (m_objectsManager.GetVisible());
	    var objectsBytes = UTF8Encoding.UTF8.GetBytes (jsonString);

        var message = new byte[objectsBytes.Length + 1];
        message[0] = 0; // The line sets up the message type (ParseObjects)
        System.Array.Copy(objectsBytes, 0, message, 1, objectsBytes.Length);

		m_client.Send (message, message.Length, m_remote);
		m_lastSendTime = Time.time;
	}
    private string buildJSONFromObjects(List<PrimitiveObject> objects)
    {
		var stringObjects = new List<string> ();

        foreach (var o in objects)
        {
            stringObjects.Add(o.ToJson());
        }

		return string.Format("{{\"Objects\":[{0}]}}", string.Join (",", stringObjects.ToArray()));
	}

    private void Start()
    {
        m_remote = new IPEndPoint(IPAddress.Parse(IP), port);
        m_client = new UdpClient();
        m_lastSendTime = 0.0f;
    }

    private string IP = "127.0.0.1";  // define in init
    private int port = 9001;        // define in init

    // "connection" things
    private IPEndPoint m_remote;
    private UdpClient m_client;

    private float m_lastSendTime;
}

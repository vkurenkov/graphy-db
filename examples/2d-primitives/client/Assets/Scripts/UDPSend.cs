using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class UDPSend : MonoBehaviour
{
    string IP = "127.0.0.1";  // define in init
    int port =   9001;        // define in init

    // "connection" things
    IPEndPoint m_Remote;
    UdpClient m_Client;

	float m_LastSendTime;
	public float m_SendingRate = 0.03f;

	public ObjectsManager m_ObjectsManager;

    private void FixedUpdate()
    {

		if (Time.time - m_LastSendTime < m_SendingRate) {
			return;
		}

		string jsonString = buildJSONFromObjects (m_ObjectsManager.GetVisible());
		byte[] bytes = UTF8Encoding.UTF8.GetBytes (jsonString);

		m_Client.Send (bytes, bytes.Length, m_Remote);
		m_LastSendTime = Time.time;
	}

    private string buildJSONFromObjects(List<PrimitiveObject> objects)
    {
		var stringObjects = new List<string> ();

        foreach (var o in objects)
        {
            stringObjects.Add(o.ToJson());
        }

		return string.Format("{{\"data\":[{0}]}}", string.Join (",", stringObjects.ToArray()));
	}

    private void Start()
    {
        Init();
        m_LastSendTime = 0.0f;
    }
    private void Init()
    {
		m_Remote= new IPEndPoint(IPAddress.Parse(IP), port);
		m_Client = new UdpClient();
    }
}
